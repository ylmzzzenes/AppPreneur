using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Syllabus;
using UniFlow.Business.Dtos;
using UniFlow.Business.Helpers;
using UniFlow.Business.Scheduling;
using UniFlow.DataAccess.Persistence;
using UniFlow.DataAccess.Queries;
using UniFlow.DataAccess.UnitOfWork;
using UniFlow.Entity.Entities;
using UniFlow.Entity.Results;
using SyllabusEntity = UniFlow.Entity.Entities.Syllabus;

namespace UniFlow.Business.Services;

public sealed class SyllabusIngestionService(
    ISyllabusFileValidationService fileValidationService,
    ISyllabusScanSessionQueries scanSessionQueries,
    ICourseQueries courseQueries,
    IUnitOfWork unitOfWork,
    UniFlowDbContext dbContext,
    IOcrService ocrService,
    ISyllabusParsingService syllabusParsingService,
    ITaskPriorityCalculator taskPriorityCalculator,
    ILogger<SyllabusIngestionService> logger) : ISyllabusIngestionService
{
    public async Task<Result<SyllabusScanResponse>> ScanAsync(
        long userId,
        string courseCode,
        string courseTitle,
        SyllabusUploadInput? upload,
        CancellationToken cancellationToken = default)
    {
        var code = courseCode.Trim();
        var title = courseTitle.Trim();
        if (code.Length == 0 || title.Length == 0)
        {
            return Result<SyllabusScanResponse>.Fail("SYLLABUS_METADATA", "Course code and title are required.");
        }

        var parseResult = await ParseUploadAsync(upload, cancellationToken).ConfigureAwait(false);
        if (!parseResult.IsSuccess || parseResult.Data is null)
        {
            return Result<SyllabusScanResponse>.Fail(
                parseResult.Error?.Code ?? "SYLLABUS_PARSE_FAILED",
                parseResult.Error?.Message ?? "Syllabus parsing failed.");
        }

        var parsed = parseResult.Data;
        var sourceSummary = SyllabusScanHelper.BuildSourceSummary(parsed.SourceText);
        var detectedItems = parsed.Drafts.Select(SyllabusScanHelper.ToDetectedItem).ToList();

        string previewJson;
        try
        {
            previewJson = SyllabusScanHelper.SerializePreview(new SyllabusScanPreviewPayload
            {
                SourceSummary = sourceSummary,
                DetectedItems = detectedItems,
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Syllabus scan preview payload too large for user {UserId}.", userId);
            return Result<SyllabusScanResponse>.Fail(
                "SYLLABUS_PARSE_FAILED",
                "Scan preview is too large. Reduce the number of detected items and try again.");
        }

        var now = DateTime.UtcNow;
        var scanId = Guid.NewGuid();
        var session = new SyllabusScanSession
        {
            Id = scanId,
            UserId = userId,
            CourseCode = code,
            CourseTitle = title,
            SourceTextHash = SyllabusScanHelper.ComputeSourceTextHash(parsed.SourceText),
            PreviewJson = previewJson,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(SyllabusScanConstants.SessionExpiryMinutes),
        };

        scanSessionQueries.Add(session);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<SyllabusScanResponse>.Success(new SyllabusScanResponse
        {
            ScanId = scanId,
            CourseCode = code,
            CourseTitle = title,
            DetectedItems = detectedItems,
            SourceSummary = sourceSummary,
            ExpiresAt = session.ExpiresAt,
        });
    }

    public async Task<Result<SyllabusIngestionResult>> ConfirmAsync(
        long userId,
        SyllabusConfirmRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await scanSessionQueries.GetByIdForUpdateAsync(request.ScanId, cancellationToken)
            .ConfigureAwait(false);

        if (session is null)
        {
            return Result<SyllabusIngestionResult>.Fail(
                "SYLLABUS_SCAN_NOT_FOUND",
                "Scan session was not found.");
        }

        if (session.UserId != userId)
        {
            return Result<SyllabusIngestionResult>.Fail(
                "SYLLABUS_SCAN_FORBIDDEN",
                "You do not have access to this scan session.");
        }

        if (session.ConfirmedAt is not null)
        {
            return Result<SyllabusIngestionResult>.Fail(
                "SYLLABUS_SCAN_ALREADY_CONFIRMED",
                "This scan session has already been confirmed.");
        }

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            return Result<SyllabusIngestionResult>.Fail(
                "SYLLABUS_SCAN_EXPIRED",
                "Scan session has expired. Please scan the syllabus again.");
        }

        var items = request.Items?.Where(i => !string.IsNullOrWhiteSpace(i.Title)).ToList() ?? [];
        if (items.Count == 0)
        {
            return Result<SyllabusIngestionResult>.Fail(
                "SYLLABUS_CONFIRM_EMPTY_ITEMS",
                "At least one task item is required to confirm.");
        }

        var code = request.CourseCode.Trim();
        var title = request.CourseTitle.Trim();
        if (code.Length == 0 || title.Length == 0)
        {
            return Result<SyllabusIngestionResult>.Fail("SYLLABUS_METADATA", "Course code and title are required.");
        }

        var preview = SyllabusScanHelper.DeserializePreview(session.PreviewJson);
        var drafts = items.Select(SyllabusScanHelper.ToDraft).ToList();
        drafts.ApplyPriorityScores(taskPriorityCalculator);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var course = await courseQueries.FindByUserAndCodeAsync(userId, code, cancellationToken)
                .ConfigureAwait(false);

            if (course is null)
            {
                course = new Course
                {
                    UserId = userId,
                    Code = code,
                    Title = title,
                };
                unitOfWork.Repository<Course>().Add(course);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var syllabus = new SyllabusEntity
            {
                CourseId = course.Id,
                Title = title,
                SourceText = string.IsNullOrEmpty(preview.SourceSummary) ? null : preview.SourceSummary,
            };

            foreach (var draft in drafts)
            {
                syllabus.Tasks.Add(new TaskItem
                {
                    Title = draft.Title,
                    Description = draft.Description,
                    DueDate = draft.DueDate,
                    Category = draft.Category,
                    PriorityScore = draft.PriorityScore,
                });
            }

            unitOfWork.Repository<SyllabusEntity>().Add(syllabus);

            session.ConfirmedAt = DateTime.UtcNow;
            session.CourseCode = code;
            session.CourseTitle = title;

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result<SyllabusIngestionResult>.Success(new SyllabusIngestionResult
            {
                CourseId = course.Id,
                SyllabusId = syllabus.Id,
                TaskCount = drafts.Count,
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Syllabus confirm failed for scan {ScanId} and user {UserId}.", request.ScanId, userId);
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    [Obsolete("Use ScanAsync followed by ConfirmAsync. This endpoint will be removed in a future version.")]
    public async Task<Result<SyllabusIngestionResult>> IngestAsync(
        long userId,
        string courseCode,
        string courseTitle,
        SyllabusUploadInput? upload,
        CancellationToken cancellationToken = default)
    {
        var scanResult = await ScanAsync(userId, courseCode, courseTitle, upload, cancellationToken)
            .ConfigureAwait(false);
        if (!scanResult.IsSuccess || scanResult.Data is null)
        {
            return Result<SyllabusIngestionResult>.Fail(
                scanResult.Error?.Code ?? "SYLLABUS_PARSE_FAILED",
                scanResult.Error?.Message ?? "Syllabus scan failed.");
        }

        var scan = scanResult.Data;
        return await ConfirmAsync(
                userId,
                new SyllabusConfirmRequest
                {
                    ScanId = scan.ScanId,
                    CourseCode = scan.CourseCode,
                    CourseTitle = scan.CourseTitle,
                    Items = scan.DetectedItems,
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<Result<ParsedSyllabusContent>> ParseUploadAsync(
        SyllabusUploadInput? upload,
        CancellationToken cancellationToken)
    {
        var fileResult = await fileValidationService.ValidateAndReadAsync(upload, cancellationToken)
            .ConfigureAwait(false);
        if (!fileResult.IsSuccess || fileResult.Data is null)
        {
            return Result<ParsedSyllabusContent>.Fail(
                fileResult.Error?.Code ?? "SYLLABUS_FILE_REQUIRED",
                fileResult.Error?.Message ?? "Syllabus file validation failed.");
        }

        var validatedFile = fileResult.Data;

        var ocrResult = await ocrService
            .ExtractTextAsync(validatedFile.Content, validatedFile.ContentType, cancellationToken)
            .ConfigureAwait(false);
        if (!ocrResult.IsSuccess || ocrResult.Data is null)
        {
            return Result<ParsedSyllabusContent>.Fail(
                ocrResult.Error?.Code ?? "OCR_FAILED",
                ocrResult.Error?.Message ?? "OCR failed.");
        }

        var parseResult = await syllabusParsingService
            .ParseTasksFromSyllabusTextAsync(ocrResult.Data, cancellationToken)
            .ConfigureAwait(false);
        if (!parseResult.IsSuccess || parseResult.Data is null)
        {
            return Result<ParsedSyllabusContent>.Fail(
                parseResult.Error?.Code ?? "SYLLABUS_PARSE_FAILED",
                parseResult.Error?.Message ?? "Syllabus parsing failed.");
        }

        var drafts = parseResult.Data.ToList();
        drafts.ApplyPriorityScores(taskPriorityCalculator);

        return Result<ParsedSyllabusContent>.Success(new ParsedSyllabusContent(ocrResult.Data, drafts));
    }

    private sealed record ParsedSyllabusContent(string SourceText, IList<SyllabusTaskDraft> Drafts);
}

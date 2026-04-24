using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Syllabus;
using UniFlow.Business.Scheduling;
using UniFlow.DataAccess.Queries;
using UniFlow.DataAccess.UnitOfWork;
using UniFlow.Entity.Entities;
using UniFlow.Entity.Results;
using SyllabusEntity = UniFlow.Entity.Entities.Syllabus;

namespace UniFlow.Business.Services;

public sealed class SyllabusIngestionService(
    ICourseQueries courseQueries,
    IUnitOfWork unitOfWork,
    IOcrService ocrService,
    ISyllabusParsingService syllabusParsingService,
    ITaskPriorityCalculator taskPriorityCalculator) : ISyllabusIngestionService
{
    public async Task<Result<SyllabusIngestionResult>> IngestAsync(
        long userId,
        string courseCode,
        string courseTitle,
        byte[] fileBytes,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        if (fileBytes.Length == 0)
        {
            return Result<SyllabusIngestionResult>.Fail("SYLLABUS_FILE_EMPTY", "Uploaded file is empty.");
        }

        var code = courseCode.Trim();
        var title = courseTitle.Trim();
        if (code.Length == 0 || title.Length == 0)
        {
            return Result<SyllabusIngestionResult>.Fail("SYLLABUS_METADATA", "Course code and title are required.");
        }

        var ocrResult = await ocrService.ExtractTextAsync(fileBytes, contentType, cancellationToken).ConfigureAwait(false);
        if (!ocrResult.IsSuccess || ocrResult.Data is null)
        {
            return Result<SyllabusIngestionResult>.Fail(
                ocrResult.Error?.Code ?? "OCR_FAILED",
                ocrResult.Error?.Message ?? "OCR failed.");
        }

        var parseResult = await syllabusParsingService.ParseTasksFromSyllabusTextAsync(ocrResult.Data, cancellationToken)
            .ConfigureAwait(false);
        if (!parseResult.IsSuccess || parseResult.Data is null)
        {
            return Result<SyllabusIngestionResult>.Fail(
                parseResult.Error?.Code ?? "PARSE_FAILED",
                parseResult.Error?.Message ?? "Syllabus parsing failed.");
        }

        var drafts = parseResult.Data.ToList();
        drafts.ApplyPriorityScores(taskPriorityCalculator);

        Course course;
        var existing = await courseQueries.FindByUserAndCodeAsync(userId, code, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            course = existing;
        }
        else
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
            SourceText = ocrResult.Data,
        };

        foreach (var d in drafts)
        {
            syllabus.Tasks.Add(new TaskItem
            {
                Title = d.Title,
                Description = d.Description,
                DueDate = d.DueDate,
                Category = d.Category,
                PriorityScore = d.PriorityScore,
            });
        }

        unitOfWork.Repository<SyllabusEntity>().Add(syllabus);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<SyllabusIngestionResult>.Success(new SyllabusIngestionResult
        {
            CourseId = course.Id,
            SyllabusId = syllabus.Id,
            TaskCount = drafts.Count,
        });
    }
}

using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniFlow.API.Configuration;
using UniFlow.API.Contracts;
using UniFlow.API.Infrastructure;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Syllabus;
using UniFlow.Entity.Results;

namespace UniFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class SyllabusController(ISyllabusIngestionService ingestionService) : ControllerBase
{
    /// <summary>
    /// Scans a syllabus file and returns a preview without persisting courses or tasks.
    /// </summary>
    [HttpPost("scan")]
    [EnableRateLimiting(RateLimitPolicies.Upload)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(SyllabusUploadConstants.MaxFileSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = SyllabusUploadConstants.MaxFileSizeBytes)]
    [ProducesResponseType(typeof(Result<SyllabusScanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<SyllabusScanResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RateLimitResponse), StatusCodes.Status429TooManyRequests)]
    public Task<IActionResult> Scan([FromForm] SyllabusIngestFormRequest form, CancellationToken cancellationToken) =>
        ExecuteUploadAsync(
            (userId, upload, ct) => ingestionService.ScanAsync(userId, form.CourseCode, form.CourseTitle, upload, ct),
            form,
            cancellationToken);

    /// <summary>
    /// Confirms a prior scan and persists course, syllabus, and task records in a single transaction.
    /// </summary>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(Result<SyllabusIngestionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<SyllabusIngestionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<SyllabusIngestionResult>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Result<SyllabusIngestionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        [FromBody] SyllabusConfirmRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await ingestionService.ConfirmAsync(userId, request, cancellationToken).ConfigureAwait(false);
        return ToConfirmActionResult(result);
    }

    /// <summary>
    /// One-step ingest (scan + confirm). Prefer <c>scan</c> and <c>confirm</c> for preview/edit flow.
    /// </summary>
    [HttpPost("ingest")]
    [EnableRateLimiting(RateLimitPolicies.Upload)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(SyllabusUploadConstants.MaxFileSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = SyllabusUploadConstants.MaxFileSizeBytes)]
    [ProducesResponseType(typeof(Result<SyllabusIngestionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RateLimitResponse), StatusCodes.Status429TooManyRequests)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use POST /api/v1/syllabus/scan then POST /api/v1/syllabus/confirm.")]
    public Task<IActionResult> Ingest([FromForm] SyllabusIngestFormRequest form, CancellationToken cancellationToken) =>
        ExecuteUploadAsync(
            (userId, upload, ct) => ingestionService.IngestAsync(userId, form.CourseCode, form.CourseTitle, upload, ct),
            form,
            cancellationToken,
            mapResult: ToIngestActionResult);

    private async Task<IActionResult> ExecuteUploadAsync<T>(
        Func<long, SyllabusUploadInput?, CancellationToken, Task<Result<T>>> action,
        SyllabusIngestFormRequest form,
        CancellationToken cancellationToken,
        Func<Result<T>, IActionResult>? mapResult = null)
    {
        Stream? fileStream = null;
        SyllabusUploadInput? upload = null;

        if (form.File is not null)
        {
            fileStream = form.File.OpenReadStream();
            upload = new SyllabusUploadInput
            {
                Content = fileStream,
                FileName = form.File.FileName,
                ContentType = form.File.ContentType,
                DeclaredLength = form.File.Length,
            };
        }

        try
        {
            var userId = HttpUser.GetUserId(User);
            var result = await action(userId, upload, cancellationToken).ConfigureAwait(false);
            var mapper = mapResult ?? (r => r.IsSuccess ? new OkObjectResult(r) : new BadRequestObjectResult(r));
            return mapper(result);
        }
        finally
        {
            if (fileStream is not null)
            {
                await fileStream.DisposeAsync().ConfigureAwait(false);
            }
        }
    }

    private static IActionResult ToConfirmActionResult(Result<SyllabusIngestionResult> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result);
        }

        return result.Error?.Code switch
        {
            "SYLLABUS_SCAN_NOT_FOUND" => new NotFoundObjectResult(result),
            "SYLLABUS_SCAN_FORBIDDEN" => new ObjectResult(result) { StatusCode = StatusCodes.Status403Forbidden },
            _ => new BadRequestObjectResult(result),
        };
    }

    private static IActionResult ToIngestActionResult(Result<SyllabusIngestionResult> result) =>
        result.IsSuccess ? new OkObjectResult(result) : new BadRequestObjectResult(result);
}

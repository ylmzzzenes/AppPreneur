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
    [HttpPost("ingest")]
    [EnableRateLimiting(RateLimitPolicies.Upload)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(SyllabusUploadConstants.MaxFileSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = SyllabusUploadConstants.MaxFileSizeBytes)]
    [ProducesResponseType(typeof(Result<SyllabusIngestionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RateLimitResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Ingest(
        [FromForm] SyllabusIngestFormRequest form,
        CancellationToken cancellationToken)
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
            var result = await ingestionService.IngestAsync(
                    userId,
                    form.CourseCode,
                    form.CourseTitle,
                    upload,
                    cancellationToken)
                .ConfigureAwait(false);

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }
        finally
        {
            if (fileStream is not null)
            {
                await fileStream.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}

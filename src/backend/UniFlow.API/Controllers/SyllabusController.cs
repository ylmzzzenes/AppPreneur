using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Result<SyllabusIngestionResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Ingest(
        [FromForm] SyllabusIngestFormRequest form,
        CancellationToken cancellationToken)
    {
        if (form.File is null || form.File.Length == 0)
        {
            return BadRequest(Result<SyllabusIngestionResult>.Fail("SYLLABUS_FILE", "File is required."));
        }

        await using var ms = new MemoryStream();
        await form.File.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        var bytes = ms.ToArray();

        var userId = HttpUser.GetUserId(User);
        var result = await ingestionService.IngestAsync(
                userId,
                form.CourseCode,
                form.CourseTitle,
                bytes,
                form.File.ContentType,
                cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniFlow.API.Infrastructure;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Courses;
using UniFlow.Entity.Results;

namespace UniFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/courses")]
public sealed class CoursesController(ICourseService courseService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyList<CourseResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await courseService.ListAsync(userId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(Result<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CourseResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await courseService.GetAsync(userId, id, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess && result.Error?.Code == "COURSE_NOT_FOUND")
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CourseResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<CourseResponse>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await courseService.CreateAsync(userId, request, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(Result<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<CourseResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<CourseResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result<CourseResponse>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateCourseRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await courseService.UpdateAsync(userId, id, request, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await courseService.DeleteAsync(userId, id, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess && result.Error?.Code == "COURSE_NOT_FOUND")
        {
            return NotFound(result);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    private IActionResult ToActionResult(Result<CourseResponse> result)
    {
        if (!result.IsSuccess && result.Error?.Code == "COURSE_NOT_FOUND")
        {
            return NotFound(result);
        }

        if (!result.IsSuccess && result.Error?.Code == "COURSE_CODE_DUPLICATE")
        {
            return Conflict(result);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

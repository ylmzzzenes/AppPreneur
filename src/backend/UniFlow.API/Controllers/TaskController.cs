using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniFlow.API.Infrastructure;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Tasks;
using UniFlow.Entity.Enums;
using UniFlow.Entity.Results;

namespace UniFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/Task")]
[Route("api/v1/tasks")]
public sealed class TaskController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Result<IReadOnlyList<TaskItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await taskService.GetMyTasksAsync(userId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("today")]
    [ProducesResponseType(typeof(Result<TaskListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Today(CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await taskService.GetTodayTasksAsync(userId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(Result<IReadOnlyList<TaskItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upcoming(
        [FromQuery] int days = 7,
        [FromQuery] TaskItemStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await taskService.GetUpcomingTasksAsync(userId, days, status, cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(Result<TaskItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<TaskItemResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await taskService.GetMyTaskAsync(userId, id, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess && result.Error?.Code == "TASK_NOT_FOUND")
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result<TaskItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<TaskItemResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<TaskItemResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await taskService.CreateAsync(userId, request, cancellationToken).ConfigureAwait(false);
        return ToMutationResult(result);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(Result<TaskItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<TaskItemResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<TaskItemResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await taskService.UpdateAsync(userId, id, request, cancellationToken).ConfigureAwait(false);
        return ToMutationResult(result);
    }

    [HttpPatch("{id:long}/status")]
    [ProducesResponseType(typeof(Result<TaskItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<TaskItemResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<TaskItemResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        long id,
        [FromBody] TaskStatusUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await taskService.UpdateStatusAsync(userId, id, request, cancellationToken).ConfigureAwait(false);
        return ToMutationResult(result);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await taskService.DeleteAsync(userId, id, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess && result.Error?.Code == "TASK_NOT_FOUND")
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    private IActionResult ToMutationResult(Result<TaskItemResponse> result)
    {
        if (!result.IsSuccess && (result.Error?.Code == "TASK_NOT_FOUND" || result.Error?.Code == "COURSE_NOT_FOUND"))
        {
            return NotFound(result);
        }

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniFlow.API.Infrastructure;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Tasks;
using UniFlow.Entity.Results;

namespace UniFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
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

    /// <summary>
    /// Updates the status of a task owned by the authenticated user.
    /// </summary>
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
        if (!result.IsSuccess && result.Error?.Code == "TASK_NOT_FOUND")
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

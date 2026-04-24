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
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniFlow.API.Infrastructure;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Dashboard;
using UniFlow.Entity.Results;

namespace UniFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    /// <summary>
    /// Returns today's dashboard: top 3 tasks, counts, and AI persona mood.
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(Result<DashboardTodayResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Today(CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await dashboardService.GetTodayAsync(userId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniFlow.API.Configuration;
using UniFlow.API.Contracts;
using UniFlow.API.Infrastructure;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Ai;
using UniFlow.Entity.Results;

namespace UniFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/ai")]
[EnableRateLimiting(RateLimitPolicies.Ai)]
public sealed class AiController(
    IStudyPlanService studyPlanService,
    ITaskFeedbackService taskFeedbackService,
    IWeeklySummaryService weeklySummaryService) : ControllerBase
{
    [HttpPost("study-plan")]
    [ProducesResponseType(typeof(Result<StudyPlanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<StudyPlanResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<StudyPlanResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RateLimitResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GenerateStudyPlan(
        [FromBody] StudyPlanRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await studyPlanService.GenerateAsync(userId, request, cancellationToken).ConfigureAwait(false);

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

    [HttpPost("task-feedback")]
    [ProducesResponseType(typeof(Result<TaskFeedbackResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<TaskFeedbackResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RateLimitResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GenerateTaskFeedback(
        [FromBody] TaskFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await taskFeedbackService.GenerateAsync(userId, request, cancellationToken).ConfigureAwait(false);

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

    [HttpGet("weekly-summary")]
    [ProducesResponseType(typeof(Result<WeeklySummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RateLimitResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetWeeklySummary(CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await weeklySummaryService.GetAsync(userId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniFlow.API.Configuration;
using UniFlow.API.Contracts;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Chat;
using UniFlow.Entity.Results;

namespace UniFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[EnableRateLimiting(RateLimitPolicies.Ai)]
public sealed class ChatController(IChatService chatService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RateLimitResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Post([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        var result = await chatService.ReplyAsync(request.Message, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

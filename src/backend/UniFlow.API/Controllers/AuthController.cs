using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Auth;
using UniFlow.Entity.Results;

namespace UniFlow.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Registers a new user. Optional <c>personalityVibe</c> defaults to Friendly; optional <c>major</c> may be omitted.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, duplicateStatus: StatusCodes.Status409Conflict);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken).ConfigureAwait(false);
        return ToActionResult(result, unauthorized: true);
    }

    private IActionResult ToActionResult(Result<AuthResponse> result, int? duplicateStatus = null, bool unauthorized = false)
    {
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        var code = result.Error?.Code ?? "ERROR";
        if (unauthorized && code == "AUTH_INVALID")
        {
            return Unauthorized(result);
        }

        if (duplicateStatus is not null && code == "AUTH_DUPLICATE_EMAIL")
        {
            return StatusCode(duplicateStatus.Value, result);
        }

        return BadRequest(result);
    }
}

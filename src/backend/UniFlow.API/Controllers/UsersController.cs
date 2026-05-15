using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniFlow.API.Infrastructure;
using UniFlow.Business.Abstractions;
using UniFlow.Business.Contracts.Users;
using UniFlow.Entity.Results;

namespace UniFlow.API.Controllers;

/// <summary>
/// Authenticated user profile and onboarding.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Updates onboarding preferences (AI tone and optional major) for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Only the caller's profile is updated (user id from JWT). Send at least one field.
    /// Empty <c>major</c> is stored as null.
    /// </remarks>
    [HttpPatch("me/onboarding")]
    [ProducesResponseType(typeof(Result<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<UserProfileResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result<UserProfileResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOnboarding(
        [FromBody] OnboardingUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpUser.GetUserId(User);
        var result = await userService.UpdateOnboardingAsync(userId, request, cancellationToken)
            .ConfigureAwait(false);

        if (!result.IsSuccess && result.Error?.Code == "USER_NOT_FOUND")
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

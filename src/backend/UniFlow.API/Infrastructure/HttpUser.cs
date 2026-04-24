using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UniFlow.API.Infrastructure;

internal static class HttpUser
{
    public static long GetUserId(ClaimsPrincipal user)
    {
        var v = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (long.TryParse(v, out var id))
        {
            return id;
        }

        throw new UnauthorizedAccessException("Invalid user context.");
    }
}

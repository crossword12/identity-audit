using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityAudit.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetApplicationUserId(
        this ClaimsPrincipal principal)
    {
        var userId =
            principal.FindFirst(
                JwtRegisteredClaimNames.Sub)
                ?.Value
            ?? principal.FindFirst(
                ClaimTypes.NameIdentifier)
                ?.Value;

        return Guid.TryParse(
            userId,
            out var parsedUserId)
                ? parsedUserId
                : null;
    }
}
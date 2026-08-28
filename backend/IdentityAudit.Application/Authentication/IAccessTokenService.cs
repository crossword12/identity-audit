namespace IdentityAudit.Application.Authentication;

public interface IAccessTokenService
{
    AccessTokenResult CreateToken(
        AccessTokenRequest request);
}

public sealed record AccessTokenRequest(
    Guid UserId,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> Roles);

public sealed record AccessTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAt);
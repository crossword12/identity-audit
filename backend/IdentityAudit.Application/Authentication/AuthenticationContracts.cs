using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.Authentication;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Password { get; init; } = string.Empty;
}

public sealed record AuthenticatedUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    IReadOnlyCollection<string> Roles);

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    AuthenticatedUserDto User);
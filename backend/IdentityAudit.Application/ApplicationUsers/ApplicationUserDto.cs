namespace IdentityAudit.Application.ApplicationUsers;

public sealed record ApplicationUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsEnabled,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
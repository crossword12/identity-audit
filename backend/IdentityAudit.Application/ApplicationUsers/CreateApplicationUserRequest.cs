using System.ComponentModel.DataAnnotations;
using IdentityAudit.Application.Authentication;

namespace IdentityAudit.Application.ApplicationUsers;

public sealed class CreateApplicationUserRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string DisplayName { get; init; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 12)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string[] Roles { get; init; } =
    [
        ApplicationRoles.Reader
    ];
}
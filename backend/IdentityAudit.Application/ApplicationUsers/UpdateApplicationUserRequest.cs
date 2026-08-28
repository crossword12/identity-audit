using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.ApplicationUsers;

public sealed class UpdateApplicationUserRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 2)]
    public string DisplayName { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }
}
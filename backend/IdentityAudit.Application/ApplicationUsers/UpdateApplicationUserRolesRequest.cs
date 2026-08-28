using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.ApplicationUsers;

public sealed class UpdateApplicationUserRolesRequest
{
    [Required]
    [MinLength(1)]
    public string[] Roles { get; init; } = [];
}
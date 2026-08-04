using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.Roles;

public sealed class ImportRolesRequest
{
    [Required]
    public IReadOnlyCollection<CollectedRoleRequest> Roles
    {
        get;
        init;
    } = Array.Empty<CollectedRoleRequest>();
}
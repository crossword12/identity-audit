using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.Groups;

public sealed class ImportGroupsRequest
{
    [Required]
    public IReadOnlyCollection<CollectedGroupRequest> Groups
    {
        get;
        init;
    } = Array.Empty<CollectedGroupRequest>();
}
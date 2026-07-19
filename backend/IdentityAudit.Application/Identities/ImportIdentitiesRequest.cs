using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.Identities;

public sealed class ImportIdentitiesRequest
{
    [Required]
    public IReadOnlyCollection<CollectedIdentityRequest> Identities
    {
        get;
        init;
    } = Array.Empty<CollectedIdentityRequest>();
}
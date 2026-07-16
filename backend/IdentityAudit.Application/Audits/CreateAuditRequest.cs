using System.ComponentModel.DataAnnotations;

namespace IdentityAudit.Application.Audits;

public sealed class CreateAuditRequest
{
    [Required]
    public Guid TargetId { get; init; }
}
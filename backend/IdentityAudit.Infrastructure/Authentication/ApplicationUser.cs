using Microsoft.AspNetCore.Identity;
using IdentityAudit.Domain.Entities;

namespace IdentityAudit.Infrastructure.Authentication;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
        = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; }
        = DateTimeOffset.UtcNow;

    public ICollection<AuditLog> AuditLogs { get; set; }
    = new List<AuditLog>();
}
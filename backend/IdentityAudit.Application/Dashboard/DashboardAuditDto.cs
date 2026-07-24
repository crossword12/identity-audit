namespace IdentityAudit.Application.Dashboard;

public sealed class DashboardAuditDto
{
    public Guid AuditId { get; set; }

    public Guid TargetId { get; set; }

    public string TargetName { get; set; } = string.Empty;

    public string TargetType { get; set; } = string.Empty;

    public string AuditStatus { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public decimal? ComplianceScore { get; set; }

    public long TotalIdentities { get; set; }

    public long EnabledIdentities { get; set; }

    public long DisabledIdentities { get; set; }

    public long PrivilegedIdentities { get; set; }

    public long ServiceAccounts { get; set; }

    public long GuestAccounts { get; set; }

    public long LockedAccounts { get; set; }
}
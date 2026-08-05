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

    public long EvaluatedRules { get; set; }

    public long CompliantRules { get; set; }

    public long NonCompliantRules { get; set; }

    public long NotApplicableRules { get; set; }

    public long NotVerifiableRules { get; set; }

    public long ErrorRules { get; set; }

    public long TotalFindings { get; set; }

    public long CriticalFindings { get; set; }

    public long HighFindings { get; set; }

    public long MediumFindings { get; set; }

    public long LowFindings { get; set; }

    public long CisControl5Findings { get; set; }

    public long CisControl6Findings { get; set; }
}
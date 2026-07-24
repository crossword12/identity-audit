namespace IdentityAudit.Application.Dashboard;

public interface IDashboardService
{
    Task<IReadOnlyList<DashboardAuditDto>> GetByTargetIdAsync(
        Guid targetId,
        CancellationToken cancellationToken);
}
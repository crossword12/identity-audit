using IdentityAudit.Application.Dashboard;
using IdentityAudit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityAudit.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IdentityAuditDbContext _dbContext;

    public DashboardService(
        IdentityAuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DashboardAuditDto>>
        GetByTargetIdAsync(
            Guid targetId,
            CancellationToken cancellationToken)
    {
        var dashboard = await _dbContext.Database
            .SqlQuery<DashboardAuditDto>(
                $"""
                SELECT *
                FROM public."fn_GetTargetDashboard"(
                    {targetId}
                )
                """)
            .ToListAsync(cancellationToken);

        return dashboard;
    }
}
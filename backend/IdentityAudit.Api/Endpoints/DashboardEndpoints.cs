using IdentityAudit.Application.Dashboard;
using IdentityAudit.Application.Targets;

namespace IdentityAudit.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/dashboard")
            .WithTags("Dashboard");

        group.MapGet(
                "/targets/{targetId:guid}",
                GetByTargetIdAsync)
            .WithName("GetTargetDashboard");

        return endpoints;
    }

    private static async Task<IResult> GetByTargetIdAsync(
        Guid targetId,
        ITargetService targetService,
        IDashboardService dashboardService,
        CancellationToken cancellationToken)
    {
        var target = await targetService.GetByIdAsync(
            targetId,
            cancellationToken);

        if (target is null)
        {
            return Results.NotFound(new
            {
                message = "La cible demandée est introuvable."
            });
        }

        var dashboard =
            await dashboardService.GetByTargetIdAsync(
                targetId,
                cancellationToken);

        return Results.Ok(dashboard);
    }
}
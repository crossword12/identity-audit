using IdentityAudit.Api.Authorization;
using IdentityAudit.Application.AuditLogs;

namespace IdentityAudit.Api.Endpoints;

public static class AuditLogEndpoints
{
    public static IEndpointRouteBuilder
        MapAuditLogEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/audit-logs")
            .WithTags("Audit logs")
            .RequireAuthorization(
                AuthorizationPolicies.CanReadActivityLogs);

        group.MapGet("", GetAllAsync)
            .WithName("GetAllAuditLogs")
            .Produces<IReadOnlyList<AuditLogDto>>(
                StatusCodes.Status200OK)
            .Produces(
                StatusCodes.Status400BadRequest)
            .Produces(
                StatusCodes.Status401Unauthorized)
            .Produces(
                StatusCodes.Status403Forbidden);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        IAuditLogService auditLogService,
        int limit = 200,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 500)
        {
            return Results.BadRequest(
                new
                {
                    message =
                        "Le paramètre limit doit être compris entre 1 et 500."
                });
        }

        var auditLogs =
            await auditLogService.GetAllAsync(
                limit,
                cancellationToken);

        return Results.Ok(auditLogs);
    }
}
using IdentityAudit.Application.Audits;
using IdentityAudit.Application.Identities;
using IdentityAudit.Api.Authorization;

namespace IdentityAudit.Api.Endpoints;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/audits/{auditId:guid}/identities")
            .WithTags("Identities")
            .RequireAuthorization(
                AuthorizationPolicies.CanReadAuditData);

        group.MapGet("", GetByAuditIdAsync)
            .WithName("GetIdentitiesByAuditId");

        group.MapPost("", ImportAsync)
            .WithName("ImportIdentities")
            .RequireAuthorization(
                AuthorizationPolicies.CanManageAudits);

        return endpoints;
    }

    private static async Task<IResult> GetByAuditIdAsync(
        Guid auditId,
        IIdentityService identityService,
        IAuditService auditService,
        CancellationToken cancellationToken)
    {
        var audit = await auditService.GetByIdAsync(
            auditId,
            cancellationToken);

        if (audit is null)
        {
            return Results.NotFound(new
            {
                message = "L'audit demandé est introuvable."
            });
        }

        var identities =
            await identityService.GetByAuditIdAsync(
                auditId,
                cancellationToken);

        return Results.Ok(identities);
    }

    private static async Task<IResult> ImportAsync(
        Guid auditId,
        ImportIdentitiesRequest request,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        var result = await identityService.ImportAsync(
            auditId,
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new
            {
                message = result.ErrorMessage
            });
        }

        return Results.CreatedAtRoute(
            "GetIdentitiesByAuditId",
            new { auditId },
            new
            {
                importedCount = result.ImportedCount,
                identities = result.Identities
            });
    }
}
using IdentityAudit.Application.Audits;
using IdentityAudit.Application.Roles;
using IdentityAudit.Api.Authorization;

namespace IdentityAudit.Api.Endpoints;

public static class DirectoryRoleEndpoints
{
    public static IEndpointRouteBuilder
        MapDirectoryRoleEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/audits/{auditId:guid}/roles")
            .WithTags("Directory Roles")
            .RequireAuthorization(
                AuthorizationPolicies.CanReadAuditData);

        group.MapGet("", GetByAuditIdAsync)
            .WithName("GetDirectoryRolesByAuditId");

        group.MapPost("", ImportAsync)
            .WithName("ImportDirectoryRoles")
            .RequireAuthorization(
                AuthorizationPolicies.CanManageAudits);

        return endpoints;
    }

    private static async Task<IResult> GetByAuditIdAsync(
        Guid auditId,
        IDirectoryRoleService roleService,
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
                message =
                    "L'audit demandé est introuvable."
            });
        }

        var roles =
            await roleService.GetByAuditIdAsync(
                auditId,
                cancellationToken);

        return Results.Ok(roles);
    }

    private static async Task<IResult> ImportAsync(
        Guid auditId,
        ImportRolesRequest request,
        IDirectoryRoleService roleService,
        CancellationToken cancellationToken)
    {
        var result = await roleService.ImportAsync(
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
            "GetDirectoryRolesByAuditId",
            new { auditId },
            new
            {
                importedCount =
                    result.ImportedCount,

                roles =
                    result.Roles
            });
    }
}
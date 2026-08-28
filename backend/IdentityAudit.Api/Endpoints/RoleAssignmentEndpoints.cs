using IdentityAudit.Application.Audits;
using IdentityAudit.Application.RoleAssignments;
using IdentityAudit.Api.Authorization;

namespace IdentityAudit.Api.Endpoints;

public static class RoleAssignmentEndpoints
{
    public static IEndpointRouteBuilder
        MapRoleAssignmentEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup(
                "/api/audits/{auditId:guid}/role-assignments")
            .WithTags("Role Assignments")
            .RequireAuthorization(
                AuthorizationPolicies.CanReadAuditData);

        group.MapGet("", GetByAuditIdAsync)
            .WithName("GetRoleAssignmentsByAuditId");

        group.MapPost("", ImportAsync)
            .WithName("ImportRoleAssignments")
            .RequireAuthorization(
                AuthorizationPolicies.CanManageAudits);

        return endpoints;
    }

    private static async Task<IResult> GetByAuditIdAsync(
        Guid auditId,
        IRoleAssignmentService assignmentService,
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

        var assignments =
            await assignmentService.GetByAuditIdAsync(
                auditId,
                cancellationToken);

        return Results.Ok(assignments);
    }

    private static async Task<IResult> ImportAsync(
        Guid auditId,
        ImportRoleAssignmentsRequest request,
        IRoleAssignmentService assignmentService,
        CancellationToken cancellationToken)
    {
        var result = await assignmentService.ImportAsync(
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
            "GetRoleAssignmentsByAuditId",
            new { auditId },
            new
            {
                importedCount =
                    result.ImportedCount,

                assignments =
                    result.Assignments
            });
    }
}
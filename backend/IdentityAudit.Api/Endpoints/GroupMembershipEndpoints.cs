using IdentityAudit.Application.Audits;
using IdentityAudit.Application.GroupMemberships;
using IdentityAudit.Api.Authorization;

namespace IdentityAudit.Api.Endpoints;

public static class GroupMembershipEndpoints
{
    public static IEndpointRouteBuilder
        MapGroupMembershipEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup(
                "/api/audits/{auditId:guid}/group-memberships")
            .WithTags("Group Memberships")
            .RequireAuthorization(
                AuthorizationPolicies.CanReadAuditData);

        group.MapGet("", GetByAuditIdAsync)
            .WithName("GetGroupMembershipsByAuditId");

        group.MapPost("", ImportAsync)
            .WithName("ImportGroupMemberships")
            .RequireAuthorization(
                AuthorizationPolicies.CanManageAudits);

        return endpoints;
    }

    private static async Task<IResult> GetByAuditIdAsync(
        Guid auditId,
        IGroupMembershipService membershipService,
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

        var memberships =
            await membershipService.GetByAuditIdAsync(
                auditId,
                cancellationToken);

        return Results.Ok(memberships);
    }

    private static async Task<IResult> ImportAsync(
        Guid auditId,
        ImportGroupMembershipsRequest request,
        IGroupMembershipService membershipService,
        CancellationToken cancellationToken)
    {
        var result =
            await membershipService.ImportAsync(
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
            "GetGroupMembershipsByAuditId",
            new { auditId },
            new
            {
                importedCount =
                    result.ImportedCount,

                memberships =
                    result.Memberships
            });
    }
}
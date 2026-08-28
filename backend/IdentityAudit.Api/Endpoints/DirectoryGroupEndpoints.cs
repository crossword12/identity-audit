using IdentityAudit.Application.Audits;
using IdentityAudit.Application.Groups;
using IdentityAudit.Api.Authorization;

namespace IdentityAudit.Api.Endpoints;

public static class DirectoryGroupEndpoints
{
    public static IEndpointRouteBuilder MapDirectoryGroupEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/audits/{auditId:guid}/groups")
            .WithTags("Directory Groups")
            .RequireAuthorization(
                AuthorizationPolicies.CanReadAuditData);

        group.MapGet("", GetByAuditIdAsync)
            .WithName("GetGroupsByAuditId");

        group.MapPost("", ImportAsync)
            .WithName("ImportGroups")
            .RequireAuthorization(
                AuthorizationPolicies.CanManageAudits);

        return endpoints;
    }

    private static async Task<IResult> GetByAuditIdAsync(
        Guid auditId,
        IDirectoryGroupService groupService,
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

        var groups = await groupService.GetByAuditIdAsync(
            auditId,
            cancellationToken);

        return Results.Ok(groups);
    }

    private static async Task<IResult> ImportAsync(
        Guid auditId,
        ImportGroupsRequest request,
        IDirectoryGroupService groupService,
        CancellationToken cancellationToken)
    {
        var result = await groupService.ImportAsync(
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
            "GetGroupsByAuditId",
            new { auditId },
            new
            {
                importedCount = result.ImportedCount,
                groups = result.Groups
            });
    }
}
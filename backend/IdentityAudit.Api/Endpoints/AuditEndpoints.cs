using IdentityAudit.Application.Audits;
using IdentityAudit.Api.Authorization;

namespace IdentityAudit.Api.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/audits")
            .WithTags("Audits")
            .RequireAuthorization(
                AuthorizationPolicies.CanReadAuditData);

        group.MapGet("", GetAllAsync)
            .WithName("GetAudits");

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetAuditById");

        group.MapPost("", CreateAsync)
            .WithName("CreateAudit")
            .RequireAuthorization(
                AuthorizationPolicies.CanManageAudits);

        group.MapPost("/{id:guid}/start", StartAsync)
            .WithName("StartAudit")
            .RequireAuthorization(
                AuthorizationPolicies.CanManageAudits);

        group.MapPost("/{id:guid}/complete", CompleteAsync)
            .WithName("CompleteAudit")
            .RequireAuthorization(
                AuthorizationPolicies.CanManageAudits);

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        IAuditService auditService,
        CancellationToken cancellationToken)
    {
        var audits = await auditService.GetAllAsync(
            cancellationToken);

        return Results.Ok(audits);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IAuditService auditService,
        CancellationToken cancellationToken)
    {
        var audit = await auditService.GetByIdAsync(
            id,
            cancellationToken);

        if (audit is null)
        {
            return Results.NotFound(new
            {
                message = "L'audit demandé est introuvable."
            });
        }

        return Results.Ok(audit);
    }

    private static async Task<IResult> CreateAsync(
        CreateAuditRequest request,
        IAuditService auditService,
        CancellationToken cancellationToken)
    {
        var result = await auditService.CreateAsync(
            request,
            cancellationToken);

        if (!result.IsSuccess || result.Audit is null)
        {
            return Results.BadRequest(new
            {
                message = result.Error
            });
        }

        return Results.CreatedAtRoute(
            "GetAuditById",
            new { id = result.Audit.Id },
            result.Audit);
    }

    private static async Task<IResult> StartAsync(
        Guid id,
        IAuditService auditService,
        CancellationToken cancellationToken)
    {
        var result = await auditService.StartAsync(
            id,
            cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new
            {
                message = result.ErrorMessage
            });
        }

        return Results.Ok(result.Audit);
    }

    private static async Task<IResult> CompleteAsync(
        Guid id,
        IAuditService auditService,
        CancellationToken cancellationToken)
    {
        var result = await auditService.CompleteAsync(
            id,
            cancellationToken);

        if (!result.Succeeded)
        {
            return Results.BadRequest(new
            {
                message = result.ErrorMessage
            });
        }

        return Results.Ok(result.Audit);
    }
}
using IdentityAudit.Application.Audits;
using IdentityAudit.Api.Authorization;
using IdentityAudit.Application.AuditExports;
using IdentityAudit.Application.AuditLogs;
using System.Security.Claims;
using IdentityAudit.Api.Authentication;
using IdentityAudit.Domain.Enums;

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

        group.MapGet("/{id:guid}/export.csv", ExportCsvAsync)
            .WithName("ExportAuditCsv");

        group.MapGet("/{id:guid}/logs", GetLogsAsync)
            .WithName("GetAuditLogs")
            .Produces<IReadOnlyCollection<AuditLogDto>>(
                StatusCodes.Status200OK)
            .Produces(
                StatusCodes.Status404NotFound);

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

    private static async Task<IResult> GetLogsAsync(
    Guid id,
    IAuditLogService auditLogService,
    CancellationToken cancellationToken)
    {
        var auditLogs =
            await auditLogService.GetByAuditAsync(
                id,
                cancellationToken);

        if (auditLogs is null)
        {
            return Results.NotFound(new
            {
                message =
                    "L'audit demandé est introuvable."
            });
        }

        return Results.Ok(auditLogs);
    }

    private static async Task<IResult> CreateAsync(
        CreateAuditRequest request,
        IAuditService auditService,
        ClaimsPrincipal principal,
        IAuditLogService auditLogService,
        CancellationToken cancellationToken)
    {
        var applicationUserId =
            principal.GetApplicationUserId();

        var result = await auditService.CreateAsync(
            request,
            applicationUserId,
            cancellationToken);

        if (!result.IsSuccess || result.Audit is null)
        {
            return Results.BadRequest(new
            {
                message = result.Error
            });
        }

        await auditLogService.RecordAsync(
            result.Audit.Id,
            applicationUserId,
            AuditLogLevel.Information,
            AuditLogEventTypes.AuditCreated,
            $"Audit {result.Audit.Id} créé pour la cible " +
            $"« {result.Audit.TargetName} ».",
            cancellationToken);

        return Results.CreatedAtRoute(
            "GetAuditById",
            new { id = result.Audit.Id },
            result.Audit);
    }

    private static async Task<IResult> StartAsync(
        Guid id,
        IAuditService auditService,
        ClaimsPrincipal principal,
        IAuditLogService auditLogService,
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

        await auditLogService.RecordAsync(
            id,
            principal.GetApplicationUserId(),
            AuditLogLevel.Information,
            AuditLogEventTypes.AuditStarted,
            $"Audit {id} démarré.",
            cancellationToken);

        return Results.Ok(result.Audit);
    }

    private static async Task<IResult> CompleteAsync(
        Guid id,
        IAuditService auditService,
        ClaimsPrincipal principal,
        IAuditLogService auditLogService,
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

        await auditLogService.RecordAsync(
            id,
            principal.GetApplicationUserId(),
            AuditLogLevel.Information,
            AuditLogEventTypes.AuditCompleted,
            $"Audit {id} terminé.",
            cancellationToken);

        return Results.Ok(result.Audit);
    }

    private static async Task<IResult> ExportCsvAsync(
    Guid id,
    IAuditExportService auditExportService,
    HttpContext httpContext,
    ClaimsPrincipal principal,
    IAuditLogService auditLogService,
    CancellationToken cancellationToken)
    {
        var result =
            await auditExportService.ExportCsvAsync(
                id,
                cancellationToken);

        if (!result.Succeeded ||
            result.Content is null ||
            string.IsNullOrWhiteSpace(result.FileName))
        {
            return Results.NotFound(new
            {
                message = result.ErrorMessage
                    ?? "L'export demandé est introuvable."
            });
        }

        await auditLogService.RecordAsync(
            id,
            principal.GetApplicationUserId(),
            AuditLogLevel.Information,
            AuditLogEventTypes.AuditExported,
            $"Résultats CIS de l'audit {id} " +
            "exportés au format CSV.",
            cancellationToken);

        httpContext.Response.Headers.CacheControl =
            "no-store";

        httpContext.Response.Headers.Pragma =
            "no-cache";

        return Results.File(
            result.Content,
            contentType: "text/csv; charset=utf-8",
            fileDownloadName: result.FileName);
    }
}
using IdentityAudit.Application.RuleEvaluations;
using IdentityAudit.Api.Authorization;

namespace IdentityAudit.Api.Endpoints;

public static class RuleEvaluationEndpoints
{
    public static IEndpointRouteBuilder
        MapRuleEvaluationEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/audits/{auditId:guid}")
            .WithTags("Rule evaluations")
            .RequireAuthorization(
                AuthorizationPolicies.CanReadAuditData);

        group.MapPost(
                "/evaluate",
                EvaluateAuditRulesAsync)
            .WithName("EvaluateAuditRules")
            .Produces<EvaluateAuditRulesResult>(
                StatusCodes.Status200OK)
            .ProducesProblem(
                StatusCodes.Status404NotFound)
            .ProducesProblem(
                StatusCodes.Status409Conflict)
            .RequireAuthorization(
                AuthorizationPolicies.CanManageAudits);

        group.MapGet(
                "/rule-evaluations",
                GetAuditRuleEvaluationsAsync)
            .WithName("GetAuditRuleEvaluations")
            .Produces<
                IReadOnlyCollection<RuleEvaluationDto>>(
                StatusCodes.Status200OK)
            .ProducesProblem(
                StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult>
        EvaluateAuditRulesAsync(
            Guid auditId,
            IAuditRuleEvaluationService service,
            CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await service.EvaluateAsync(
                    auditId,
                    cancellationToken);

            return Results.Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return Results.Problem(
                statusCode:
                    StatusCodes.Status404NotFound,
                title:
                    "Audit introuvable",
                detail:
                    exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Results.Problem(
                statusCode:
                    StatusCodes.Status409Conflict,
                title:
                    "Évaluation impossible",
                detail:
                    exception.Message);
        }
    }

    private static async Task<IResult>
        GetAuditRuleEvaluationsAsync(
            Guid auditId,
            IAuditRuleEvaluationService service,
            CancellationToken cancellationToken)
    {
        try
        {
            var evaluations =
                await service.GetByAuditAsync(
                    auditId,
                    cancellationToken);

            return Results.Ok(evaluations);
        }
        catch (KeyNotFoundException exception)
        {
            return Results.Problem(
                statusCode:
                    StatusCodes.Status404NotFound,
                title:
                    "Audit introuvable",
                detail:
                    exception.Message);
        }
    }
}
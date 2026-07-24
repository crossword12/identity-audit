using IdentityAudit.Application.Targets;

namespace IdentityAudit.Api.Endpoints;

public static class TargetEndpoints
{
    public static IEndpointRouteBuilder MapTargetEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/targets")
            .WithTags("Targets");

        group.MapGet("/", GetAllAsync)
            .WithName("GetTargets");

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetTargetById");

        group.MapPost("/", CreateAsync)
            .WithName("CreateTarget");

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateTarget");

        group.MapPost(
                "/{id:guid}/test-connection",
                TestConnectionAsync)
            .WithName("TestTargetConnection");

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        ITargetService targetService,
        CancellationToken cancellationToken)
    {
        var targets = await targetService.GetAllAsync(
            cancellationToken);

        return Results.Ok(targets);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ITargetService targetService,
        CancellationToken cancellationToken)
    {
        var target = await targetService.GetByIdAsync(
            id,
            cancellationToken);

        if (target is null)
        {
            return Results.NotFound(new
            {
                message = "La cible demandée est introuvable."
            });
        }

        return Results.Ok(target);
    }

    private static async Task<IResult> CreateAsync(
        CreateTargetRequest request,
        ITargetService targetService,
        CancellationToken cancellationToken)
    {
        var target = await targetService.CreateAsync(
            request,
            cancellationToken);

        return Results.CreatedAtRoute(
            "GetTargetById",
            new { id = target.Id },
            target);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateTargetRequest request,
        ITargetService targetService,
        CancellationToken cancellationToken)
    {
        var result = await targetService.UpdateAsync(
            id,
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return Results.NotFound(new
            {
                message = result.ErrorMessage
            });
        }

        return Results.Ok(result.Target);
    }

    private static async Task<IResult> TestConnectionAsync(
        Guid id,
        ITargetService targetService,
        CancellationToken cancellationToken)
    {
        var result = await targetService.TestConnectionAsync(
            id,
            cancellationToken);

        if (!result.TargetFound)
        {
            return Results.NotFound(new
            {
                message = result.Message,
                testedAt = result.TestedAt
            });
        }

        if (!result.Succeeded)
        {
            return Results.BadRequest(new
            {
                succeeded = result.Succeeded,
                message = result.Message,
                testedAt = result.TestedAt
            });
        }

        return Results.Ok(new
        {
            succeeded = result.Succeeded,
            message = result.Message,
            testedAt = result.TestedAt
        });
    }
}
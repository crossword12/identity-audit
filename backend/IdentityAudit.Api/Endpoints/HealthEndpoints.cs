using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityAudit.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/health", () =>
        {
            return Results.Ok(new
            {
                status = "Healthy",
                service = "IdentityAudit.Api",
                timestamp = DateTimeOffset.UtcNow
            });
        })
        .WithName("GetHealth")
        .WithTags("Health")
        .Produces(StatusCodes.Status200OK);

        return endpoints;
    }
}
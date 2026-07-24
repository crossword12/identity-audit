namespace IdentityAudit.Api.Endpoints;

public static class ApiEndpoints
{
    public static IEndpointRouteBuilder MapApiEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthEndpoints();
        endpoints.MapTargetEndpoints();
        endpoints.MapAuditEndpoints();
        endpoints.MapIdentityEndpoints();
        endpoints.MapDashboardEndpoints();

        return endpoints;
    }
}
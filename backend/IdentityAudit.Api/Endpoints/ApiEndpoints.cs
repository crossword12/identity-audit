namespace IdentityAudit.Api.Endpoints;

public static class ApiEndpoints
{
    public static IEndpointRouteBuilder MapApiEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuthenticationEndpoints();
        endpoints.MapApplicationUserEndpoints();
        endpoints.MapHealthEndpoints();
        endpoints.MapTargetEndpoints();
        endpoints.MapAuditEndpoints();
        endpoints.MapAuditLogEndpoints();
        endpoints.MapIdentityEndpoints();
        endpoints.MapDirectoryGroupEndpoints();
        endpoints.MapGroupMembershipEndpoints();
        endpoints.MapDirectoryRoleEndpoints();
        endpoints.MapRoleAssignmentEndpoints();
        endpoints.MapRuleEvaluationEndpoints();
        endpoints.MapDashboardEndpoints();

        return endpoints;
    }
}
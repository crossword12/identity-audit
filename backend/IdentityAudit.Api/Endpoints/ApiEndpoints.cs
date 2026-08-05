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
        endpoints.MapDirectoryGroupEndpoints();
        endpoints.MapGroupMembershipEndpoints();
        endpoints.MapDirectoryRoleEndpoints();
        endpoints.MapRoleAssignmentEndpoints();
        endpoints.MapRuleEvaluationEndpoints();
        endpoints.MapDashboardEndpoints();

        return endpoints;
    }
}
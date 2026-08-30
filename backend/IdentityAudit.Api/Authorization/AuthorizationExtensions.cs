using IdentityAudit.Application.Authentication;

namespace IdentityAudit.Api.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection
        AddIdentityAuditAuthorization(
            this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.CanReadAuditData,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        ApplicationRoles.Reader,
                        ApplicationRoles.Auditor,
                        ApplicationRoles.Administrator));

            options.AddPolicy(
                AuthorizationPolicies.CanManageAudits,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        ApplicationRoles.Auditor,
                        ApplicationRoles.Administrator));

            options.AddPolicy(
                AuthorizationPolicies.CanManageTargets,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        ApplicationRoles.Administrator));

            options.AddPolicy(
                AuthorizationPolicies.CanManageUsers,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        ApplicationRoles.Administrator));

            options.AddPolicy(
                AuthorizationPolicies.CanReadActivityLogs,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        ApplicationRoles.Administrator));
        });

        return services;
    }
}
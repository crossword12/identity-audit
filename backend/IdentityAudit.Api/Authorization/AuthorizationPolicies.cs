namespace IdentityAudit.Api.Authorization;

public static class AuthorizationPolicies
{
    public const string CanReadAuditData =
        nameof(CanReadAuditData);

    public const string CanManageAudits =
        nameof(CanManageAudits);

    public const string CanManageTargets =
        nameof(CanManageTargets);

    public const string CanManageUsers =
        nameof(CanManageUsers);
}
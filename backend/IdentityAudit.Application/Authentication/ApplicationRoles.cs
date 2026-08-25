namespace IdentityAudit.Application.Authentication;

public static class ApplicationRoles
{
    public const string Administrator = "Administrator";

    public const string Auditor = "Auditor";

    public const string Reader = "Reader";

    public static IReadOnlyCollection<string> All { get; } =
    [
        Administrator,
        Auditor,
        Reader
    ];
}
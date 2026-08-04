namespace IdentityAudit.Application.Roles;

public sealed class ImportRolesResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyList<DirectoryRoleDto> Roles
    {
        get;
        init;
    } = Array.Empty<DirectoryRoleDto>();

    public int ImportedCount => Roles.Count;

    public static ImportRolesResult Success(
        IReadOnlyList<DirectoryRoleDto> roles)
    {
        return new ImportRolesResult
        {
            Succeeded = true,
            Roles = roles
        };
    }

    public static ImportRolesResult Failure(
        string errorMessage)
    {
        return new ImportRolesResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}
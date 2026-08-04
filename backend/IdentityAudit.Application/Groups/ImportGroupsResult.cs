namespace IdentityAudit.Application.Groups;

public sealed class ImportGroupsResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyList<DirectoryGroupDto> Groups { get; init; }
        = Array.Empty<DirectoryGroupDto>();

    public int ImportedCount => Groups.Count;

    public static ImportGroupsResult Success(
        IReadOnlyList<DirectoryGroupDto> groups)
    {
        return new ImportGroupsResult
        {
            Succeeded = true,
            Groups = groups
        };
    }

    public static ImportGroupsResult Failure(
        string errorMessage)
    {
        return new ImportGroupsResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}
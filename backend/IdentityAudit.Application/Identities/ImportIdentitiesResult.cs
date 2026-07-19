namespace IdentityAudit.Application.Identities;

public sealed class ImportIdentitiesResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyList<DirectoryIdentityDto> Identities { get; init; }
        = Array.Empty<DirectoryIdentityDto>();

    public int ImportedCount => Identities.Count;

    public static ImportIdentitiesResult Success(
        IReadOnlyList<DirectoryIdentityDto> identities)
    {
        return new ImportIdentitiesResult
        {
            Succeeded = true,
            Identities = identities
        };
    }

    public static ImportIdentitiesResult Failure(
        string errorMessage)
    {
        return new ImportIdentitiesResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}
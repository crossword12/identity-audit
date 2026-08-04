namespace IdentityAudit.EntraCollector.Models;

public sealed class CreateAuditPayload
{
    public Guid TargetId { get; init; }
}

public sealed class AuditApiResponse
{
    public Guid Id { get; init; }

    public Guid TargetId { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }
}

public sealed class ImportIdentitiesPayload
{
    public IReadOnlyCollection<CollectedIdentityPayload> Identities
    {
        get;
        init;
    } = Array.Empty<CollectedIdentityPayload>();
}

public sealed class ImportIdentitiesApiResponse
{
    public int ImportedCount { get; init; }
}
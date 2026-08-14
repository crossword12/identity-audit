namespace IdentityAudit.ActiveDirectoryCollector.Models;

public sealed class ActiveDirectoryGroup
{
    public string ObjectGuid { get; init; } = string.Empty;

    public string ObjectSid { get; init; } = string.Empty;

    public string DistinguishedName { get; init; } = string.Empty;

    public string SamAccountName { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int GroupTypeValue { get; init; }
}
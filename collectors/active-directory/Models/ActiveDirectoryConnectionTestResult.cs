namespace IdentityAudit.ActiveDirectoryCollector.Models;

public sealed record ActiveDirectoryConnectionTestResult(
    string Host,
    int Port,
    bool UseSsl,
    string ConfiguredBaseDn,
    string? ServerDnsHostName,
    string? DefaultNamingContext,
    IReadOnlyCollection<string> SupportedLdapVersions);
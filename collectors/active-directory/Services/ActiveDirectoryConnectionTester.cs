using System.DirectoryServices.Protocols;
using System.Net;
using IdentityAudit.ActiveDirectoryCollector.Configuration;
using IdentityAudit.ActiveDirectoryCollector.Models;

namespace IdentityAudit.ActiveDirectoryCollector.Services;

public sealed class ActiveDirectoryConnectionTester
{
    private readonly ActiveDirectoryOptions _options;

    public ActiveDirectoryConnectionTester(
        ActiveDirectoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Validate();

        _options = options;
    }

    public ActiveDirectoryConnectionTestResult Test()
    {
        var identifier =
            new LdapDirectoryIdentifier(
                _options.Host,
                _options.Port,
                fullyQualifiedDnsHostName: true,
                connectionless: false);

        var credentials =
            new NetworkCredential(
                _options.BindUsername,
                _options.BindPassword);

        using var connection =
            new LdapConnection(
                identifier,
                credentials,
                AuthType.Basic);

        connection.Timeout =
            _options.Timeout;

        connection.SessionOptions.ProtocolVersion = 3;

        connection.SessionOptions.SecureSocketLayer =
            _options.UseSsl;

        LdapServerCertificateValidator? certificateValidator = null;

        string? serverDnsHostName = null;
        string? defaultNamingContext = null;

        IReadOnlyCollection<string> supportedLdapVersions =
            Array.Empty<string>();

        if (_options.UseSsl &&
            !string.IsNullOrWhiteSpace(
                _options.TrustedCaCertificatePath))
        {
            certificateValidator =
                new LdapServerCertificateValidator(
                    _options.Host,
                    _options.TrustedCaCertificatePath);

            connection.SessionOptions.VerifyServerCertificate =
                certificateValidator.Validate;
        }

        try
        {
            connection.Bind();

            var rootDseRequest =
                new SearchRequest(
                    distinguishedName: string.Empty,
                    ldapFilter: "(objectClass=*)",
                    searchScope: SearchScope.Base,
                    attributeList:
                    [
                        "defaultNamingContext",
                        "dnsHostName",
                        "supportedLDAPVersion"
                    ]);

            var rootDseResponse =
                (SearchResponse)connection.SendRequest(
                    rootDseRequest);

            if (rootDseResponse.Entries.Count != 1)
            {
                throw new InvalidOperationException(
                    "Le RootDSE Active Directory n'a pas pu être lu.");
            }

            var rootDse =
                rootDseResponse.Entries[0];

            defaultNamingContext =
                ReadFirstString(
                    rootDse,
                    "defaultNamingContext");

            serverDnsHostName =
                ReadFirstString(
                    rootDse,
                    "dnsHostName");

            supportedLdapVersions =
                ReadAllStrings(
                    rootDse,
                    "supportedLDAPVersion");

            ValidateConfiguredBaseDn(
                connection);
        }
        finally
        {
            certificateValidator?.Dispose();
        }

        return new ActiveDirectoryConnectionTestResult(
            Host: _options.Host,
            Port: _options.Port,
            UseSsl: _options.UseSsl,
            ConfiguredBaseDn: _options.BaseDn,
            ServerDnsHostName: serverDnsHostName,
            DefaultNamingContext: defaultNamingContext,
            SupportedLdapVersions: supportedLdapVersions);
    }

    private void ValidateConfiguredBaseDn(
        LdapConnection connection)
    {
        var baseDnRequest =
            new SearchRequest(
                distinguishedName:
                    _options.BaseDn,

                ldapFilter:
                    "(objectClass=*)",

                searchScope:
                    SearchScope.Base,

                attributeList:
                [
                    "distinguishedName"
                ]);

        var baseDnResponse =
            (SearchResponse)connection.SendRequest(
                baseDnRequest);

        if (baseDnResponse.Entries.Count != 1)
        {
            throw new InvalidOperationException(
                "Le Base DN configuré est introuvable : " +
                _options.BaseDn);
        }
    }

    private static string? ReadFirstString(
        SearchResultEntry entry,
        string attributeName)
    {
        var values =
            ReadAllStrings(
                entry,
                attributeName);

        return values.FirstOrDefault();
    }

    private static IReadOnlyCollection<string>
        ReadAllStrings(
            SearchResultEntry entry,
            string attributeName)
    {
        var attribute =
            entry.Attributes[attributeName];

        if (attribute is null ||
            attribute.Count == 0)
        {
            return Array.Empty<string>();
        }

        return attribute
            .GetValues(typeof(string))
            .Cast<string>()
            .ToArray();
    }
}
using System.DirectoryServices.Protocols;
using System.Net;
using IdentityAudit.ActiveDirectoryCollector.Configuration;
using IdentityAudit.ActiveDirectoryCollector.Models;

namespace IdentityAudit.ActiveDirectoryCollector.Services;

public sealed class ActiveDirectoryUserCollector
{
    private const int PageSize = 500;

    private readonly ActiveDirectoryOptions _options;

    public ActiveDirectoryUserCollector(
        ActiveDirectoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Validate();

        _options = options;
    }

    public IReadOnlyCollection<ActiveDirectoryUser> Collect()
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

        LdapServerCertificateValidator? certificateValidator =
            null;

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

            return CollectUsers(
                connection);
        }
        finally
        {
            certificateValidator?.Dispose();
        }
    }

    private IReadOnlyCollection<ActiveDirectoryUser>
        CollectUsers(
            LdapConnection connection)
    {
        var users =
            new List<ActiveDirectoryUser>();

        var pageRequest =
            new PageResultRequestControl(
                PageSize);

        do
        {
            var request =
                new SearchRequest(
                    distinguishedName:
                        _options.BaseDn,

                    ldapFilter:
                        "(&(objectCategory=person)(objectClass=user))",

                    searchScope:
                        SearchScope.Subtree,

                    attributeList:
                    [
                        "objectGUID",
                        "distinguishedName",
                        "sAMAccountName",
                        "userPrincipalName",
                        "displayName",
                        "mail",
                        "userAccountControl",
                        "msDS-User-Account-Control-Computed",
                        "lastLogonTimestamp",
                        "description",
                        "servicePrincipalName",
                        "memberOf"
                    ]);

            request.Controls.Add(
                pageRequest);

            var response =
                (SearchResponse)connection.SendRequest(
                    request);

            foreach (SearchResultEntry entry
                in response.Entries)
            {
                var user =
                    MapEntry(entry);

                if (user is not null)
                {
                    users.Add(user);
                }
            }

            var pageResponse =
                response.Controls
                    .OfType<PageResultResponseControl>()
                    .FirstOrDefault();

            if (pageResponse is null ||
                pageResponse.Cookie.Length == 0)
            {
                break;
            }

            pageRequest.Cookie =
                pageResponse.Cookie;
        }
        while (true);

        return users;
    }

    private static ActiveDirectoryUser? MapEntry(
        SearchResultEntry entry)
    {
        var objectGuid =
            ReadGuid(
                entry,
                "objectGUID");

        var samAccountName =
            ReadFirstString(
                entry,
                "sAMAccountName");

        if (objectGuid is null ||
            string.IsNullOrWhiteSpace(
                samAccountName))
        {
            return null;
        }

        return new ActiveDirectoryUser
        {
            ObjectGuid =
                objectGuid.Value.ToString(),

            DistinguishedName =
                ReadFirstString(
                    entry,
                    "distinguishedName")
                ?? entry.DistinguishedName
                ?? string.Empty,

            SamAccountName =
                samAccountName.Trim(),

            UserPrincipalName =
                NormalizeOptionalText(
                    ReadFirstString(
                        entry,
                        "userPrincipalName")),

            DisplayName =
                NormalizeOptionalText(
                    ReadFirstString(
                        entry,
                        "displayName")),

            Email =
                NormalizeOptionalText(
                    ReadFirstString(
                        entry,
                        "mail")),

            UserAccountControl =
                ReadInteger(
                    entry,
                    "userAccountControl")
                ?? 0,

            ComputedUserAccountControl =
                ReadInteger(
                    entry,
                    "msDS-User-Account-Control-Computed"),

            LastLogonAt =
                ReadFileTime(
                    entry,
                    "lastLogonTimestamp"),

            Description =
                NormalizeOptionalText(
                    ReadFirstString(
                        entry,
                        "description")),

            ServicePrincipalNames =
                ReadAllStrings(
                    entry,
                    "servicePrincipalName"),

            MemberOfDistinguishedNames =
                ReadAllStrings(
                    entry,
                    "memberOf")
        };
    }

    private static Guid? ReadGuid(
        SearchResultEntry entry,
        string attributeName)
    {
        var attribute =
            entry.Attributes[
                attributeName];

        if (attribute is null ||
            attribute.Count == 0)
        {
            return null;
        }

        var values =
            attribute.GetValues(
                typeof(byte[]));

        if (values.Length == 0 ||
            values[0] is not byte[] bytes ||
            bytes.Length != 16)
        {
            return null;
        }

        return new Guid(bytes);
    }

    private static int? ReadInteger(
        SearchResultEntry entry,
        string attributeName)
    {
        var value =
            ReadFirstString(
                entry,
                attributeName);

        if (string.IsNullOrWhiteSpace(
            value))
        {
            return null;
        }

        return int.TryParse(
            value,
            out var result)
            ? result
            : null;
    }

    private static DateTimeOffset? ReadFileTime(
        SearchResultEntry entry,
        string attributeName)
    {
        var value =
            ReadFirstString(
                entry,
                attributeName);

        if (string.IsNullOrWhiteSpace(value) ||
            !long.TryParse(
                value,
                out var fileTime) ||
            fileTime <= 0)
        {
            return null;
        }

        try
        {
            return new DateTimeOffset(
                DateTime.FromFileTimeUtc(
                    fileTime));
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static string? ReadFirstString(
        SearchResultEntry entry,
        string attributeName)
    {
        return ReadAllStrings(
                entry,
                attributeName)
            .FirstOrDefault();
    }

    private static IReadOnlyCollection<string>
        ReadAllStrings(
            SearchResultEntry entry,
            string attributeName)
    {
        var attribute =
            entry.Attributes[
                attributeName];

        if (attribute is null ||
            attribute.Count == 0)
        {
            return Array.Empty<string>();
        }

        return attribute
            .GetValues(
                typeof(string))
            .Cast<string>()
            .Where(
                value =>
                    !string.IsNullOrWhiteSpace(
                        value))
            .Select(
                value =>
                    value.Trim())
            .ToArray();
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
            ? null
            : value.Trim();
    }
}
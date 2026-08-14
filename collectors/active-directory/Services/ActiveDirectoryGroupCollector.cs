using System.DirectoryServices.Protocols;
using System.Net;
using IdentityAudit.ActiveDirectoryCollector.Configuration;
using IdentityAudit.ActiveDirectoryCollector.Models;

namespace IdentityAudit.ActiveDirectoryCollector.Services;

public sealed class ActiveDirectoryGroupCollector
{
    private const int PageSize = 500;

    private readonly ActiveDirectoryOptions _options;

    public ActiveDirectoryGroupCollector(
        ActiveDirectoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Validate();

        _options = options;
    }

    public IReadOnlyCollection<ActiveDirectoryGroup> Collect()
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

            return CollectGroups(connection);
        }
        finally
        {
            certificateValidator?.Dispose();
        }
    }

    private IReadOnlyCollection<ActiveDirectoryGroup>
        CollectGroups(
            LdapConnection connection)
    {
        var groups =
            new List<ActiveDirectoryGroup>();

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
                        "(objectClass=group)",

                    searchScope:
                        SearchScope.Subtree,

                    attributeList:
                    [
                        "objectGUID",
                        "objectSid",
                        "distinguishedName",
                        "sAMAccountName",
                        "cn",
                        "description",
                        "groupType",
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
                var group =
                    MapEntry(entry);

                if (group is not null)
                {
                    groups.Add(group);
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

        return groups;
    }

    private static ActiveDirectoryGroup? MapEntry(
        SearchResultEntry entry)
    {
        var objectGuid =
            ReadGuid(
                entry,
                "objectGUID");

        var objectSidBytes =
            ReadBytes(
                entry,
                "objectSid");

        var samAccountName =
            ReadFirstString(
                entry,
                "sAMAccountName");

        if (objectGuid is null ||
            objectSidBytes is null ||
            string.IsNullOrWhiteSpace(
                samAccountName))
        {
            return null;
        }

        return new ActiveDirectoryGroup
        {
            ObjectGuid =
                objectGuid.Value.ToString(),

            ObjectSid =
                ConvertSidToString(
                    objectSidBytes),

            DistinguishedName =
                ReadFirstString(
                    entry,
                    "distinguishedName")
                ?? entry.DistinguishedName
                ?? string.Empty,

            SamAccountName =
                samAccountName.Trim(),

            Name =
                ReadFirstString(
                    entry,
                    "cn")
                ?? samAccountName.Trim(),

            Description =
                NormalizeOptionalText(
                    ReadFirstString(
                        entry,
                        "description")),

            GroupTypeValue =
                ReadInteger(
                    entry,
                    "groupType")
                ?? 0,

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
        var bytes =
            ReadBytes(
                entry,
                attributeName);

        if (bytes is null ||
            bytes.Length != 16)
        {
            return null;
        }

        return new Guid(bytes);
    }

    private static byte[]? ReadBytes(
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

        return values.Length > 0 &&
               values[0] is byte[] bytes
            ? bytes
            : null;
    }

    private static int? ReadInteger(
        SearchResultEntry entry,
        string attributeName)
    {
        var value =
            ReadFirstString(
                entry,
                attributeName);

        return int.TryParse(
            value,
            out var result)
            ? result
            : null;
    }

    private static string? ReadFirstString(
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
                typeof(string));

        return values.Length > 0
            ? values[0]?.ToString()
            : null;
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
            .GetValues(typeof(string))
            .Cast<string>()
            .Where(value =>
                !string.IsNullOrWhiteSpace(value))
            .Select(value =>
                value.Trim())
            .ToArray();
    }

    private static string ConvertSidToString(
        byte[] sid)
    {
        if (sid.Length < 8)
        {
            return string.Empty;
        }

        var revision =
            sid[0];

        var subAuthorityCount =
            sid[1];

        ulong identifierAuthority = 0;

        for (var i = 2; i < 8; i++)
        {
            identifierAuthority =
                (identifierAuthority << 8) |
                sid[i];
        }

        var result =
            $"S-{revision}-{identifierAuthority}";

        for (var i = 0;
             i < subAuthorityCount;
             i++)
        {
            var offset =
                8 + (i * 4);

            if (offset + 4 > sid.Length)
            {
                break;
            }

            var subAuthority =
                BitConverter.ToUInt32(
                    sid,
                    offset);

            result +=
                $"-{subAuthority}";
        }

        return result;
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
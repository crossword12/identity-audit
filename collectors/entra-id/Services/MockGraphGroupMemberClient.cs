using System.Text.Json;
using IdentityAudit.EntraCollector.Models;

namespace IdentityAudit.EntraCollector.Services;

public sealed class MockGraphGroupMemberClient
{
    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public async Task<IReadOnlyCollection<GraphGroupMember>>
        GetAllMembersAsync(
            string groupExternalId,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(groupExternalId))
        {
            throw new ArgumentException(
                "L'identifiant externe du groupe est obligatoire.",
                nameof(groupExternalId));
        }

        var members = new List<GraphGroupMember>();

        string? currentFileName =
            ResolveFirstFileName(groupExternalId);

        var visitedFiles = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        while (currentFileName is not null)
        {
            if (!visitedFiles.Add(currentFileName))
            {
                throw new InvalidOperationException(
                    $"Boucle de pagination détectée avec le fichier {currentFileName}.");
            }

            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "MockGraph",
                "GroupMembers",
                currentFileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "La page des membres Microsoft Graph simulée est introuvable.",
                    filePath);
            }

            await using var stream = File.OpenRead(filePath);

            var page =
                await JsonSerializer.DeserializeAsync<GraphGroupMemberPage>(
                    stream,
                    _jsonOptions,
                    cancellationToken);

            if (page is null)
            {
                throw new InvalidOperationException(
                    $"Impossible de lire la page {currentFileName}.");
            }

            Console.WriteLine(
                $"Page de membres chargée : {currentFileName} — " +
                $"{page.Value.Count} membre(s).");

            members.AddRange(page.Value);

            currentFileName = ResolveNextFileName(
                groupExternalId,
                page.ODataNextLink);
        }

        return members;
    }

    private static string ResolveFirstFileName(
        string groupExternalId)
    {
        return groupExternalId switch
        {
            "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
                => "group-aaaaaaaa-page-1.json",

            "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
                => "group-bbbbbbbb-page-1.json",

            "cccccccc-cccc-cccc-cccc-cccccccccccc"
                => "group-cccccccc-page-1.json",

            _ => throw new InvalidOperationException(
                $"Aucune réponse simulée n'est configurée pour le groupe {groupExternalId}.")
        };
    }

    private static string? ResolveNextFileName(
        string groupExternalId,
        string? nextLink)
    {
        if (string.IsNullOrWhiteSpace(nextLink))
        {
            return null;
        }

        if (!Uri.TryCreate(
                nextLink,
                UriKind.Absolute,
                out var uri))
        {
            throw new InvalidOperationException(
                $"Le lien de pagination des membres est invalide : {nextLink}");
        }

        var skipToken = uri.Query
            .TrimStart('?')
            .Split(
                '&',
                StringSplitOptions.RemoveEmptyEntries)
            .Select(parameter =>
                parameter.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .FirstOrDefault(parts =>
                Uri.UnescapeDataString(parts[0])
                    .Equals(
                        "$skiptoken",
                        StringComparison.OrdinalIgnoreCase));

        if (skipToken is null)
        {
            throw new InvalidOperationException(
                $"Le lien ne contient pas de $skiptoken : {nextLink}");
        }

        var tokenValue =
            Uri.UnescapeDataString(skipToken[1]);

        return (groupExternalId, tokenValue) switch
        {
            (
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                "members-page-2"
            ) => "group-aaaaaaaa-page-2.json",

            _ => throw new InvalidOperationException(
                $"Le jeton de pagination des membres est inconnu : " +
                $"{groupExternalId} / {tokenValue}")
        };
    }
}
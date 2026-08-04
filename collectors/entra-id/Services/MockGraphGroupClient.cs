using System.Text.Json;
using IdentityAudit.EntraCollector.Models;

namespace IdentityAudit.EntraCollector.Services;

public sealed class MockGraphGroupClient
{
    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public async Task<IReadOnlyCollection<GraphGroup>>
        GetAllGroupsAsync(
            CancellationToken cancellationToken = default)
    {
        var groups = new List<GraphGroup>();

        string? currentFileName = "groups-page-1.json";

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
                currentFileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "La page de groupes Microsoft Graph simulée est introuvable.",
                    filePath);
            }

            await using var stream = File.OpenRead(filePath);

            var page =
                await JsonSerializer.DeserializeAsync<GraphGroupPage>(
                    stream,
                    _jsonOptions,
                    cancellationToken);

            if (page is null)
            {
                throw new InvalidOperationException(
                    $"Impossible de lire la page {currentFileName}.");
            }

            Console.WriteLine(
                $"Page de groupes chargée : {currentFileName} — " +
                $"{page.Value.Count} groupe(s).");

            groups.AddRange(page.Value);

            currentFileName =
                ResolveNextFileName(page.ODataNextLink);
        }

        return groups;
    }

    private static string? ResolveNextFileName(
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
                $"Le lien de pagination des groupes est invalide : {nextLink}");
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

        return tokenValue switch
        {
            "groups-page-2" => "groups-page-2.json",

            _ => throw new InvalidOperationException(
                $"Le jeton de pagination des groupes est inconnu : {tokenValue}")
        };
    }
}
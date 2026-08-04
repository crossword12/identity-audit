using System.Text.Json;
using IdentityAudit.EntraCollector.Models;

namespace IdentityAudit.EntraCollector.Services;

public sealed class MockGraphRoleAssignmentClient
{
    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public async Task<
        IReadOnlyCollection<GraphRoleAssignment>>
        GetAllRoleAssignmentsAsync(
            CancellationToken cancellationToken = default)
    {
        var assignments =
            new List<GraphRoleAssignment>();

        string? currentFileName =
            "role-assignments-page-1.json";

        var visitedFiles = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        while (currentFileName is not null)
        {
            if (!visitedFiles.Add(currentFileName))
            {
                throw new InvalidOperationException(
                    "Boucle de pagination détectée avec " +
                    $"le fichier {currentFileName}.");
            }

            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "MockGraph",
                currentFileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "La page des affectations de rôles " +
                    "Microsoft Graph simulée est introuvable.",
                    filePath);
            }

            await using var stream =
                File.OpenRead(filePath);

            var page =
                await JsonSerializer.DeserializeAsync<
                    GraphRoleAssignmentPage>(
                        stream,
                        _jsonOptions,
                        cancellationToken);

            if (page is null)
            {
                throw new InvalidOperationException(
                    $"Impossible de lire la page {currentFileName}.");
            }

            Console.WriteLine(
                "Page d'affectations chargée : " +
                $"{currentFileName} — " +
                $"{page.Value.Count} affectation(s).");

            assignments.AddRange(page.Value);

            currentFileName =
                ResolveNextFileName(
                    page.ODataNextLink);
        }

        return assignments;
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
                "Le lien de pagination des affectations " +
                $"est invalide : {nextLink}");
        }

        var skipToken = uri.Query
            .TrimStart('?')
            .Split(
                '&',
                StringSplitOptions.RemoveEmptyEntries)
            .Select(parameter =>
                parameter.Split('=', 2))
            .Where(parts =>
                parts.Length == 2)
            .FirstOrDefault(parts =>
                Uri.UnescapeDataString(parts[0])
                    .Equals(
                        "$skiptoken",
                        StringComparison.OrdinalIgnoreCase));

        if (skipToken is null)
        {
            throw new InvalidOperationException(
                "Le lien de pagination des affectations " +
                $"ne contient pas de $skiptoken : {nextLink}");
        }

        var tokenValue =
            Uri.UnescapeDataString(skipToken[1]);

        return tokenValue switch
        {
            "role-assignments-page-2"
                => "role-assignments-page-2.json",

            _ => throw new InvalidOperationException(
                "Le jeton de pagination des affectations " +
                $"est inconnu : {tokenValue}")
        };
    }
}
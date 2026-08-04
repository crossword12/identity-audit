using IdentityAudit.EntraCollector.Services;

const string defaultApiBaseUrl =
    "http://localhost:5173";

if (args.Length == 0 ||
    !Guid.TryParse(args[0], out var targetId))
{
    Console.Error.WriteLine(
        "Utilisation : dotnet run " +
        "--project collectors/entra-id/IdentityAudit.EntraCollector.csproj " +
        "-- <targetId> [apiUrl]");

    return 1;
}

var apiBaseUrl = args.Length >= 2
    ? args[1].TrimEnd('/')
    : defaultApiBaseUrl;

try
{
    Console.WriteLine(
        "Démarrage du collecteur Microsoft Entra ID en mode simulé.");

    Console.WriteLine(
        $"Cible Entra ID : {targetId}");

    Console.WriteLine(
        $"Backend utilisé : {apiBaseUrl}");

    Console.WriteLine();

    var graphClient =
        new MockGraphUserClient();

    var graphUsers =
        await graphClient.GetAllUsersAsync();

    Console.WriteLine();

    Console.WriteLine(
        $"{graphUsers.Count} utilisateur(s) Microsoft Graph récupéré(s).");

    var mapper =
        new GraphUserMapper();

    var identities =
        mapper.Map(graphUsers);

    Console.WriteLine(
        $"{identities.Count} identité(s) normalisée(s) pour l'API.");

    using var httpClient =
        new HttpClient
        {
            BaseAddress = new Uri(
                $"{apiBaseUrl}/"),

            Timeout = TimeSpan.FromSeconds(30)
        };

    var apiClient =
        new IdentityAuditApiClient(httpClient);

    Console.WriteLine();
    Console.WriteLine("Création d'un nouvel audit...");

    var createdAudit =
        await apiClient.CreateAuditAsync(targetId);

    Console.WriteLine(
        $"Audit créé : {createdAudit.Id}");

    Console.WriteLine(
        $"Statut : {createdAudit.Status}");

    Console.WriteLine();
    Console.WriteLine("Démarrage de l'audit...");

    var startedAudit =
        await apiClient.StartAuditAsync(
            createdAudit.Id);

    Console.WriteLine(
        $"Statut : {startedAudit.Status}");

    Console.WriteLine();
    Console.WriteLine(
        "Envoi des identités vers le backend...");

    var importResult =
        await apiClient.ImportIdentitiesAsync(
            createdAudit.Id,
            identities);

    Console.WriteLine(
        $"Importation réussie : " +
        $"{importResult.ImportedCount} identité(s).");

    Console.WriteLine();
    Console.WriteLine("Finalisation de l'audit...");

    var completedAudit =
        await apiClient.CompleteAuditAsync(
            createdAudit.Id);

    Console.WriteLine(
        $"Statut final : {completedAudit.Status}");

    Console.WriteLine(
        $"Date de fin : " +
        $"{completedAudit.CompletedAt?.ToString("u") ?? "indisponible"}");

    Console.WriteLine();
    Console.WriteLine(
        "Collecte Microsoft Entra ID simulée terminée avec succès.");

    Console.WriteLine(
        $"Audit concerné : {completedAudit.Id}");

    return 0;
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine(
        $"Impossible de contacter le backend : {exception.Message}");

    return 1;
}
catch (TaskCanceledException)
{
    Console.Error.WriteLine(
        "La communication avec le backend a dépassé le délai autorisé.");

    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine(
        $"Échec de la collecte : {exception.Message}");

    return 1;
}
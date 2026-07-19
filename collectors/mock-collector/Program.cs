using System.Net.Http.Json;
using System.Text.Json;

const string defaultApiBaseUrl = "http://localhost:5173";

if (args.Length == 0 || !Guid.TryParse(args[0], out var targetId))
{
    Console.Error.WriteLine(
        "Utilisation : dotnet run --project collectors/mock-collector -- <targetId> [apiUrl]");

    return 1;
}

var apiBaseUrl = args.Length >= 2
    ? args[1].TrimEnd('/')
    : defaultApiBaseUrl;

var identitiesFilePath = Path.Combine(
    AppContext.BaseDirectory,
    "identities.json");

var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

try
{
    if (!File.Exists(identitiesFilePath))
    {
        Console.Error.WriteLine(
            $"Le fichier est introuvable : {identitiesFilePath}");

        return 1;
    }

    var jsonContent = await File.ReadAllTextAsync(
        identitiesFilePath);

    var identities =
        JsonSerializer.Deserialize<List<CollectedIdentityPayload>>(
            jsonContent,
            jsonOptions);

    if (identities is null || identities.Count == 0)
    {
        Console.Error.WriteLine(
            "Le fichier identities.json ne contient aucune identité.");

        return 1;
    }

    Console.WriteLine(
        $"{identities.Count} identité(s) chargée(s) depuis identities.json.");

    using var httpClient = new HttpClient
    {
        BaseAddress = new Uri(apiBaseUrl)
    };

    Console.WriteLine(
        $"Création d'un audit pour la cible {targetId}...");

    var createAuditResponse = await httpClient.PostAsJsonAsync(
        "/api/audits",
        new CreateAuditRequest
        {
            TargetId = targetId
        },
        jsonOptions);

    if (!createAuditResponse.IsSuccessStatusCode)
    {
        var errorContent =
            await createAuditResponse.Content.ReadAsStringAsync();

        Console.Error.WriteLine(
            $"Échec de création de l'audit : {(int)createAuditResponse.StatusCode}");

        Console.Error.WriteLine(errorContent);

        return 1;
    }

    var audit =
        await createAuditResponse.Content
            .ReadFromJsonAsync<AuditResponse>(jsonOptions);

    if (audit is null || audit.Id == Guid.Empty)
    {
        Console.Error.WriteLine(
            "La réponse de création de l'audit est invalide.");

        return 1;
    }

    Console.WriteLine($"Audit créé : {audit.Id}");
    Console.WriteLine("Démarrage de l'audit...");

    var startAuditResponse = await httpClient.PostAsync(
        $"/api/audits/{audit.Id}/start",
        content: null);

    if (!startAuditResponse.IsSuccessStatusCode)
    {
        var errorContent =
            await startAuditResponse.Content.ReadAsStringAsync();

        Console.Error.WriteLine(
            $"Échec du démarrage de l'audit : {(int)startAuditResponse.StatusCode}");

        Console.Error.WriteLine(errorContent);

        return 1;
    }

    var startedAudit =
        await startAuditResponse.Content
            .ReadFromJsonAsync<AuditResponse>(jsonOptions);

    Console.WriteLine(
        $"Audit démarré avec le statut : {startedAudit?.Status ?? "Running"}");
    Console.WriteLine("Envoi des identités vers l'API...");

    var importResponse = await httpClient.PostAsJsonAsync(
        $"/api/audits/{audit.Id}/identities",
        new ImportIdentitiesRequest
        {
            Identities = identities
        },
        jsonOptions);

    if (!importResponse.IsSuccessStatusCode)
    {
        var errorContent =
            await importResponse.Content.ReadAsStringAsync();

        Console.Error.WriteLine(
            $"Échec de l'importation : {(int)importResponse.StatusCode}");

        Console.Error.WriteLine(errorContent);

        return 1;
    }

    var importResult =
        await importResponse.Content
            .ReadFromJsonAsync<ImportIdentitiesResponse>(
                jsonOptions);

    Console.WriteLine(
    $"Importation réussie : {importResult?.ImportedCount ?? identities.Count} identité(s).");

    Console.WriteLine("Finalisation de l'audit...");

    var completeAuditResponse = await httpClient.PostAsync(
        $"/api/audits/{audit.Id}/complete",
        content: null);

    if (!completeAuditResponse.IsSuccessStatusCode)
    {
        var errorContent =
            await completeAuditResponse.Content.ReadAsStringAsync();

        Console.Error.WriteLine(
            $"Échec de la finalisation de l'audit : {(int)completeAuditResponse.StatusCode}");

        Console.Error.WriteLine(errorContent);

        return 1;
    }

    var completedAudit =
        await completeAuditResponse.Content
            .ReadFromJsonAsync<AuditResponse>(jsonOptions);

    Console.WriteLine(
        $"Audit terminé avec le statut : {completedAudit?.Status ?? "Completed"}");

    Console.WriteLine($"Audit concerné : {audit.Id}");
    Console.WriteLine("Collecte simulée terminée.");

    return 0;
}
catch (HttpRequestException exception)
{
    Console.Error.WriteLine(
        $"Impossible de contacter l'API : {exception.Message}");

    return 1;
}
catch (JsonException exception)
{
    Console.Error.WriteLine(
        $"Le fichier JSON est invalide : {exception.Message}");

    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine(
        $"Une erreur inattendue est survenue : {exception.Message}");

    return 1;
}

public sealed class CreateAuditRequest
{
    public Guid TargetId { get; init; }
}

public sealed class AuditResponse
{
    public Guid Id { get; init; }

    public Guid TargetId { get; init; }

    public string Status { get; init; } = string.Empty;
}

public sealed class ImportIdentitiesRequest
{
    public IReadOnlyCollection<CollectedIdentityPayload> Identities
    {
        get;
        init;
    } = Array.Empty<CollectedIdentityPayload>();
}

public sealed class ImportIdentitiesResponse
{
    public int ImportedCount { get; init; }
}

public sealed class CollectedIdentityPayload
{
    public string ExternalId { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string? Email { get; init; }

    public string Source { get; init; } = string.Empty;

    public string AccountType { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }

    public bool IsPrivileged { get; init; }

    public bool IsServiceAccount { get; init; }

    public bool? IsLocked { get; init; }

    public DateTimeOffset? LastSignInAt { get; init; }

    public string? Description { get; init; }

    public string? Owner { get; init; }
}
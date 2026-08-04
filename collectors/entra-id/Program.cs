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

    Console.WriteLine();
    Console.WriteLine(
        "Collecte des groupes Microsoft Graph simulés...");

    var graphGroupClient =
        new MockGraphGroupClient();

    var graphGroups =
        await graphGroupClient.GetAllGroupsAsync();

    var groupMapper =
        new GraphGroupMapper();

    var groups =
        groupMapper.Map(graphGroups);

    Console.WriteLine();

    Console.WriteLine(
        $"{graphGroups.Count} groupe(s) Microsoft Graph récupéré(s).");

    Console.WriteLine(
        $"{groups.Count} groupe(s) normalisé(s) pour l'API.");

    var knownUserExternalIds =
        graphUsers
            .Select(user => user.Id.Trim())
            .ToHashSet(
                StringComparer.OrdinalIgnoreCase);

    var graphMemberClient =
        new MockGraphGroupMemberClient();

    var membershipMapper =
        new GraphGroupMembershipMapper();

    var memberships =
        new List<
            IdentityAudit.EntraCollector.Models
                .CollectedGroupMembershipPayload>();

    Console.WriteLine();
    Console.WriteLine(
        "Collecte des membres des groupes...");

    foreach (var graphGroup in graphGroups)
    {
        Console.WriteLine();
        Console.WriteLine(
            $"Groupe : {graphGroup.DisplayName}");

        var graphMembers =
            await graphMemberClient.GetAllMembersAsync(
                graphGroup.Id);

        var mappedMemberships =
            membershipMapper.Map(
                graphGroup,
                graphMembers,
                knownUserExternalIds);

        memberships.AddRange(
            mappedMemberships);

        Console.WriteLine(
            $"{mappedMemberships.Count} appartenance(s) utilisateur conservée(s).");
    }

    Console.WriteLine();

    Console.WriteLine(
        $"{memberships.Count} appartenance(s) normalisée(s) au total.");

    Console.WriteLine();

    foreach (var groupPayload in groups)
    {
        Console.WriteLine(
            $"- Groupe : {groupPayload.Name}");

        Console.WriteLine(
            $"  ExternalId : {groupPayload.ExternalId}");

        Console.WriteLine(
            $"  GroupType : {groupPayload.GroupType}");

        Console.WriteLine(
            $"  IsPrivileged : {groupPayload.IsPrivileged}");
    }

    Console.WriteLine();

    foreach (var membership in memberships)
    {
        Console.WriteLine(
            $"- Appartenance : " +
            $"{membership.IdentityExternalId} " +
            $"→ {membership.GroupExternalId}");
    }

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
    Console.WriteLine(
        "Envoi des groupes vers le backend...");

    var groupImportResult =
        await apiClient.ImportGroupsAsync(
            createdAudit.Id,
            groups);

    Console.WriteLine(
        $"Importation réussie : " +
        $"{groupImportResult.ImportedCount} groupe(s).");

    Console.WriteLine();
    Console.WriteLine(
        "Envoi des appartenances vers le backend...");

    var membershipImportResult =
        await apiClient.ImportGroupMembershipsAsync(
            createdAudit.Id,
            memberships);

    Console.WriteLine(
        $"Importation réussie : " +
        $"{membershipImportResult.ImportedCount} appartenance(s).");

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
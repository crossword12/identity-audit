using System.Net.Http.Json;
using System.Text.Json;
using IdentityAudit.EntraCollector.Models;

namespace IdentityAudit.EntraCollector.Services;

public sealed class IdentityAuditApiClient
{
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    public IdentityAuditApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient
            ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<AuditApiResponse> CreateAuditAsync(
        Guid targetId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/audits",
            new CreateAuditPayload
            {
                TargetId = targetId
            },
            _jsonOptions,
            cancellationToken);

        return await ReadRequiredResponseAsync<AuditApiResponse>(
            response,
            "création de l'audit",
            cancellationToken);
    }

    public async Task<AuditApiResponse> StartAuditAsync(
        Guid auditId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(
            $"api/audits/{auditId}/start",
            content: null,
            cancellationToken);

        return await ReadRequiredResponseAsync<AuditApiResponse>(
            response,
            "démarrage de l'audit",
            cancellationToken);
    }

    public async Task<ImportIdentitiesApiResponse> ImportIdentitiesAsync(
        Guid auditId,
        IReadOnlyCollection<CollectedIdentityPayload> identities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identities);

        var response = await _httpClient.PostAsJsonAsync(
            $"api/audits/{auditId}/identities",
            new ImportIdentitiesPayload
            {
                Identities = identities
            },
            _jsonOptions,
            cancellationToken);

        return await ReadRequiredResponseAsync<ImportIdentitiesApiResponse>(
            response,
            "importation des identités",
            cancellationToken);
    }

    public async Task<ImportGroupsApiResponse> ImportGroupsAsync(
    Guid auditId,
    IReadOnlyCollection<CollectedGroupPayload> groups,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(groups);

        var response = await _httpClient.PostAsJsonAsync(
            $"api/audits/{auditId}/groups",
            new ImportGroupsPayload
            {
                Groups = groups
            },
            _jsonOptions,
            cancellationToken);

        return await ReadRequiredResponseAsync<ImportGroupsApiResponse>(
            response,
            "importation des groupes",
            cancellationToken);
    }

    public async Task<ImportGroupMembershipsApiResponse>
        ImportGroupMembershipsAsync(
            Guid auditId,
            IReadOnlyCollection<CollectedGroupMembershipPayload>
                memberships,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(memberships);

        var response = await _httpClient.PostAsJsonAsync(
            $"api/audits/{auditId}/group-memberships",
            new ImportGroupMembershipsPayload
            {
                Memberships = memberships
            },
            _jsonOptions,
            cancellationToken);

        return await ReadRequiredResponseAsync<
            ImportGroupMembershipsApiResponse>(
                response,
                "importation des appartenances aux groupes",
                cancellationToken);
    }

    public async Task<AuditApiResponse> CompleteAuditAsync(
        Guid auditId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync(
            $"api/audits/{auditId}/complete",
            content: null,
            cancellationToken);

        return await ReadRequiredResponseAsync<AuditApiResponse>(
            response,
            "finalisation de l'audit",
            cancellationToken);
    }

    private async Task<T> ReadRequiredResponseAsync<T>(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new InvalidOperationException(
                $"Échec pendant la {operation}. " +
                $"HTTP {(int)response.StatusCode} " +
                $"{response.ReasonPhrase}. " +
                $"Réponse : {errorContent}");
        }

        var result =
            await response.Content.ReadFromJsonAsync<T>(
                _jsonOptions,
                cancellationToken);

        if (result is null)
        {
            throw new InvalidOperationException(
                $"La réponse obtenue pendant la {operation} est vide ou invalide.");
        }

        return result;
    }
}
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace IdentityAudit.Collector.Common.Authentication;

public static class IdentityAuditApiAuthenticator
{
    public const string EmailEnvironmentVariable =
        "IDENTITY_AUDIT_API_EMAIL";

    public const string PasswordEnvironmentVariable =
        "IDENTITY_AUDIT_API_PASSWORD";

    public static async Task AuthenticateAsync(
        HttpClient httpClient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        var email =
            Environment.GetEnvironmentVariable(
                EmailEnvironmentVariable)
            ?.Trim();

        var password =
            Environment.GetEnvironmentVariable(
                PasswordEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException(
                $"La variable {EmailEnvironmentVariable} " +
                "n'est pas configurée.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"La variable {PasswordEnvironmentVariable} " +
                "n'est pas configurée.");
        }

        using var response =
            await httpClient.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest(email, password),
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "L'authentification du collecteur a échoué. " +
                "Vérifiez son adresse électronique, son mot de passe " +
                "et l'état de son compte.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                "L'API a refusé l'authentification du collecteur. " +
                $"HTTP {(int)response.StatusCode} " +
                $"{response.ReasonPhrase}.");
        }

        LoginResponse? loginResponse;

        try
        {
            loginResponse =
                await response.Content
                    .ReadFromJsonAsync<LoginResponse>(
                        cancellationToken);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "La réponse d'authentification de l'API est invalide.",
                exception);
        }

        if (string.IsNullOrWhiteSpace(
            loginResponse?.AccessToken))
        {
            throw new InvalidOperationException(
                "La réponse d'authentification ne contient aucun JWT.");
        }

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResponse.AccessToken);
    }

    private sealed record LoginRequest(
        string Email,
        string Password);

    private sealed record LoginResponse(
        string AccessToken);
}
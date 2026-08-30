using System.Net;
using System.Net.Http.Json;
using IdentityAudit.Application.AuditLogs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IdentityAudit.Api.IntegrationTests;

public sealed class AuditLogEndpointTests
    : IClassFixture<IdentityAuditApiFactory>
{
    private readonly IdentityAuditApiFactory _factory;

    public AuditLogEndpointTests(
        IdentityAuditApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetLogs_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(
            GetLogsUrl(
                FakeAuditExportService.ExistingAuditId));

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetLogs_WithAuditor_ReturnsOk()
    {
        using var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(
            GetLogsUrl(
                FakeAuditExportService.ExistingAuditId));

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var auditLogs =
            await response.Content
                .ReadFromJsonAsync<
                    IReadOnlyList<AuditLogDto>>();

        Assert.NotNull(auditLogs);
        Assert.Empty(auditLogs);
    }

    [Fact]
    public async Task GetLogs_WithUnknownAudit_ReturnsNotFound()
    {
        using var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(
            GetLogsUrl(Guid.NewGuid()));

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress =
                    new Uri("https://localhost")
            });
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();

        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserHeaderName,
            "auditor");

        return client;
    }

    private static string GetLogsUrl(
        Guid auditId)
    {
        return $"/api/audits/{auditId}/logs";
    }
}
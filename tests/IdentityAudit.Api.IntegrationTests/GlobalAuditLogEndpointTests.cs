using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IdentityAudit.Api.IntegrationTests;

public sealed class GlobalAuditLogEndpointTests
    : IClassFixture<IdentityAuditApiFactory>
{
    private readonly IdentityAuditApiFactory _factory;

    public GlobalAuditLogEndpointTests(
        IdentityAuditApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response =
            await client.GetAsync(
                "/api/audit-logs?limit=200");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithAuditor_ReturnsForbidden()
    {
        using var client =
            CreateClient("auditor");

        var response =
            await client.GetAsync(
                "/api/audit-logs?limit=200");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithAdministrator_ReturnsJsonArray()
    {
        using var client =
            CreateClient("administrator");

        var response =
            await client.GetAsync(
                "/api/audit-logs?limit=200");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content.ReadAsStringAsync();

        using var document =
            JsonDocument.Parse(json);

        Assert.Equal(
            JsonValueKind.Array,
            document.RootElement.ValueKind);
    }

    private HttpClient CreateClient(
        string? testUser = null)
    {
        var client =
            _factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress =
                        new Uri("https://localhost")
                });

        if (!string.IsNullOrWhiteSpace(testUser))
        {
            client.DefaultRequestHeaders.Add(
                TestAuthenticationHandler.UserHeaderName,
                testUser);
        }

        return client;
    }
}
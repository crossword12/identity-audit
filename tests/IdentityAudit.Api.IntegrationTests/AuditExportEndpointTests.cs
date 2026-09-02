using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using IdentityAudit.Application.AuditExports;
using IdentityAudit.Application.Authentication;
using IdentityAudit.Application.AuditLogs;
using IdentityAudit.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityAudit.Api.IntegrationTests;

public sealed class AuditExportEndpointTests
    : IClassFixture<IdentityAuditApiFactory>
{
    private readonly IdentityAuditApiFactory _factory;

    public AuditExportEndpointTests(
        IdentityAuditApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ExportCsv_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(
            GetExportUrl(
                FakeAuditExportService.ExistingAuditId));

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task ExportCsv_WithAuditor_ReturnsDownloadableUtf8Csv()
    {
        using var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(
            GetExportUrl(
                FakeAuditExportService.ExistingAuditId));

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Equal(
            "text/csv",
            response.Content.Headers.ContentType?.MediaType);

        Assert.Equal(
            "utf-8",
            response.Content.Headers.ContentType?.CharSet);

        Assert.True(
            response.Headers.CacheControl?.NoStore);

        var expectedFileName =
            $"audit-" +
            $"{FakeAuditExportService.ExistingAuditId:N}" +
            "-resultats-cis.csv";

        var contentDisposition =
            response.Content.Headers.ContentDisposition;

        var receivedFileName =
            contentDisposition?.FileNameStar
            ?? contentDisposition?.FileName?.Trim('"');

        Assert.Equal(
            expectedFileName,
            receivedFileName);

        var content =
            await response.Content.ReadAsByteArrayAsync();

        Assert.True(content.Length >= 3);
        Assert.Equal(0xEF, content[0]);
        Assert.Equal(0xBB, content[1]);
        Assert.Equal(0xBF, content[2]);

        var csvText =
            Encoding.UTF8.GetString(
                content,
                3,
                content.Length - 3);

        var lines =
            csvText.Split(
                new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        Assert.Contains(
            "AD-05-01",
            lines[1]);
    }

    [Fact]
    public async Task ExportCsv_WithUnknownAudit_ReturnsNotFound()
    {
        using var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(
            GetExportUrl(Guid.NewGuid()));

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task ExportPdf_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync(
            GetPdfExportUrl(
                FakeAuditExportService.ExistingAuditId));

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task ExportPdf_WithAuditor_ReturnsDownloadablePdf()
    {
        using var client =
            CreateAuthenticatedClient();

        var response = await client.GetAsync(
            GetPdfExportUrl(
                FakeAuditExportService.ExistingAuditId));

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Equal(
            "application/pdf",
            response.Content.Headers.ContentType?.MediaType);

        Assert.True(
            response.Headers.CacheControl?.NoStore);

        var expectedFileName =
            $"audit-" +
            $"{FakeAuditExportService.ExistingAuditId:N}" +
            "-rapport.pdf";

        var contentDisposition =
            response.Content.Headers.ContentDisposition;

        var receivedFileName =
            contentDisposition?.FileNameStar
            ?? contentDisposition?.FileName?.Trim('"');

        Assert.Equal(
            expectedFileName,
            receivedFileName);

        var content =
            await response.Content.ReadAsByteArrayAsync();

        Assert.True(content.Length >= 5);

        Assert.Equal(
            (byte)'%',
            content[0]);

        Assert.Equal(
            (byte)'P',
            content[1]);

        Assert.Equal(
            (byte)'D',
            content[2]);

        Assert.Equal(
            (byte)'F',
            content[3]);

        Assert.Equal(
            (byte)'-',
            content[4]);
    }

    [Fact]
    public async Task ExportPdf_WithUnknownAudit_ReturnsNotFound()
    {
        using var client =
            CreateAuthenticatedClient();

        var response = await client.GetAsync(
            GetPdfExportUrl(Guid.NewGuid()));

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

    private static string GetExportUrl(
        Guid auditId)
    {
        return $"/api/audits/{auditId}/export.csv";
    }
    private static string GetPdfExportUrl(
    Guid auditId)
    {
        return $"/api/audits/{auditId}/export.pdf";
    }
}

public sealed class IdentityAuditApiFactory
    : WebApplicationFactory<Program>
{
    private const string TestConnectionString =
    "Host=127.0.0.1;" +
    "Port=1;" +
    "Database=identity_audit_tests;" +
    "Username=test;" +
    "Password=test;" +
    "Timeout=1";

    public IdentityAuditApiFactory()
    {
        Environment.SetEnvironmentVariable(
            "DOTNET_ENVIRONMENT",
            "Testing");

        Environment.SetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT",
            "Testing");

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            TestConnectionString);
    }
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");



        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAuditExportService>();

            services.AddSingleton<
                IAuditExportService,
                FakeAuditExportService>();

            services.RemoveAll<IAuditLogService>();

            services.AddSingleton<
                IAuditLogService,
                FakeAuditLogService>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        TestAuthenticationHandler.SchemeName;

                    options.DefaultChallengeScheme =
                        TestAuthenticationHandler.SchemeName;

                    options.DefaultForbidScheme =
                        TestAuthenticationHandler.SchemeName;
                })
                .AddScheme<
                    AuthenticationSchemeOptions,
                    TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ =>
                    {
                    });
        });
    }
}

internal sealed class TestAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName =
        "IdentityAuditTest";

    public const string UserHeaderName =
        "X-IdentityAudit-Test-User";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(
            options,
            logger,
            encoder)
    {
    }

    protected override Task<AuthenticateResult>
    HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(
                UserHeaderName,
                out var userValue))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var testUser =
            userValue
                .ToString()
                .Trim()
                .ToLowerInvariant();

        var role =
            testUser switch
            {
                "auditor" =>
                    ApplicationRoles.Auditor,

                "administrator" =>
                    ApplicationRoles.Administrator,

                _ => null
            };

        if (role is null)
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var displayName =
            role == ApplicationRoles.Administrator
                ? "Administrateur de test"
                : "Auditeur de test";

        var claims = new[]
        {
        new Claim(
            ClaimTypes.NameIdentifier,
            $"integration-test-{testUser}"),

        new Claim(
            ClaimTypes.Name,
            displayName),

        new Claim(
            ClaimTypes.Role,
            role)
    };

        var identity =
            new ClaimsIdentity(
                claims,
                SchemeName);

        var principal =
            new ClaimsPrincipal(identity);

        var ticket =
            new AuthenticationTicket(
                principal,
                SchemeName);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}

internal sealed class FakeAuditExportService
    : IAuditExportService
{
    public static readonly Guid ExistingAuditId =
        Guid.Parse(
            "3c2fc841-a829-4636-9211-f66b5da6865e");

    private static readonly byte[] CsvContent =
        CreateCsvContent();

    private static readonly byte[] PdfContent =
    Encoding.ASCII.GetBytes(
        "%PDF-1.4\n% Identity Audit integration test\n");

    public Task<AuditCsvExportResult> ExportCsvAsync(
        Guid auditId,
        CancellationToken cancellationToken = default)
    {
        if (auditId != ExistingAuditId)
        {
            return Task.FromResult(
                AuditCsvExportResult.Failure(
                    "Audit introuvable."));
        }

        return Task.FromResult(
            AuditCsvExportResult.Success(
                CsvContent,
                $"audit-{auditId:N}-resultats-cis.csv"));
    }

    public Task<AuditPdfExportResult> ExportPdfAsync(
    Guid auditId,
    CancellationToken cancellationToken = default)
    {
        if (auditId != ExistingAuditId)
        {
            return Task.FromResult(
                AuditPdfExportResult.Failure(
                    "Audit introuvable."));
        }

        return Task.FromResult(
            AuditPdfExportResult.Success(
                PdfContent,
                $"audit-{auditId:N}-rapport.pdf"));
    }

    private static byte[] CreateCsvContent()
    {
        var csv =
            "\"IdentifiantAudit\";\"CodeRegle\"\r\n" +
            $"\"{ExistingAuditId}\";\"AD-05-01\"\r\n";

        return Encoding.UTF8
            .GetPreamble()
            .Concat(
                Encoding.UTF8.GetBytes(csv))
            .ToArray();
    }

    internal sealed class FakeAuditLogService
    : IAuditLogService
    {
        public Task RecordAsync(
            Guid? auditId,
            Guid? applicationUserId,
            AuditLogLevel level,
            string eventType,
            string message,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AuditLogDto>?>
            GetByAuditAsync(
                Guid auditId,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyList<AuditLogDto>? result =
                auditId ==
                FakeAuditExportService.ExistingAuditId
                    ? Array.Empty<AuditLogDto>()
                    : null;

            return Task.FromResult(result);
        }
        public Task<IReadOnlyList<AuditLogDto>> GetAllAsync(
            int maximumCount,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<AuditLogDto> auditLogs =
                Array.Empty<AuditLogDto>();

            return Task.FromResult(auditLogs);
        }
    }
}
internal sealed class FakeAuditLogService
    : IAuditLogService
{
    public Task RecordAsync(
        Guid? auditId,
        Guid? applicationUserId,
        AuditLogLevel level,
        string eventType,
        string message,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditLogDto>?>
        GetByAuditAsync(
            Guid auditId,
            CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AuditLogDto>? result =
            auditId ==
            FakeAuditExportService.ExistingAuditId
                ? Array.Empty<AuditLogDto>()
                : null;

        return Task.FromResult(result);
    }
    public Task<IReadOnlyList<AuditLogDto>> GetAllAsync(
    int maximumCount,
    CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AuditLogDto> auditLogs =
            Array.Empty<AuditLogDto>();

        return Task.FromResult(auditLogs);
    }
}
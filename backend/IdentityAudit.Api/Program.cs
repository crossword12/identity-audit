using IdentityAudit.Api.Endpoints;
using IdentityAudit.Application.Audits;
using IdentityAudit.Application.Identities;
using IdentityAudit.Application.Targets;
using IdentityAudit.Application.Dashboard;
using IdentityAudit.Infrastructure.Persistence;
using IdentityAudit.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "La chaîne de connexion 'DefaultConnection' est absente.");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

builder.Services.AddValidation();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<IdentityAuditDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ITargetService, TargetService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapApiEndpoints();

app.Run();
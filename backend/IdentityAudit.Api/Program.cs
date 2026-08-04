using IdentityAudit.Api.Endpoints;
using IdentityAudit.Application.Audits;
using IdentityAudit.Application.Identities;
using IdentityAudit.Application.Targets;
using IdentityAudit.Application.Groups;
using IdentityAudit.Application.GroupMemberships;
using IdentityAudit.Application.Roles;
using IdentityAudit.Application.RoleAssignments;
using IdentityAudit.Application.Dashboard;
using IdentityAudit.Infrastructure.Persistence;
using IdentityAudit.Infrastructure.Persistence.Seeding;
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
builder.Services.AddScoped<
    IDirectoryGroupService,
    DirectoryGroupService>();
builder.Services.AddScoped<
    IGroupMembershipService,
    GroupMembershipService>();
builder.Services.AddScoped<
    IDirectoryRoleService,
    DirectoryRoleService>();
builder.Services.AddScoped<
    IRoleAssignmentService,
    RoleAssignmentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<IdentityAuditDbContext>();

    var insertedRuleCount =
        await AuditRuleCatalogSeeder.SeedAsync(
            dbContext);

    Console.WriteLine(
        "Catalogue CIS initialisé : " +
        $"{insertedRuleCount} nouvelle(s) règle(s) ajoutée(s).");
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapApiEndpoints();

app.Run();
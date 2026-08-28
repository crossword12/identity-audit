using IdentityAudit.Api.Endpoints;
using IdentityAudit.Api.Authentication;
using IdentityAudit.Api.Authorization;
using IdentityAudit.Application.Audits;
using IdentityAudit.Application.Identities;
using IdentityAudit.Application.Targets;
using IdentityAudit.Application.Groups;
using IdentityAudit.Application.GroupMemberships;
using IdentityAudit.Application.Roles;
using IdentityAudit.Application.RoleAssignments;
using IdentityAudit.Application.RuleEvaluations;
using IdentityAudit.Application.Dashboard;
using IdentityAudit.Application.Authentication;
using IdentityAudit.Application.ApplicationUsers;
using IdentityAudit.Infrastructure.Persistence;
using IdentityAudit.Infrastructure.Persistence.Seeding;
using IdentityAudit.Infrastructure.Services;
using IdentityAudit.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<IdentityAuditDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(
        JwtOptions.SectionName));

builder.Services.AddDataProtection();

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredUniqueChars = 4;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);

        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<IdentityAuditDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddJwtAuthentication(
    builder.Configuration);

builder.Services.AddIdentityAuditAuthorization();

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
builder.Services.AddScoped<
    IAuditRuleEvaluationService,
    AuditRuleEvaluationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(
        JwtOptions.SectionName));

builder.Services.AddScoped<
    IAccessTokenService,
    JwtTokenService>();

builder.Services.AddScoped<
    IApplicationUserService,
    ApplicationUserService>();

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

    var roleManager =
        scope.ServiceProvider
            .GetRequiredService<
                RoleManager<IdentityRole<Guid>>>();

    var userManager =
        scope.ServiceProvider
            .GetRequiredService<
                UserManager<ApplicationUser>>();

    var identitySeedResult =
        await ApplicationIdentitySeeder.SeedAsync(
            roleManager,
            userManager,
            app.Configuration[
                "Authentication:InitialAdmin:Email"],
            app.Configuration[
                "Authentication:InitialAdmin:DisplayName"],
            app.Configuration[
                "Authentication:InitialAdmin:Password"]);

    Console.WriteLine(
        "Rôles applicatifs initialisés : " +
        $"{identitySeedResult.CreatedRoleCount} " +
        "nouveau(x) rôle(s).");

    Console.WriteLine(
        identitySeedResult.AdministratorConfigured
            ? identitySeedResult.AdministratorCreated
                ? "Administrateur initial créé."
                : "Administrateur initial déjà présent."
            : "Aucun administrateur initial configuré.");
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapApiEndpoints();

app.Run();
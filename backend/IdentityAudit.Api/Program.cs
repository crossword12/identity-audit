using IdentityAudit.Application.Targets;
using IdentityAudit.Infrastructure.Persistence;
using IdentityAudit.Infrastructure.Services;
using IdentityAudit.Application.Audits;
using IdentityAudit.Application.Identities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "La chaîne de connexion 'DefaultConnection' est absente.");

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

builder.Services.AddDbContext<IdentityAuditDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ITargetService, TargetService>();

builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddScoped<IIdentityService, IdentityService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
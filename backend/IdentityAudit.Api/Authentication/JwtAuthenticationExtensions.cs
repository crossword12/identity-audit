using IdentityAudit.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace IdentityAudit.Api.Authentication;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection =
            configuration.GetRequiredSection(
                JwtOptions.SectionName);

        var jwtOptions =
            jwtSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "La configuration JWT est absente.");

        jwtOptions.Validate();

        services.Configure<JwtOptions>(jwtSection);

        services
            .AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                jwtOptions.GetSigningKeyBytes()),

                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,

                        NameClaimType = "name",
                        RoleClaimType = "role",

                        ClockSkew =
                            TimeSpan.FromSeconds(30)
                    };
            });

        return services;
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityAudit.Application.Authentication;
using IdentityAudit.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;

namespace IdentityAudit.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder
        MapAuthenticationEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .Produces<LoginResponse>(
                StatusCodes.Status200OK)
            .Produces(
                StatusCodes.Status401Unauthorized);

        group.MapGet("/me", GetCurrentUserAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .Produces<AuthenticatedUserDto>(
                StatusCodes.Status200OK)
            .Produces(
                StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAccessTokenService tokenService)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return AuthenticationFailed();
        }

        var email = request.Email.Trim();

        var user = await userManager.FindByEmailAsync(
            email);

        if (user is null || !user.IsEnabled)
        {
            return AuthenticationFailed();
        }

        var signInResult =
            await signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            return AuthenticationFailed();
        }

        var roles = (await userManager.GetRolesAsync(user))
            .OrderBy(role => role)
            .ToArray();

        var token = tokenService.CreateToken(
            new AccessTokenRequest(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                roles));

        var authenticatedUser =
            new AuthenticatedUserDto(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                roles);

        return Results.Ok(
            new LoginResponse(
                token.AccessToken,
                "Bearer",
                token.ExpiresAt,
                authenticatedUser));
    }

    private static async Task<IResult> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager)
    {
        var userId =
            principal
                .FindFirst(
                    JwtRegisteredClaimNames.Sub)
                ?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Results.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(
            userId);

        if (user is null || !user.IsEnabled)
        {
            return Results.Unauthorized();
        }

        var roles = (await userManager.GetRolesAsync(user))
            .OrderBy(role => role)
            .ToArray();

        return Results.Ok(
            new AuthenticatedUserDto(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                roles));
    }

    private static IResult AuthenticationFailed()
    {
        return Results.Json(
            new
            {
                message =
                    "Adresse électronique ou mot de passe incorrect."
            },
            statusCode:
                StatusCodes.Status401Unauthorized);
    }
}
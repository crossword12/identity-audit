using System.Security.Claims;
using IdentityAudit.Application.ApplicationUsers;
using IdentityAudit.Application.Authentication;

namespace IdentityAudit.Api.Endpoints;

public static class ApplicationUserEndpoints
{
    public static IEndpointRouteBuilder
        MapApplicationUserEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/application-users")
            .WithTags("Application Users")
            .RequireAuthorization(
                policy => policy.RequireRole(
                    ApplicationRoles.Administrator));

        group.MapGet("/", GetAllAsync)
            .WithName("GetApplicationUsers");

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetApplicationUserById");

        group.MapPost("/", CreateAsync)
            .WithName("CreateApplicationUser");

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateApplicationUser");

        group.MapPut("/{id:guid}/roles", UpdateRolesAsync)
            .WithName("UpdateApplicationUserRoles");

        return endpoints;
    }

    private static async Task<IResult> GetAllAsync(
        IApplicationUserService userService,
        CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(
            cancellationToken);

        return Results.Ok(users);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IApplicationUserService userService,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(
            id,
            cancellationToken);

        if (user is null)
        {
            return Results.NotFound(new
            {
                message =
                    "L'utilisateur demandé est introuvable."
            });
        }

        return Results.Ok(user);
    }

    private static async Task<IResult> CreateAsync(
        CreateApplicationUserRequest request,
        IApplicationUserService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.CreateAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return ToErrorResult(result);
        }

        return Results.CreatedAtRoute(
            "GetApplicationUserById",
            new { id = result.User!.Id },
            result.User);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateApplicationUserRequest request,
        ClaimsPrincipal principal,
        IApplicationUserService userService,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(
                principal,
                out var currentUserId))
        {
            return Results.Unauthorized();
        }

        var result = await userService.UpdateAsync(
            id,
            currentUserId,
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return ToErrorResult(result);
        }

        return Results.Ok(result.User);
    }

    private static async Task<IResult> UpdateRolesAsync(
        Guid id,
        UpdateApplicationUserRolesRequest request,
        ClaimsPrincipal principal,
        IApplicationUserService userService,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(
                principal,
                out var currentUserId))
        {
            return Results.Unauthorized();
        }

        var result = await userService.UpdateRolesAsync(
            id,
            currentUserId,
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return ToErrorResult(result);
        }

        return Results.Ok(result.User);
    }

    private static bool TryGetCurrentUserId(
        ClaimsPrincipal principal,
        out Guid currentUserId)
    {
        return Guid.TryParse(
            principal.FindFirst("sub")?.Value,
            out currentUserId);
    }

    private static IResult ToErrorResult(
        ApplicationUserOperationResult result)
    {
        var response = new
        {
            message = result.ErrorMessage
        };

        return result.ErrorType switch
        {
            ApplicationUserErrorType.NotFound =>
                Results.NotFound(response),

            ApplicationUserErrorType.Conflict =>
                Results.Conflict(response),

            ApplicationUserErrorType.Validation =>
                Results.BadRequest(response),

            ApplicationUserErrorType.Forbidden =>
                Results.Json(
                    response,
                    statusCode:
                        StatusCodes.Status403Forbidden),

            _ => Results.Problem(
                title:
                    "La gestion de l'utilisateur a échoué.",
                statusCode:
                    StatusCodes.Status500InternalServerError)
        };
    }
}
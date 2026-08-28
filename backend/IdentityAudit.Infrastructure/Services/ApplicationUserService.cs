using IdentityAudit.Application.ApplicationUsers;
using IdentityAudit.Application.Authentication;
using IdentityAudit.Infrastructure.Authentication;
using IdentityAudit.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityAudit.Infrastructure.Services;

public sealed class ApplicationUserService
    : IApplicationUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityAuditDbContext _dbContext;

    public ApplicationUserService(
        UserManager<ApplicationUser> userManager,
        IdentityAuditDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ApplicationUserDto>>
        GetAllAsync(
            CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .ToListAsync(cancellationToken);

        var result =
            new List<ApplicationUserDto>(users.Count);

        foreach (var user in users)
        {
            result.Add(await ToDtoAsync(user));
        }

        return result;
    }

    public async Task<ApplicationUserDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(
            id.ToString());

        return user is null
            ? null
            : await ToDtoAsync(user);
    }

    public async Task<ApplicationUserOperationResult>
        CreateAsync(
            CreateApplicationUserRequest request,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryNormalizeRoles(
                request.Roles,
                out var roles,
                out var roleError))
        {
            return ApplicationUserOperationResult.Failure(
                ApplicationUserErrorType.Validation,
                roleError!);
        }

        var email = request.Email.Trim();

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return ApplicationUserOperationResult.Failure(
                ApplicationUserErrorType.Conflict,
                "Cette adresse électronique est déjà utilisée.");
        }

        var now = DateTimeOffset.UtcNow;

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            DisplayName = request.DisplayName.Trim(),
            IsEnabled = true,
            LockoutEnabled = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var creationResult =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!creationResult.Succeeded)
        {
            return FromIdentityFailure(creationResult);
        }

        var roleResult =
            await _userManager.AddToRolesAsync(
                user,
                roles);

        if (!roleResult.Succeeded)
        {
            return FromIdentityFailure(roleResult);
        }

        await transaction.CommitAsync(cancellationToken);

        return ApplicationUserOperationResult.Success(
            await ToDtoAsync(user));
    }

    public async Task<ApplicationUserOperationResult>
        UpdateAsync(
            Guid id,
            Guid currentUserId,
            UpdateApplicationUserRequest request,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(
            id.ToString());

        if (user is null)
        {
            return ApplicationUserOperationResult.Failure(
                ApplicationUserErrorType.NotFound,
                "L'utilisateur demandé est introuvable.");
        }

        if (id == currentUserId && !request.IsEnabled)
        {
            return ApplicationUserOperationResult.Failure(
                ApplicationUserErrorType.Forbidden,
                "Vous ne pouvez pas désactiver votre propre compte.");
        }

        if (!request.IsEnabled &&
            await IsLastEnabledAdministratorAsync(user))
        {
            return ApplicationUserOperationResult.Failure(
                ApplicationUserErrorType.Forbidden,
                "Le dernier administrateur actif ne peut pas être désactivé.");
        }

        var email = request.Email.Trim();

        var emailOwner =
            await _userManager.FindByEmailAsync(email);

        if (emailOwner is not null &&
            emailOwner.Id != user.Id)
        {
            return ApplicationUserOperationResult.Failure(
                ApplicationUserErrorType.Conflict,
                "Cette adresse électronique est déjà utilisée.");
        }

        user.Email = email;
        user.UserName = email;
        user.DisplayName = request.DisplayName.Trim();
        user.IsEnabled = request.IsEnabled;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return FromIdentityFailure(updateResult);
        }

        return ApplicationUserOperationResult.Success(
            await ToDtoAsync(user));
    }

    public async Task<ApplicationUserOperationResult>
        UpdateRolesAsync(
            Guid id,
            Guid currentUserId,
            UpdateApplicationUserRolesRequest request,
            CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(
            id.ToString());

        if (user is null)
        {
            return ApplicationUserOperationResult.Failure(
                ApplicationUserErrorType.NotFound,
                "L'utilisateur demandé est introuvable.");
        }

        if (!TryNormalizeRoles(
                request.Roles,
                out var requestedRoles,
                out var roleError))
        {
            return ApplicationUserOperationResult.Failure(
                ApplicationUserErrorType.Validation,
                roleError!);
        }

        var currentRoles =
            (await _userManager.GetRolesAsync(user))
                .ToArray();

        var removesAdministrator =
            currentRoles.Contains(
                ApplicationRoles.Administrator,
                StringComparer.OrdinalIgnoreCase) &&
            !requestedRoles.Contains(
                ApplicationRoles.Administrator,
                StringComparer.OrdinalIgnoreCase);

        if (id == currentUserId &&
            removesAdministrator)
        {
            return ApplicationUserOperationResult.Failure(
                ApplicationUserErrorType.Forbidden,
                "Vous ne pouvez pas retirer votre propre rôle administrateur.");
        }

        if (removesAdministrator &&
            await IsLastEnabledAdministratorAsync(user))
        {
            return ApplicationUserOperationResult.Failure(
                ApplicationUserErrorType.Forbidden,
                "Le rôle du dernier administrateur actif ne peut pas être retiré.");
        }

        var rolesToAdd = requestedRoles
            .Except(
                currentRoles,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var rolesToRemove = currentRoles
            .Except(
                requestedRoles,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        if (rolesToAdd.Length > 0)
        {
            var addResult =
                await _userManager.AddToRolesAsync(
                    user,
                    rolesToAdd);

            if (!addResult.Succeeded)
            {
                return FromIdentityFailure(addResult);
            }
        }

        if (rolesToRemove.Length > 0)
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    rolesToRemove);

            if (!removeResult.Succeeded)
            {
                return FromIdentityFailure(removeResult);
            }
        }

        user.UpdatedAt = DateTimeOffset.UtcNow;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return FromIdentityFailure(updateResult);
        }

        await transaction.CommitAsync(cancellationToken);

        return ApplicationUserOperationResult.Success(
            await ToDtoAsync(user));
    }

    private async Task<ApplicationUserDto> ToDtoAsync(
        ApplicationUser user)
    {
        var roles = (await _userManager.GetRolesAsync(user))
            .OrderBy(
                role => role,
                StringComparer.Ordinal)
            .ToArray();

        return new ApplicationUserDto(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.IsEnabled,
            roles,
            user.CreatedAt,
            user.UpdatedAt);
    }

    private async Task<bool>
        IsLastEnabledAdministratorAsync(
            ApplicationUser user)
    {
        if (!user.IsEnabled ||
            !await _userManager.IsInRoleAsync(
                user,
                ApplicationRoles.Administrator))
        {
            return false;
        }

        var administrators =
            await _userManager.GetUsersInRoleAsync(
                ApplicationRoles.Administrator);

        return administrators.All(
            administrator =>
                administrator.Id == user.Id ||
                !administrator.IsEnabled);
    }

    private static bool TryNormalizeRoles(
        IEnumerable<string>? requestedRoles,
        out string[] normalizedRoles,
        out string? errorMessage)
    {
        var roles = requestedRoles?.ToArray() ?? [];

        if (roles.Length == 0)
        {
            normalizedRoles = [];
            errorMessage =
                "Au moins un rôle doit être attribué.";

            return false;
        }

        var result = new List<string>();

        foreach (var requestedRole in roles)
        {
            var trimmedRole = requestedRole?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedRole))
            {
                normalizedRoles = [];
                errorMessage =
                    "Un rôle vide n'est pas autorisé.";

                return false;
            }

            var allowedRole =
                ApplicationRoles.All.FirstOrDefault(
                    role => string.Equals(
                        role,
                        trimmedRole,
                        StringComparison.OrdinalIgnoreCase));

            if (allowedRole is null)
            {
                normalizedRoles = [];
                errorMessage =
                    $"Le rôle '{trimmedRole}' n'est pas autorisé.";

                return false;
            }

            if (!result.Contains(
                    allowedRole,
                    StringComparer.OrdinalIgnoreCase))
            {
                result.Add(allowedRole);
            }
        }

        normalizedRoles = result
            .OrderBy(
                role => role,
                StringComparer.Ordinal)
            .ToArray();

        errorMessage = null;

        return true;
    }

    private static ApplicationUserOperationResult
        FromIdentityFailure(
            IdentityResult identityResult)
    {
        var errors = identityResult.Errors.ToArray();

        var errorType = errors.Any(
            error =>
                error.Code is
                    "DuplicateEmail" or
                    "DuplicateUserName")
            ? ApplicationUserErrorType.Conflict
            : ApplicationUserErrorType.Validation;

        var message = string.Join(
            " ",
            errors.Select(TranslateIdentityError));

        return ApplicationUserOperationResult.Failure(
            errorType,
            string.IsNullOrWhiteSpace(message)
                ? "L'opération sur l'utilisateur a échoué."
                : message);
    }

    private static string TranslateIdentityError(
        IdentityError error)
    {
        return error.Code switch
        {
            "DuplicateEmail" or "DuplicateUserName" =>
                "Cette adresse électronique est déjà utilisée.",

            "InvalidEmail" =>
                "L'adresse électronique est invalide.",

            "InvalidUserName" =>
                "Le nom d'utilisateur est invalide.",

            "PasswordTooShort" =>
                "Le mot de passe est trop court.",

            "PasswordRequiresDigit" =>
                "Le mot de passe doit contenir un chiffre.",

            "PasswordRequiresLower" =>
                "Le mot de passe doit contenir une minuscule.",

            "PasswordRequiresUpper" =>
                "Le mot de passe doit contenir une majuscule.",

            "PasswordRequiresNonAlphanumeric" =>
                "Le mot de passe doit contenir un caractère spécial.",

            "PasswordRequiresUniqueChars" =>
                "Le mot de passe doit contenir davantage de caractères différents.",

            _ => error.Description
        };
    }
}
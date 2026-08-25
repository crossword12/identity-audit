using IdentityAudit.Application.Authentication;
using Microsoft.AspNetCore.Identity;

namespace IdentityAudit.Infrastructure.Authentication;

public static class ApplicationIdentitySeeder
{
    public static async Task<ApplicationIdentitySeedResult> SeedAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        string? administratorEmail,
        string? administratorDisplayName,
        string? administratorPassword)
    {
        ArgumentNullException.ThrowIfNull(roleManager);
        ArgumentNullException.ThrowIfNull(userManager);

        var createdRoleCount = 0;

        foreach (var roleName in ApplicationRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var role = new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = roleName
            };

            var roleCreationResult =
                await roleManager.CreateAsync(role);

            EnsureSucceeded(
                roleCreationResult,
                $"La création du rôle '{roleName}'");

            createdRoleCount++;
        }

        var administratorSettings = new[]
        {
            administratorEmail,
            administratorDisplayName,
            administratorPassword
        };

        var hasAnyAdministratorSetting =
            administratorSettings.Any(
                value => !string.IsNullOrWhiteSpace(value));

        if (!hasAnyAdministratorSetting)
        {
            return new ApplicationIdentitySeedResult(
                createdRoleCount,
                AdministratorConfigured: false,
                AdministratorCreated: false);
        }

        var hasAllAdministratorSettings =
            administratorSettings.All(
                value => !string.IsNullOrWhiteSpace(value));

        if (!hasAllAdministratorSettings)
        {
            throw new InvalidOperationException(
                "La configuration de l'administrateur initial est " +
                "incomplète. Les paramètres Email, DisplayName et " +
                "Password doivent tous être renseignés.");
        }

        var email = administratorEmail!.Trim();
        var displayName = administratorDisplayName!.Trim();
        var password = administratorPassword!;

        var administrator =
            await userManager.FindByEmailAsync(email);

        var administratorCreated = false;

        if (administrator is null)
        {
            var now = DateTimeOffset.UtcNow;

            administrator = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName,
                IsEnabled = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            var userCreationResult =
                await userManager.CreateAsync(
                    administrator,
                    password);

            EnsureSucceeded(
                userCreationResult,
                "La création de l'administrateur initial");

            administratorCreated = true;
        }

        var isAdministrator =
            await userManager.IsInRoleAsync(
                administrator,
                ApplicationRoles.Administrator);

        if (!isAdministrator)
        {
            var roleAssignmentResult =
                await userManager.AddToRoleAsync(
                    administrator,
                    ApplicationRoles.Administrator);

            EnsureSucceeded(
                roleAssignmentResult,
                "L'attribution du rôle Administrator");
        }

        return new ApplicationIdentitySeedResult(
            createdRoleCount,
            AdministratorConfigured: true,
            AdministratorCreated: administratorCreated);
    }

    private static void EnsureSucceeded(
        IdentityResult result,
        string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            "; ",
            result.Errors.Select(
                error =>
                    $"{error.Code}: {error.Description}"));

        throw new InvalidOperationException(
            $"{operation} a échoué. {errors}");
    }
}

public sealed record ApplicationIdentitySeedResult(
    int CreatedRoleCount,
    bool AdministratorConfigured,
    bool AdministratorCreated);
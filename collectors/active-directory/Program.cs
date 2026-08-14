using System.DirectoryServices.Protocols;
using IdentityAudit.ActiveDirectoryCollector.Configuration;
using IdentityAudit.ActiveDirectoryCollector.Services;

try
{
    Console.WriteLine(
        "Démarrage du collecteur Active Directory.");

    Console.WriteLine(
        "Chargement de la configuration LDAP...");

    var options =
        ActiveDirectoryOptions.FromEnvironment();

    options.Validate();

    Console.WriteLine(
        $"Serveur : {options.Host}:{options.Port}");

    Console.WriteLine(
        $"Protocole : {(options.UseSsl ? "LDAPS" : "LDAP")}");

    Console.WriteLine(
        $"Base DN : {options.BaseDn}");

    if (!options.UseSsl)
    {
        Console.WriteLine();
        Console.WriteLine(
            "Attention : LDAP sans TLS est réservé " +
            "à l'environnement de laboratoire isolé.");
    }

    Console.WriteLine();
    Console.WriteLine(
        "Test de connexion et d'authentification...");

    var connectionTester =
        new ActiveDirectoryConnectionTester(
            options);

    var result =
        connectionTester.Test();

    Console.WriteLine();
    Console.WriteLine(
        "Connexion Active Directory réussie.");

    Console.WriteLine(
        $"Contrôleur de domaine : " +
        $"{result.ServerDnsHostName ?? "indisponible"}");

    Console.WriteLine(
        $"Domaine détecté : " +
        $"{result.DefaultNamingContext ?? "indisponible"}");

    Console.WriteLine(
        "Versions LDAP prises en charge : " +
        (result.SupportedLdapVersions.Count > 0
            ? string.Join(
                ", ",
                result.SupportedLdapVersions)
            : "indisponibles"));

    Console.WriteLine(
        $"Base DN validé : {result.ConfiguredBaseDn}");
    Console.WriteLine();
    Console.WriteLine(
        "Collecte des utilisateurs Active Directory...");

    var userCollector =
        new ActiveDirectoryUserCollector(
            options);

    var users =
        userCollector.Collect();

    Console.WriteLine();
    Console.WriteLine(
        $"{users.Count} utilisateur(s) Active Directory récupéré(s).");

    Console.WriteLine();

    foreach (var user in users)
    {
        Console.WriteLine(
            $"- {user.SamAccountName}");

        Console.WriteLine(
            $"  ObjectGuid : {user.ObjectGuid}");

        Console.WriteLine(
            $"  UPN : " +
            $"{user.UserPrincipalName ?? "indisponible"}");

        Console.WriteLine(
            $"  DisplayName : " +
            $"{user.DisplayName ?? "indisponible"}");

        Console.WriteLine(
            $"  UserAccountControl : " +
            $"{user.UserAccountControl}");

        Console.WriteLine(
            $"  ComputedUAC : " +
            $"{user.ComputedUserAccountControl?.ToString() ?? "indisponible"}");

        Console.WriteLine(
            $"  LastLogon : " +
            $"{user.LastLogonAt?.ToString("u") ?? "indisponible"}");

        Console.WriteLine(
            $"  SPN : {user.ServicePrincipalNames.Count}");
    }
    Console.WriteLine();
    Console.WriteLine(
        "Normalisation des utilisateurs pour l'API...");

    var userMapper =
        new ActiveDirectoryUserMapper();

    var identities =
        userMapper.Map(users);

    Console.WriteLine();
    Console.WriteLine(
        $"{identities.Count} identité(s) Active Directory normalisée(s).");

    Console.WriteLine();

    foreach (var identity in identities)
    {
        Console.WriteLine(
            $"- {identity.UserName}");

        Console.WriteLine(
            $"  ExternalId : {identity.ExternalId}");

        Console.WriteLine(
            $"  DisplayName : {identity.DisplayName}");

        Console.WriteLine(
            $"  Source : {identity.Source}");

        Console.WriteLine(
            $"  AccountType : {identity.AccountType}");

        Console.WriteLine(
            $"  IsEnabled : {identity.IsEnabled}");

        Console.WriteLine(
            $"  IsLocked : " +
            $"{identity.IsLocked?.ToString() ?? "indisponible"}");

        Console.WriteLine(
            $"  IsServiceAccount : {identity.IsServiceAccount}");

        Console.WriteLine(
            $"  IsPrivileged : {identity.IsPrivileged}");

        Console.WriteLine(
            $"  LastSignInAt : " +
            $"{identity.LastSignInAt?.ToString("u") ?? "indisponible"}");
    }
    Console.WriteLine();
    Console.WriteLine(
        "Collecte des groupes Active Directory...");

    var groupCollector =
        new ActiveDirectoryGroupCollector(
            options);

    var adGroups =
        groupCollector.Collect();

    Console.WriteLine();
    Console.WriteLine(
        $"{adGroups.Count} groupe(s) Active Directory récupéré(s).");

    var groupMapper =
        new ActiveDirectoryGroupMapper();

    var groups =
        groupMapper.Map(
            adGroups);

    Console.WriteLine(
        $"{groups.Count} groupe(s) normalisé(s) pour l'API.");

    Console.WriteLine();

    foreach (var group in groups)
    {
        Console.WriteLine(
            $"- {group.Name}");

        Console.WriteLine(
            $"  ExternalId : {group.ExternalId}");

        Console.WriteLine(
            $"  GroupType : {group.GroupType}");

        Console.WriteLine(
            $"  IsPrivileged : {group.IsPrivileged}");
    }

    Console.WriteLine();
    Console.WriteLine(
        "Résolution des appartenances aux groupes...");

    var membershipResolver =
        new ActiveDirectoryGroupMembershipResolver();

    var memberships =
        membershipResolver.Resolve(
            users,
            adGroups);

    Console.WriteLine();
    Console.WriteLine(
        $"{memberships.Count} appartenance(s) " +
        "utilisateur-groupe résolue(s).");

    Console.WriteLine();

    var identityNamesByExternalId =
        identities.ToDictionary(
            identity => identity.ExternalId,
            identity => identity.UserName,
            StringComparer.OrdinalIgnoreCase);

    var groupNamesByExternalId =
        groups.ToDictionary(
            group => group.ExternalId,
            group => group.Name,
            StringComparer.OrdinalIgnoreCase);

    foreach (var membership in memberships)
    {
        var identityName =
            identityNamesByExternalId.TryGetValue(
                membership.IdentityExternalId,
                out var resolvedIdentityName)
                ? resolvedIdentityName
                : membership.IdentityExternalId;

        var groupName =
            groupNamesByExternalId.TryGetValue(
                membership.GroupExternalId,
                out var resolvedGroupName)
                ? resolvedGroupName
                : membership.GroupExternalId;

        Console.WriteLine(
            $"- {identityName}");

        Console.WriteLine(
            $"  → {groupName}");

        Console.WriteLine(
            $"  Type : {membership.MembershipType}");
    }
    var privilegedGroupExternalIds =
    groups
        .Where(group =>
            group.IsPrivileged)
        .Select(group =>
            group.ExternalId)
        .ToHashSet(
            StringComparer.OrdinalIgnoreCase);

    var privilegedIdentityExternalIds =
        memberships
            .Where(membership =>
                privilegedGroupExternalIds.Contains(
                    membership.GroupExternalId))
            .Select(membership =>
                membership.IdentityExternalId)
            .ToHashSet(
                StringComparer.OrdinalIgnoreCase);

    Console.WriteLine();
    Console.WriteLine(
        "Identités disposant d'une appartenance " +
        "à un groupe privilégié :");

    Console.WriteLine();

    foreach (var identity in identities
        .Where(identity =>
            privilegedIdentityExternalIds.Contains(
                identity.ExternalId)))
    {
        Console.WriteLine(
            $"- {identity.UserName}");
    }

    return 0;
}
catch (LdapException exception)
{
    Console.Error.WriteLine(
        "Échec LDAP : " +
        exception.Message);

    Console.Error.WriteLine(
        $"Code LDAP : {exception.ErrorCode}");

    return 1;
}
catch (InvalidOperationException exception)
{
    Console.Error.WriteLine(
        "Configuration ou validation incorrecte : " +
        exception.Message);

    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine(
        "Échec inattendu du collecteur AD : " +
        exception.Message);

    return 1;
}
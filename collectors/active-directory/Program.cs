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
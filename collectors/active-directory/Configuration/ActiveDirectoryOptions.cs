namespace IdentityAudit.ActiveDirectoryCollector.Configuration;

public sealed class ActiveDirectoryOptions
{
    public string Host { get; init; }
        = string.Empty;

    public int Port { get; init; }

    public bool UseSsl { get; init; }

    public string BaseDn { get; init; }
        = string.Empty;

    public string BindUsername { get; init; }
        = string.Empty;

    public string BindPassword { get; init; }
        = string.Empty;

    public TimeSpan Timeout { get; init; }
        = TimeSpan.FromSeconds(15);

    public static ActiveDirectoryOptions
        FromEnvironment()
    {
        var useSsl = ReadBoolean(
            "AD_USE_SSL",
            defaultValue: false);

        var defaultPort =
            useSsl ? 636 : 389;

        return new ActiveDirectoryOptions
        {
            Host = ReadRequired(
                "AD_HOST"),

            Port = ReadPort(
                "AD_PORT",
                defaultPort),

            UseSsl = useSsl,

            BaseDn = ReadRequired(
                "AD_BASE_DN"),

            BindUsername = ReadRequired(
                "AD_BIND_USERNAME"),

            BindPassword = ReadRequired(
                "AD_BIND_PASSWORD"),

            Timeout = TimeSpan.FromSeconds(
                ReadPositiveInteger(
                    "AD_TIMEOUT_SECONDS",
                    defaultValue: 15))
        };
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host))
        {
            throw new InvalidOperationException(
                "Le serveur Active Directory est absent.");
        }

        if (Port is < 1 or > 65535)
        {
            throw new InvalidOperationException(
                "Le port LDAP doit être compris entre 1 et 65535.");
        }

        if (string.IsNullOrWhiteSpace(BaseDn))
        {
            throw new InvalidOperationException(
                "Le Base DN Active Directory est absent.");
        }

        if (string.IsNullOrWhiteSpace(BindUsername))
        {
            throw new InvalidOperationException(
                "Le compte de lecture LDAP est absent.");
        }

        if (string.IsNullOrEmpty(BindPassword))
        {
            throw new InvalidOperationException(
                "Le mot de passe LDAP est absent.");
        }
    }

    private static string ReadRequired(
        string variableName)
    {
        var value =
            Environment.GetEnvironmentVariable(
                variableName);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"La variable d'environnement " +
                $"{variableName} est absente.");
        }

        return value.Trim();
    }

    private static bool ReadBoolean(
        string variableName,
        bool defaultValue)
    {
        var value =
            Environment.GetEnvironmentVariable(
                variableName);

        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (!bool.TryParse(value, out var result))
        {
            throw new InvalidOperationException(
                $"La variable {variableName} doit contenir " +
                "'true' ou 'false'.");
        }

        return result;
    }

    private static int ReadPort(
        string variableName,
        int defaultValue)
    {
        var value =
            Environment.GetEnvironmentVariable(
                variableName);

        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (!int.TryParse(value, out var port) ||
            port is < 1 or > 65535)
        {
            throw new InvalidOperationException(
                $"La variable {variableName} contient " +
                "un port invalide.");
        }

        return port;
    }

    private static int ReadPositiveInteger(
        string variableName,
        int defaultValue)
    {
        var value =
            Environment.GetEnvironmentVariable(
                variableName);

        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (!int.TryParse(value, out var result) ||
            result <= 0)
        {
            throw new InvalidOperationException(
                $"La variable {variableName} doit contenir " +
                "un entier strictement positif.");
        }

        return result;
    }
}
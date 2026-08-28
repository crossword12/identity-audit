namespace IdentityAudit.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName =
        "Authentication:Jwt";

    public string Issuer { get; init; }
        = string.Empty;

    public string Audience { get; init; }
        = string.Empty;

    public string SigningKey { get; init; }
        = string.Empty;

    public int AccessTokenMinutes { get; init; }
        = 15;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException(
                "L'émetteur JWT n'est pas configuré.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException(
                "L'audience JWT n'est pas configurée.");
        }

        if (AccessTokenMinutes is < 5 or > 60)
        {
            throw new InvalidOperationException(
                "La durée du jeton JWT doit être comprise entre 5 et 60 minutes.");
        }

        _ = GetSigningKeyBytes();
    }

    public byte[] GetSigningKeyBytes()
    {
        if (string.IsNullOrWhiteSpace(SigningKey))
        {
            throw new InvalidOperationException(
                "La clé de signature JWT n'est pas configurée.");
        }

        byte[] signingKeyBytes;

        try
        {
            signingKeyBytes =
                Convert.FromBase64String(SigningKey);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "La clé de signature JWT doit être encodée en Base64.",
                exception);
        }

        if (signingKeyBytes.Length < 32)
        {
            throw new InvalidOperationException(
                "La clé de signature JWT doit contenir au moins 32 octets.");
        }

        return signingKeyBytes;
    }
}
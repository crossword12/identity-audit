using System.DirectoryServices.Protocols;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace IdentityAudit.ActiveDirectoryCollector.Services;

public sealed class LdapServerCertificateValidator : IDisposable
{
    private const string ServerAuthenticationOid =
        "1.3.6.1.5.5.7.3.1";

    private readonly string _expectedHost;
    private readonly X509Certificate2 _trustedRoot;

    public LdapServerCertificateValidator(
        string expectedHost,
        string trustedCaCertificatePath)
    {
        if (string.IsNullOrWhiteSpace(expectedHost))
        {
            throw new ArgumentException(
                "Le nom du serveur LDAP est obligatoire.",
                nameof(expectedHost));
        }

        if (string.IsNullOrWhiteSpace(trustedCaCertificatePath))
        {
            throw new ArgumentException(
                "Le chemin du certificat CA est obligatoire.",
                nameof(trustedCaCertificatePath));
        }

        if (!File.Exists(trustedCaCertificatePath))
        {
            throw new FileNotFoundException(
                "Le certificat CA de confiance est introuvable.",
                trustedCaCertificatePath);
        }

        _expectedHost = expectedHost;

        _trustedRoot =
            X509CertificateLoader.LoadCertificateFromFile(
                trustedCaCertificatePath);
    }

    public bool Validate(
        LdapConnection connection,
        X509Certificate certificate)
    {
        try
        {
            using var serverCertificate =
                X509CertificateLoader.LoadCertificate(
                    certificate.GetRawCertData());

            // 1. Vérifier que le certificat appartient bien
            //    au serveur LDAP attendu.
            var hostnameIsValid =
                serverCertificate.MatchesHostname(
                    _expectedHost,
                    allowWildcards: false,
                    allowCommonName: false);

            if (!hostnameIsValid)
            {
                Console.Error.WriteLine(
                    $"Certificat LDAPS refusé : " +
                    $"le nom ne correspond pas à {_expectedHost}.");

                return false;
            }

            // 2. Construire une chaîne qui ne fait confiance
            //    qu'à notre CA de laboratoire.
            using var chain = new X509Chain();

            chain.ChainPolicy.TrustMode =
                X509ChainTrustMode.CustomRootTrust;

            chain.ChainPolicy.CustomTrustStore.Add(
                _trustedRoot);

            chain.ChainPolicy.VerificationFlags =
                X509VerificationFlags.NoFlag;

            // Notre CA de laboratoire ne publie pas de CRL.
            // À utiliser uniquement dans le laboratoire.
            chain.ChainPolicy.RevocationMode =
                X509RevocationMode.NoCheck;

            // Le certificat doit être utilisable
            // pour l'authentification d'un serveur TLS.
            chain.ChainPolicy.ApplicationPolicy.Add(
                new Oid(ServerAuthenticationOid));

            var chainIsValid =
                chain.Build(serverCertificate);

            if (!chainIsValid)
            {
                Console.Error.WriteLine(
                    "Certificat LDAPS refusé : " +
                    "la chaîne de confiance est invalide.");

                foreach (var status in chain.ChainStatus)
                {
                    Console.Error.WriteLine(
                        $" - {status.Status}: " +
                        $"{status.StatusInformation.Trim()}");
                }

                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"Erreur pendant la validation " +
                $"du certificat LDAPS : {ex.Message}");

            return false;
        }
    }

    public void Dispose()
    {
        _trustedRoot.Dispose();
    }
}
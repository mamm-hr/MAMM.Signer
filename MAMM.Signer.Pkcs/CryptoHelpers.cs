using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Pkcs;

internal static class CryptoHelpers
{
    /// <summary>
    /// Identificira poznatu vrstu implementacije kriptografskog modula.
    /// </summary>
    ///
    /// <param name="cert">
    ///     Certifikat iz čijeg se javnog ključa utvrduje implementacija kripografskog modula.</param>
    ///
    /// <returns>
    ///     Vrati jednu od vrijednosti iz enumeracije <see cref="CryptoProviderType"/> ili <see
    ///     cref="CryptoProviderType.Unknown"/> ako nije bilo moguće raspoznati implementaciju.</returns>
    ///
    public static CryptoProviderType GetProviderType(
          X509Certificate2 cert
        )
    {
        CryptoProviderType retVal;
        switch(cert.PublicKey.Oid.FriendlyName)
        {
            case "RSA": // Ili cert.PublicKey.Oid.Value == Oids.Rsa ("1.2.840.113549.1.1.1").
                {
                    //using var key = cert.GetRSAPublicKey(); // Does not return RSACng, see https://github.com/dotnet/runtime/pull/76277
                    using var key = cert.GetRSAPrivateKey() ?? throw new PrivateKeyMissingException(cert.Issuer, cert.SerialNumber);
                    if(key is RSACryptoServiceProvider rsa)
                        retVal
                            = StringComparer.InvariantCultureIgnoreCase.Equals( rsa.CspKeyContainerInfo.ProviderName, "AKDSHCard CSP" )
                            ? CryptoProviderType.RsaAkdshCsp
                            : CryptoProviderType.RsaCsp;
                    else if(key is RSACng)
                        retVal = CryptoProviderType.RsaKsp;
                    else
                        retVal = CryptoProviderType.Unknown;
                }
                break;

            case "ECC": // Ili cert.PublicKey.Oid.Value == Oids.EcPublicKey ("1.2.840.10045.2.1")
                {
                    //using var key = cert.GetECDsaPublicKey();
                    using var key = cert.GetECDsaPrivateKey() ?? throw new PrivateKeyMissingException(cert.Issuer, cert.SerialNumber);
                    retVal = key.KeySize switch
                    {
                        384 => CryptoProviderType.Ecdsa384,
                        521 => CryptoProviderType.Ecdsa521,
                        _ => CryptoProviderType.Ecdsa,
                    };
                }
                break;

            default:
                retVal = CryptoProviderType.Unknown;
                break;
        }
        return retVal;
    }
}

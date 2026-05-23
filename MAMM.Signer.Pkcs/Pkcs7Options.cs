using System.Security.Cryptography;

namespace MAMM.Signer.Pkcs;

/// <summary>
/// Opcije za usmjeravanje rada metoda u razredu <see cref="Pkcs7"/> i <see cref="CmsMessage"/>.
/// </summary>
///
/// <example>
/// <code>
/// var options = new Pkcs7Options
/// {
///     DefaultDigestAlgorithms = new()
///     {
///         RsaCsp = Oid.FromFriendlyName("sha1", OidGroup.HashAlgorithm),
///         RsaKsp = Oid.FromFriendlyName("sha256", OidGroup.HashAlgorithm),
///         RsaKsp = Oid.FromFriendlyName("sha384", OidGroup.HashAlgorithm),
///     }
/// };
/// </code>
/// </example>
///
public class Pkcs7Options
{
    /// <summary>
    /// Opcije algoritama digitalnih sažetaka za različite implementacije kriptografskog modula.
    /// </summary>
    public class DigestAlgorithms
    {
        /// <summary>
        /// OID algoritma digitalnog sažetka kad se potpisuje RSA privatnim ključem kroz CSP implementaciju koja NIJE
        /// AKDSHCard CSP. Prešutno koristi algoritam koji je pretpostavljan od .NET-a, a u vrijeme pisanja ovog
        /// komentara to je SHA-256.
        /// </summary>
        public Oid? RsaCsp { get; set; } = null;

        /// <summary>
        /// OID algoritma digitalnog sažetka kad se potpisuje RSA privatnim ključem kroz modernu KSP implementaciju.
        /// Prešutno koristi algoritam koji je pretpostavljan od .NET-a, a u vrijeme pisanja ovog komentara to je
        /// SHA-256.
        /// </summary>
        public Oid? RsaKsp { get; set; } = null;

        /// <summary>
        /// OID algoritma digitalnog sažetka kad se potpisuje ECDSA privatnim ključem na krivulji P-256. Ova
        /// implementacija biblioteke u tom slučaju prešutno koristi SHA-256.
        /// </summary>
        public Oid? Ecdsa256 { get; set; } = null;

        /// <summary>
        /// OID algoritma digitalnog sažetka kad se potpisuje ECDSA privatnim ključem na krivulji P-384. Ova
        /// implementacija biblioteke u tom slučaju prešutno koristi SHA-384.
        /// </summary>
        public Oid? Ecdsa384 { get; set; } = null;

        /// <summary>
        /// OID algoritma digitalnog sažetka kad se potpisuje ECDSA privatnim ključem na krivulji P-521. Ova
        /// implementacija biblioteke u tom slučaju prešutno koristi SHA-512.
        /// </summary>
        public Oid? Ecdsa521 { get; set; } = null;
    }

    /// <summary>
    /// Prešutne opcije algoritama digitalnih sažetaka za različite implementacije kriptografskog modula. Vidi raspravu
    /// uz <see cref="CryptoProviderType"/> za detalje.
    /// </summary>
    public DigestAlgorithms DefaultDigestAlgorithms { get; set; } = new();

    /// <summary>
    /// Ako je ovo istina, neće prikazati korisničko sučelje ako je potrebno unijeti PIN ili potvrditi upotrebu
    /// privatnog ključa.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    ///     Staviti ovo u istinu samo ako se želi da operacija završi neuspjehom u slučaju potrebe interakcije s
    ///     korisnikom.</para>
    /// </remarks>
    public bool SilentUi { get; set; } = false;

    /// <summary>
    /// Ako je ovo laž, prilikom ovjere (verifikacije) potpisa provjerava se i valjanost (validira) potpisnikov
    /// certifikat i lanac povjerenja.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    ///     Normalno ponašanje operacije ovjere potpisa je da uz provjeru autentičnosti potpisa certifikatom provjeri i
    ///     valjanost, odnosno vjerodostojnost potpisnikovog certifikata, kao i je li certifikat namijenjen
    ///     potpisivanju. Ovom se opcijom ta provjera može isključiti i potpisnikovom certifikatu vjerovati i smatrati
    ///     ga valjanim.</para>
    /// </remarks>
    public bool TrustCertificates { get; set; } = false;
}

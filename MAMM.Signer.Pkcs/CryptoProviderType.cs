namespace MAMM.Signer.Pkcs;

/// <summary>
/// Poznate implementacije kriptografskih modula.
/// </summary>
internal enum CryptoProviderType
{
    /// <summary>
    /// Nije poznato.
    /// </summary>
    Unknown,

    /// <summary>
    /// <see cref="System.Security.Cryptography.RSACryptoServiceProvider"/> koji prijavljuje AKDSHCard CSP, pa
    /// pretpostavimo da je u pitanju "plava" kartica. Koristi SHA-1 algoritam digitalnog sažetka.
    /// </summary>
    RsaAkdshCsp,

    /// <summary>
    /// <see cref="System.Security.Cryptography.RSACryptoServiceProvider"/> RSACryptoServiceProvider, dakle RSA kroz
    /// bilo koji drugi CSP (koji nije AKDSHCard CSP). Za ove koristimo od korisnika konfigurirani algoritam digitalnog
    /// sažetka, a inače od .NET-a prešutni, što će u vrijeme ovog pisanja biti SHA-256.
    /// </summary>
    RsaCsp,

    /// <summary>
    /// <see cref="System.Security.Cryptography.RSACng"/>, dakle RSA kroz KSP. U principu, KSP će uvijek biti Microsoft
    /// Smart Card Key Storage Provider jer će izdavači kartica raditi minidrivere za njega umjesto da implementiraju
    /// cijeli vlastiti KSP. I za ove koristimo od korisnika konfigurirani algoritam digitalnog sažetka, a inače od
    /// .NET-a prešutni, što će u vrijeme ovog pisanja biti SHA-256.
    /// </summary>
    RsaKsp,


    /// <summary>
    /// <see cref="System.Security.Cryptography.ECDsa"/>, dakle ECDSA ali bez da je utvrđena krivulja. Ovo nije
    /// podržano, jer birati se mora algoritam digitalnog sažetka koji odgovara duljini ključa, tj. krivulji.
    /// </summary>
    Ecdsa,

    /// <summary>
    /// <see cref="System.Security.Cryptography.ECDsa"/> s utvrđenom krivuljom. Stavimo od korisnika konfigurirani
    /// algoritam digitalnog sažetak, a inake SHA-256, SHA-384, odn. SHA-512.
    /// </summary>
    Ecdsa256,
    Ecdsa384,
    Ecdsa521,
};

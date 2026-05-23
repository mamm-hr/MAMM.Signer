namespace MAMM.Signer.Shared;

/// <summary>
/// Moguće namjene cerifikata.
/// </summary>
public enum CertificatePurpose
{
    /// <summary>
    /// Svrha nije navedena ili nije bitna.
    /// </summary>
    Unspecified,

    /// <summary>
    /// Certifikat je namijenjen identifkaciji, enkripciji i slično.
    /// </summary>
    Identification,

    /// <summary>
    /// Certifikat je potpisni.
    /// </summary>
    Signature,
}

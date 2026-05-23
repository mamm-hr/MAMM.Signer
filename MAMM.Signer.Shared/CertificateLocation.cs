namespace MAMM.Signer.Shared;

/// <summary>
/// Lokacije u kojima se mogu tražiti certifikati.
/// </summary>
public enum CertificateLocation
{
    /// <summary>
    /// U spremištu prijavljenog korisnika.
    /// </summary>
    CurrentUser,

    /// <summary>
    /// U spremištu lokalnog računala.
    /// </summary>
    LocalMachine,

    /// <summary>
    /// U spojenim kriptografskim uređajima.
    /// </summary>
    SmartCardReaders,
}

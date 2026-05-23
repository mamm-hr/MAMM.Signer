using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Shared;

/// <summary>
/// Implementacija potrebnih operacija s certifikatima.
/// </summary>
///
internal class CertificateManager
    : ICertificateManager
{
    /// <summary>
    /// Učita certifikate iz specificirane lokacije u pripremi skupa certifikata nad kojim rade ostale operacije.
    /// </summary>
    ///
    /// <param name="location">
    ///     Spremište iz kojeg se bira certifikat.</param>
    ///
    /// <param name="includeCsp">
    ///     Ako je ovo istina, a <paramref name="location"/> je <see cref="CertificateLocation.SmartCardReaders"/>, onda
    ///     se certifikate na karticama traži i kroz staro CSP sučelje.</param>
    ///
    public void LoadCertificates(
          CertificateLocation location
        , bool includeCsp
        )
        => m_certificates = CertHelpers.GetUserCertificates(location, includeCsp);

    /// <summary>
    /// Nađe certifikat po digitalnom otisku.
    /// </summary>
    ///
    public X509Certificate2? FindCertificate(
          string thumbprint
        , bool validOnly = false
        )
        => !string.IsNullOrEmpty( thumbprint )
        && CertHelpers.FindCertificateByThumbprint( m_certificates, thumbprint, validOnly, out var cert )
        ? cert : null;

    /// <summary>
    /// Izabere certifikat za kriptografsku operaciju. V. <see cref="ICertificateManager"/> za detalje.
    /// </summary>
    public X509Certificate2? SelectCertificate(
          CertificatePurpose purpose
        , bool validOnly
        , string? title
        , string? message
        )
        => CertHelpers.SelectCertificate(m_certificates, purpose, validOnly, title, message );


    /// <summary>
    /// Vraća neslužbeni naziv certifikata ako je upisan, inače predmet certifikata.
    /// </summary>
    public string GetFriendlyOrSubjectName(
          X509Certificate2 cert
        )
        => CertHelpers.GetFriendlyOrSubjectName( cert );

    private X509Certificate2Collection m_certificates = [];
}

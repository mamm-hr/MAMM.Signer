using MAMM.Signer.Shared;

namespace MAMM.Signer.Interop;

public class CoCertificates
    : ICertificates
{
    /// <summary>
    /// Učita certifikate iz specificirane lokacije u pripremi skupa certifikata nad kojim rade ostale operacije.
    /// </summary>
    ///
    public void LoadCertificates(
          CoCertificateLocation location
        , bool includeCsp
        )
    {
        m_certManager.LoadCertificates(
              location switch
              {
                  CoCertificateLocation.CurrentUser => CertificateLocation.CurrentUser,
                  CoCertificateLocation.LocalMachine => CertificateLocation.LocalMachine,
                  CoCertificateLocation.SmartCardReaders => CertificateLocation.SmartCardReaders,
                  _ => throw new ArgumentException( nameof( location ) )
              }
            , includeCsp
            );
    }

    /// <summary>
    /// Nađe certifikat po digitalnom otisku u skupu certifikata pripremljenom metodom <see
    /// cref="LoadCertificates(CoCertificateLocation, bool)"/> .
    /// </summary>
    ///
    public ICertificate? FindCertificate(
          string thumbprint
        , bool validOnly
        )
    {
        var cert = m_certManager.FindCertificate(thumbprint, validOnly);
        return cert is null ? null : new CoCertificate( cert );
    }

    /// <summary>
    /// Izabere certifikat za kriptografsku operaciju.
    /// </summary>
    ///
    public ICertificate? SelectCertificate(
          CoCertificatePurpose purpose
        , bool validOnly
        , string title
        , string message
        )
    {
        var cert = m_certManager.SelectCertificate(
              purpose switch
              {
                  CoCertificatePurpose.Unspecified => CertificatePurpose.Unspecified,
                  CoCertificatePurpose.Identification => CertificatePurpose.Identification,
                  CoCertificatePurpose.Signature => CertificatePurpose.Signature,
                  _ => throw new ArgumentException(nameof(purpose))
              }
            , validOnly
            , title
            , message
            );
        return cert is null ? null : new CoCertificate( cert );
    }

    private readonly CertificateManager m_certManager = new();
}

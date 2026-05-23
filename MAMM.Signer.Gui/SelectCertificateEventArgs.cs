using MAMM.Signer.Shared;
using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Gui;

internal class SelectCertificateEventArgs(
      CertificateLocation certificateLocation
    , X509Certificate2? certificate
    )
{
    public CertificateLocation CertificateLocation { get; } = certificateLocation;

    public bool Canceled { get; set; } = false;

    public X509Certificate2? Certificate { get; set; } = certificate;
}

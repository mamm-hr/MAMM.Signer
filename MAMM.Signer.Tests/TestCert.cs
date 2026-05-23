using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Tests;

internal class TestCert(
      TestCert.Properties props
    , X509Certificate2 cert
    )
    : IDisposable
{
    public class Properties(
          string thumbprint
        , bool showsUI
        , bool cezih
        , string? fileName
        , StoreName storeName
        )
    {
        public readonly string Thumbprint = thumbprint;
        public readonly bool ShowsUI = showsUI;
        public readonly bool Cezih = cezih;
        public readonly string? FileName = fileName;
        public readonly StoreName StoreName = storeName;
    }

    public readonly Properties Props = props;
    public readonly X509Certificate2 Cert = cert;


    public void Dispose()
    {
        if(!this.Props.Cezih)
            Helpers.RemoveCertificateFromStore( this.Props.StoreName, this.Props.Thumbprint );
    }
}

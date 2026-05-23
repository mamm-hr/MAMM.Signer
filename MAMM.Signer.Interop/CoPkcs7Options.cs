using MAMM.Signer.Pkcs;

namespace MAMM.Signer.Interop;

public sealed class CoPkcs7Options(
      Pkcs7Options innerObject
    )
    : IPkcs7Options
{
    public Pkcs7Options InnerObject => innerObject;

    public IDigestAlgorithms DefaultDigestAlgorithms => m_defaultDigestAlgorithms;

    public bool SilentUi { get => innerObject.SilentUi; set => innerObject.SilentUi = value; }

    public bool TrustCertificates { get => innerObject.TrustCertificates; set => innerObject.TrustCertificates = value; }

    private readonly CoDigestAlgorithms m_defaultDigestAlgorithms = new(innerObject.DefaultDigestAlgorithms);
}

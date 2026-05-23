using System.Security.Cryptography.Xml;

namespace MAMM.Signer.Interop;

public sealed class CoIssuerSerial(
      X509IssuerSerial issuerSerial
    )
    : IIssuerSerial
{
    public string IssuerName => issuerSerial.IssuerName ?? "";

    public string SerialNumber => issuerSerial.SerialNumber ?? "";
}

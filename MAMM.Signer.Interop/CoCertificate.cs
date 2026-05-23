using MAMM.Signer.Shared;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace MAMM.Signer.Interop;

public sealed class CoCertificate
    : ICertificate
    , IHandle
{
    public CoCertificate(
          X509Certificate2 certificate
        )
    {
        m_certificate = certificate;
        lock(m_map)
        {
            m_handle = m_nextHandle++;
            m_map.Add( m_handle, m_certificate );
        }
        m_issuerSerial = new(
               new X509IssuerSerial() { IssuerName = m_certificate.Issuer, SerialNumber = m_certificate.SerialNumber }
            );
    }

    ~CoCertificate()
    {
        lock(m_map)
            m_map.Remove( m_handle );
    }

    public string FriendlyName => m_certificate.FriendlyName ?? "";

    public int Handle => m_handle;

    public IIssuerSerial IssuerSerial => m_issuerSerial;

    public string Subject => m_certificate.Subject;

    public string Thumbprint => m_certificate.Thumbprint ?? "";

    public bool Valid => m_certificate.Verify();

    public string FriendlyOrSubjectName => m_certificate.GetFriendlyOrSubjectName();

    public static X509Certificate2? GetCertificate(
          int handle
        )
        => m_map.TryGetValue( handle, out var cert ) ? cert : null;

    private readonly X509Certificate2 m_certificate;

    private readonly int m_handle;

    private readonly CoIssuerSerial m_issuerSerial;

    private static readonly Dictionary<int, X509Certificate2> m_map = [];

    private static int m_nextHandle = 1;

}

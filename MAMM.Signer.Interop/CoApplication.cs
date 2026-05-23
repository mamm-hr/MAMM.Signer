using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string CLSID_Application = "1EFDA486-414C-4273-B81E-A9FC8C715432"; // Usklađivati s IDL datotekom
                                                                                    // i datotekom manifesta za net48.
}

[ComVisible( true )]
[Guid( InteropGuids.CLSID_Application )]
[ClassInterface( ClassInterfaceType.None )]
[ComDefaultInterface( typeof( IApplication ) )]
[ProgId( "MAMM.Signer.Application" )]
public sealed class CoApplication
    : IApplication
{
    public ICertificates Certificates => m_certificates;

    public IPkcs7Oids Oids => m_pkcs7Oids;

    public IPkcs7 CreatePkcs7()
        => new CoPkcs7();

    private readonly CoCertificates m_certificates = new();

    private readonly CoPkcs7Oids m_pkcs7Oids = new();
}

using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_IPkcs7Options = "1C43B9EE-FA31-449F-BE42-933D2C4C5F55"; // Usklađivati s IDL datotekom.
}

[ComVisible(true)]
[Guid(InteropGuids.IID_IPkcs7Options)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IPkcs7Options
{
    [DispId( 1 )] IDigestAlgorithms DefaultDigestAlgorithms { get; }

    [DispId( 2 )] bool SilentUi { get; set; }

    [DispId( 3 )] bool TrustCertificates { get; set; }
}

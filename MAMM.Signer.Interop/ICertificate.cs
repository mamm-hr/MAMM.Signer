using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_ICertificate = "EFE29223-A05F-422C-92CA-1285D9E2D16D"; // Usklađivati s IDL datotekom.
}

[ComVisible(true)]
[Guid(InteropGuids.IID_ICertificate)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface ICertificate
{
    [DispId( 1 )] string FriendlyName { get; }

    [DispId( 2 )] IIssuerSerial IssuerSerial { get; }

    [DispId( 3 )] string Subject { get; }

    [DispId( 4 )] string Thumbprint { get; }

    [DispId( 5 )] bool Valid { get; }

    [DispId( 6 )] string FriendlyOrSubjectName { get; }
}

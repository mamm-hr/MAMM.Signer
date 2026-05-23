using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_ICmsMessage = "C090C1A3-A5F5-4BDB-A0EF-B5F2AE21ED0E"; // Usklađivati s IDL datotekom.
}

[ComVisible(true)]
[Guid(InteropGuids.IID_ICmsMessage)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface ICmsMessage
{
    [DispId( 1 )]
    [return: MarshalAs( UnmanagedType.Struct )]
    object Encode();

    [DispId( 2 )]
    void Envelope(
          ICertificate certificate
        , string algorithmOid
        );

    [DispId( 3 )]
    void OpenEnvelope(
          ICertificate recipient
        );

    [DispId( 4 )]
    [return: MarshalAs( UnmanagedType.Struct )]
    object Read();

    [DispId( 5 )]
    void SignAt(
          ICertificate certificate
        , DateTime signingTime
        );

    [DispId( 6 )]
    void SignNow(
          ICertificate certificate
        );

    [DispId( 7 )]
    void Verify(
        );
}

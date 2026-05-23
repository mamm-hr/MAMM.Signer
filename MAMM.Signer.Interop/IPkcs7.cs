using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_IPkcs7 = "74C1E2AA-AA0F-40AF-865D-A31879B234FA"; // Usklađivati s IDL datotekom.
}

[ComVisible(true)]
[Guid(InteropGuids.IID_IPkcs7)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IPkcs7
{
    [DispId( 1 )]
    IPkcs7Options Options { get; }

    [DispId( 2 )]
    [return: MarshalAs( UnmanagedType.Struct )]
    object EnvelopeData(
          [MarshalAs( UnmanagedType.Struct )] object data
        , ICertificate certificate
        , string algorithmOid
        );

    [DispId( 3 )]
    string GetContentTypeOid(
          [MarshalAs( UnmanagedType.Struct )] object data
        );

    [DispId( 4 )]
    [return: MarshalAs( UnmanagedType.Struct )]
    object OpenEnvelopedData(
          [MarshalAs( UnmanagedType.Struct )] object data
        , ICertificate certificate
        );

    [DispId( 5 )]
    [return: MarshalAs( UnmanagedType.Struct )]
    object SignData(
          [MarshalAs( UnmanagedType.Struct )] object data
        , ICertificate certificate
        , DateTime signingTime
        );

    [DispId( 6 )]
    [return: MarshalAs( UnmanagedType.Struct )]
    object VerifySignedData(
          [MarshalAs( UnmanagedType.Struct )] object data
        );
}

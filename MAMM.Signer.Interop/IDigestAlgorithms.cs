using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_IDigestAlgorithms = "AE56ABDB-8A0B-4047-9B75-A1B0484B0B47"; // Usklađivati s IDL datotekom.
}

[ComVisible(true)]
[Guid(InteropGuids.IID_IDigestAlgorithms)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IDigestAlgorithms
{
    [DispId( 1 )] string RsaCspOid { get; set; }

    [DispId( 2 )] string RsaKspOid { get; set; }

    [DispId( 3 )] string Ecdsa256Oid { get; set; }

    [DispId( 4 )] string Ecdsa384Oid { get; set; }

    [DispId( 5 )] string Ecdsa521Oid { get; set; }
}

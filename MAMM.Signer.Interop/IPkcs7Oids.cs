using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_IPkcs7Oids = "9B446DFA-017B-4799-9BE0-40F0D785C765"; // Usklađivati s IDL datotekom.
}

[ComVisible(true)]
[Guid(InteropGuids.IID_IPkcs7Oids)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IPkcs7Oids
{
    [DispId( 1 )] string DataOid { get; }

    [DispId( 2 )] string SignedDataOid { get; }

    [DispId( 3 )] string EnvelopedDataOid { get; }

    [DispId( 4 )] string SignedAndEnvelopedDataOid { get; }

    [DispId( 5 )] string DigestedDataOid { get; }

    [DispId( 6 )] string EncryptedDataOid { get; }

    [DispId( 7 )]
    string GetOid(
          string name
        );

    [DispId( 8 )]
    string GetOidName(
          string oid
        );

    [DispId( 9 )]
    bool IsHashAlgorithm(
          string oid
        );

    [DispId( 10 )]
    bool IsEncryptionAlgorithm(
          string oid
        );
}

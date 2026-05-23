using MAMM.Signer.Pkcs;
using System.Security.Cryptography;

namespace MAMM.Signer.Interop;

public sealed class CoPkcs7Oids
    : IPkcs7Oids
{
    public string DataOid => Pkcs7.Oids.Data.Value!;

    public string SignedDataOid => Pkcs7.Oids.SignedData.Value!;

    public string EnvelopedDataOid => Pkcs7.Oids.EnvelopedData.Value!;

    public string SignedAndEnvelopedDataOid => Pkcs7.Oids.SignedAndEnvelopedData.Value!;

    public string DigestedDataOid => Pkcs7.Oids.DigestedData.Value!;

    public string EncryptedDataOid => Pkcs7.Oids.EncryptedData.Value!;

    public string GetOid( string name )
        => Oid.FromFriendlyName( name, OidGroup.All ).Value ?? "";

    public string GetOidName( string oid )
        => Oid.FromOidValue( oid, OidGroup.All ).FriendlyName ?? "";

    public bool IsHashAlgorithm( string oid )
    {
        try { Oid.FromOidValue( oid, OidGroup.HashAlgorithm ); } catch { return false; }
        return true;
    }

    public bool IsEncryptionAlgorithm( string oid )
    {
        try { Oid.FromOidValue( oid, OidGroup.EncryptionAlgorithm ); } catch { return false; }
        return true;
    }
}

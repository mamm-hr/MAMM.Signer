using System.Security.Cryptography;
using MAMM.Signer.Pkcs;

namespace MAMM.Signer.Interop;

public sealed class CoDigestAlgorithms(
      Pkcs7Options.DigestAlgorithms innerObject
    )
    : IDigestAlgorithms
{
    public string RsaCspOid
    {
        get => innerObject.RsaCsp?.Value ?? "";
        set => innerObject.RsaCsp = string.IsNullOrEmpty( value ) ? null : Oid.FromOidValue( value, OidGroup.HashAlgorithm );
    }

    public string RsaKspOid
    {
        get => innerObject.RsaKsp?.Value ?? "";
        set => innerObject.RsaKsp = string.IsNullOrEmpty( value ) ? null : Oid.FromOidValue( value, OidGroup.HashAlgorithm );
    }

    public string Ecdsa256Oid
    {
        get => innerObject.Ecdsa256?.Value ?? "";
        set => innerObject.Ecdsa256 = string.IsNullOrEmpty( value ) ? null : Oid.FromOidValue( value, OidGroup.HashAlgorithm );
    }

    public string Ecdsa384Oid
    {
        get => innerObject.Ecdsa384?.Value ?? "";
        set => innerObject.Ecdsa384 = string.IsNullOrEmpty( value ) ? null : Oid.FromOidValue( value, OidGroup.HashAlgorithm );
    }

    public string Ecdsa521Oid
    {
        get => innerObject.Ecdsa521?.Value ?? "";
        set => innerObject.Ecdsa521 = string.IsNullOrEmpty( value ) ? null : Oid.FromOidValue( value, OidGroup.HashAlgorithm );
    }
}

using MAMM.Signer.Pkcs;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Interop;

public sealed class CoPkcs7
    : IPkcs7
{
    public IPkcs7Options Options => m_options;

    [return: MarshalAs( UnmanagedType.Struct )]
    public object EnvelopeData(
          [MarshalAs( UnmanagedType.Struct )] object data
        , ICertificate certificate
        , string algorithmOidValue
        )
    {
        if(!ComHelpers.TryConvert( data, out var vartype, out var bytes ))
            throw new ArgumentException( Resources.CoPkcs7_InvalidDataType );
        if(certificate is null)
            throw new InvalidOperationException( Resources.CoPkcs7_RecipientNotSpecified );
        if(certificate is not IHandle certHandle)
            throw new ArgumentException( Resources.CoPkcs7_InvalidCertificateObject );
        var cert = CoCertificate.GetCertificate(certHandle.Handle)
            ?? throw new InvalidOperationException( Resources.CoPkcs7_MissingCertificate );
        Oid? algorithmOid = string.IsNullOrEmpty(algorithmOidValue) ? null : Oid.FromOidValue(algorithmOidValue, OidGroup.EncryptionAlgorithm);
        return ComHelpers.Convert( Pkcs7.EnvelopeData( bytes!, cert, algorithmOid, m_options.InnerObject ), vartype );
    }

    public string GetContentTypeOid(
          [MarshalAs( UnmanagedType.Struct )] object data
        )
    {
        if(!ComHelpers.TryConvert( data, out _, out var bytes ))
            throw new ArgumentException( Resources.CoPkcs7_InvalidDataType );
        return Pkcs7.GetContentTypeOid( bytes! ).Value ?? "";
    }

    [return: MarshalAs( UnmanagedType.Struct )]
    public object OpenEnvelopedData(
          [MarshalAs( UnmanagedType.Struct )] object data
        , ICertificate certificate
        )
    {
        if(!ComHelpers.TryConvert( data, out var vartype, out var bytes ))
            throw new ArgumentException( Resources.CoPkcs7_InvalidDataType );
        X509Certificate2? cert = null;
        if(certificate is not null)
        {
            if(certificate is not IHandle certHandle)
                throw new ArgumentException( Resources.CoPkcs7_InvalidCertificateObject );
            cert = CoCertificate.GetCertificate( certHandle.Handle )
                ?? throw new InvalidOperationException( Resources.CoPkcs7_MissingCertificate );
        }
        return ComHelpers.Convert( Pkcs7.OpenEnvelopedData( bytes!, cert, m_options.InnerObject ), vartype );
}

    [return: MarshalAs( UnmanagedType.Struct )]
    public object SignData(
          [MarshalAs( UnmanagedType.Struct )] object data
        , ICertificate certificate
        , DateTime signingTime
        )
    {
        if(!ComHelpers.TryConvert( data, out var vartype, out var bytes ))
            throw new ArgumentException( Resources.CoPkcs7_InvalidDataType );
        if(certificate is null)
            throw new InvalidOperationException( Resources.CoPkcs7_SignerNotSpecified );
        if(certificate is not IHandle certHandle)
            throw new ArgumentException( Resources.CoPkcs7_InvalidCertificateObject );
        var cert = CoCertificate.GetCertificate(certHandle.Handle)
            ?? throw new InvalidOperationException( Resources.CoPkcs7_MissingCertificate );
        return ComHelpers.Convert( Pkcs7.SignData( bytes!, cert, signingTime, m_options.InnerObject ), vartype );
    }

    [return: MarshalAs( UnmanagedType.Struct )]
    public object VerifySignedData(
          [MarshalAs( UnmanagedType.Struct )] object data
        )
    {
        if(!ComHelpers.TryConvert( data, out var vartype, out var bytes ))
            throw new ArgumentException( Resources.CoPkcs7_InvalidDataType );
        return ComHelpers.Convert( Pkcs7.VerifySignedData( bytes!, m_options.InnerObject ), vartype );
    }

    private readonly CoPkcs7Options m_options = new(new());
}

using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Pkcs;

/// <summary>
/// Metode za izradu i čitanje potpisivanih i šifriranih podataka u PKCS #7, odn. CMS sintaksi (RFC 2315, odn. RFC
/// 5652).
/// </summary>
///
public static class Pkcs7
{
    /// <summary>
    /// OID-ovi vezani uz PKCS #7. Za više detetalja <see href="https://www.rfc-editor.org/rfc/rfc2315">RFC 2315</see>.
    /// </summary>
    public static class Oids
    {
        /// <summary>
        /// PKCS #7 podatak nakon dekodiranja.
        /// </summary>
        public static readonly Oid Data                   = Oid.FromOidValue( "1.2.840.113549.1.7.1", OidGroup.All );

        /// <summary>
        /// PKCS #7 potpisani podatak, "signed-data content type", odnosno SignedData tip.
        /// </summary>
        public static readonly Oid SignedData             = Oid.FromOidValue( "1.2.840.113549.1.7.2", OidGroup.All );

        /// <summary>
        /// PKCS #7 kuvertirani /enveloped/ podatak, "enveloped-data content type", odnosno EnvelopedData tip.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///     Omatanje podatka se vrši šifriranjem podataka simetričnim ključem i šifriranjem tog simetričnog ključa
        ///     javnim ključem primatelja. Ovo je podatak koji nakon potpisivanja kreira AKDSH Signer kad se izabere
        ///     opcija šifriranja čime producira strukturu EnvelopedData(SignedData(Data)).</para>
        /// </remarks>
        ///
        public static readonly Oid EnvelopedData          = Oid.FromOidValue( "1.2.840.113549.1.7.3", OidGroup.All );

        /// <summary>
        /// PKCS #7 potpisani i kuvertirani /enveloped/ podatak, "signed-and-enveloped-data content type", odnosno
        /// SignedAndEnvelopedData tip.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Ovo *nije* potpisan pa omotan podataka, dakle ovo nije EnvelopedData(SignedData(Data)). Ova sintaksa
        ///     nije podržana u .NET-u.</para>
        /// </remarks>
        public static readonly Oid SignedAndEnvelopedData = Oid.FromOidValue( "1.2.840.113549.1.7.4", OidGroup.All );

        /// <summary>
        /// PKCS #7 sažeti podatak, "digested-data content type", odnosno DigestedData.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Ova sintaksa nije podržana u .NET-u.</para>
        /// </remarks>
        public static readonly Oid DigestedData           = Oid.FromOidValue( "1.2.840.113549.1.7.5", OidGroup.All );

        /// <summary>
        /// PKCS #7 šifrirani podatak, "encrypted-data content type", odnosno EncryptedData.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     Ovo *nije* struktura koju generira AKDSH Signer u operaciji šifriranja. Ova sintaksa nije podržana u
        ///     .NET-u.</para>
        /// </remarks>
        public static readonly Oid EncryptedData          = Oid.FromOidValue( "1.2.840.113549.1.7.6", OidGroup.All );
    }

    /// <summary>
    /// Izradi <see cref="CmsMessage"/> objekt inicijaliziran sadržajem koji se šalje, tj. sadržajem koji treba
    /// potpisati ili šifrirati.
    /// </summary>
    ///
    /// <param name="data">
    ///     Sadržaj poruke.
    ///     </param>
    ///
    /// <param name="options">
    ///     Dodatne opcije za usmjeravanje rads metoda objekta, vidi <see cref="Pkcs7Options"/>.</param>
    ///
    /// <returns>
    ///     Vrati <see cref="CmsMessage"/> objekt čijim se metodama <see cref="CmsMessage.Sign(X509Certificate2,
    ///     DateTimeOffset)"/> i <see cref="CmsMessage.Envelope(X509Certificate2, Oid?)"/> može pripremiti potpisana
    ///     i/ili šifrirana poruka i njen tekst onda očitati funkcijom <see cref="Encode"/>.</returns>
    ///
    public static CmsMessage CreateForSending(
          byte[] data
        , Pkcs7Options? options = null
        )
        => new( data, isReceived: false, options );

    /// <summary>
    /// Izradi <see cref="CmsMessage"/> objekt inicijaliziran tekstom poruke koja je primljena, tj. tekstom koji treba
    /// dešifrirati i/ili potpis verificirati.
    /// </summary>
    ///
    /// <param name="data">
    ///     Tekst poruke tipa PKCS #7/CMS "enveloped-data content type" (EnvelopedData) ili "signed-data content type"
    ///     (SignedData).
    ///     </param>
    ///
    /// <param name="options">
    ///     Dodatne opcije za usmjeravanje rads metoda objekta, vidi <see cref="Pkcs7Options"/>.</param>
    ///
    /// <returns>
    ///     Vrati <see cref="CmsMessage"/> objekt čijim se metodama <see cref="CmsMessage.Sign(X509Certificate2,
    ///     DateTimeOffset)"/> i <see cref="CmsMessage.Envelope(X509Certificate2, Oid?)"/> može pripremiti potpisana
    ///     i/ili šifrirana poruka i njen tekst onda očitati funkcijom <see cref="Encode"/>.</returns>
    ///
    public static CmsMessage CreateFromReceived(
          byte[] data
        , Pkcs7Options? options = null
        )
        => new( data, isReceived: true, options );

    /// <summary>
    /// Kuvertira sadržaj poruke kao CMS/PKCS 7# "enveloped-data content type" s jednim primateljem.
    /// </summary>
    ///
    /// <param name="data">
    ///     Sadržaj koji kuvertira.</param>
    ///
    /// <param name="certificate">
    ///     Primateljev certifikat koji ne treba sadržavati privatni ključ, već samo javni.</param>
    ///
    /// <param name="algorithm">
    ///     Opcionalnni OID algoritma za enkripciju sadržaja. Ako se ne navede, .NET Framework od v. 4.8 i .NET od v.
    ///     4.6.0 NuGet paketa koriste AES-256, a ranije DES3-EDE.</param>
    ///
    /// <param name="options">
    ///     Dodatne opcije za usmjeravanje rada metode, vidi <see cref="Pkcs7Options"/>.</param>
    ///
    /// <returns>
    ///     Vrati sadržaj poruke šifriran i kodiran kao CMS/PKCS #7 "enveloped-data content type"
    ///     (EnvelopedData).</returns>
    ///
    /// <remarks>
    ///     <para>
    ///         V. <see cref="CmsMessage.Envelope(X509Certificate2, Oid?)"/> za više detalja.</para>
    /// </remarks>
    ///
    public static byte[] EnvelopeData(
          byte[] data
        , X509Certificate2 certificate
        , Oid? algorithm = null
        , Pkcs7Options? options = null
        )
    {
        if(data is null) throw new ArgumentNullException( nameof( data ) );
        if(certificate is null) throw new ArgumentNullException( nameof( certificate ) );

        var message = new CmsMessage(data, isReceived: false, options);
        message.Envelope( certificate, algorithm );

        return message.Encode();
    }

    /// <summary>
    /// Vrati OID koji identificira vrstu podatka u tekstu poruke.
    /// </summary>
    ///
    /// <param name="data">
    ///     Tekst valjano kodirane PKCS #7/CMS poruke.</param>
    ///
    /// <returns>
    ///     Jedan od OID-ova iz razreda <see cref="Oids"/>, a koji se odnosi na vrstu sadržaja.</returns>
    ///
    /// <remarks>
    /// <para>
    ///     Nakon što se PKCS #7/CMS tekst poruke dekodira, vrsta podatka je uvijek <see cref="Oids.Data"/>. Funkciju se
    ///     poziva prije dekodiranja kako bi se odredio format u kojem je podatak kodiran, odnosno algoritam za
    ///     dekodiranje.</para>
    /// </remarks>
    ///
    public static Oid GetContentTypeOid( byte[] data )
    {
        try
        {
            return ContentInfo.GetContentType( data );
        }
        catch(CryptographicException ex) when( 0x8009310B == (uint)ex.HResult) // ASN1 bad tag value met.
        {
            // Za sintaksno neispravan podatak pretpostavi da je u pitanju sadržaj poruke.
            return Pkcs7.Oids.Data;
        }
    }

    /// <summary>
    /// Otvori sadržaj PKCS #7/CMS "enveloped-data content type" (EnvelopedData) poruke.
    /// </summary>
    ///
    /// <param name="data">
    ///     Podatak tipa EnvelopedData čiji sadržaj otvori (dešifrira).</param>
    ///
    /// <param name="certificate">
    ///     Certifikat koji mora odgovarati identitetu jednog od primatelja ili <see langword="null"/> da se poruka
    ///     dešifrira raspoloživim certifikatom bilo kojeg od primatelja.</param>
    ///
    /// <param name="options">
    ///     Dodatne opcije za usmjeravanje rada metode, vidi <see cref="Pkcs7Options"/>.</param>
    ///
    /// <returns>
    ///     Vrati sadržaj izvorno stavljen u kuvertu.</returns>
    ///
    /// <remarks>
    ///     <para>
    ///         V. <see cref="CmsMessage.OpenEnvelope(X509Certificate2?)"/> za više detalja.</para>
    /// </remarks>
    ///
    public static byte[] OpenEnvelopedData(
          byte[] data
        , X509Certificate2? certificate = null
        , Pkcs7Options? options = null
        )
    {
        if(data is null) throw new ArgumentNullException( nameof( data ) );

        var message = new CmsMessage(data, isReceived: true, options);
        message.OpenEnvelope(certificate);

        return message.Read();
    }

    /// <summary>
    /// Potpiše sadržaj poruke kao PKCS #7/CMS "signed-data content type" (SignedData) s jednim potpisnikom.
    /// </summary>
    ///
    /// <param name="data">
    ///     Sadržaj koji potpiše.</param>
    ///
    /// <param name="certificate">
    ///     Potpisni certifikat.</param>
    ///
    /// <param name="signingTime">
    ///     Vrijeme potpisa koje se dodaje kao potpisani atribut.</param>
    ///
    /// <param name="options">
    ///     Dodatne opcije za usmjeravanje rada metode, vidi <see cref="Pkcs7Options"/>.</param>
    ///
    /// <returns>
    ///     Vrati sadržaj poruke šifriran i kodiran kao CMS/PKCS #7 "signed-data content type" (SignedData).</returns>
    ///
    /// <remarks>
    /// <para>
    ///     V. <see cref="CmsMessage.Sign(X509Certificate2, DateTimeOffset)"/> za više detalja.</para>
    /// </remarks>
    ///
    public static byte[] SignData(
          byte[] data
        , X509Certificate2 certificate
        , DateTimeOffset signingTime
        , Pkcs7Options? options = null
        )
    {
        if(data is null) throw new ArgumentNullException( nameof( data ) );
        if(certificate is null) throw new ArgumentNullException( nameof( certificate ) );

        var message = new CmsMessage(data, isReceived: false, options);
        message.Sign( certificate, signingTime );

        return message.Encode();
    }

    /// <summary>
    /// Ovjeri potpise PKCS #7/CMS "signed-data content type" (SignedData) poruke i opcionalno provjeri valjanost
    /// certifikata u lancu povjerenja potpisnih certifikata.
    /// </summary>
    ///
    /// <param name="data">
    ///     Poruka tipa (SignedData kojoj verificira potpise.</param>
    ///
    /// <param name="options">
    ///     Dodatne opcije za usmjeravanje rada metode, vidi <see cref="Pkcs7Options"/>.</param>
    ///
    /// <returns>
    ///     Vrati sadržaj potpisane poruke.</returns>
    ///
    /// <remarks>
    /// <para>
    ///     V. <see cref="CmsMessage.Verify"/> za više detalja.</para>
    /// </remarks>
    ///
    public static byte[] VerifySignedData(
          byte[] data
        , Pkcs7Options? options = null
        )
    {
        if(data is null) throw new ArgumentNullException( nameof( data ) );

        var message = new CmsMessage(data, isReceived: true, options);
        message.Verify();

        return message.Read();
    }
}

using MAMM.Signer.Pkcs;
using System.Formats.Asn1;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace MAMM.Signer.Tests;

/// <summary>
/// Istraživanje .NET biblioteke za rad s PKCS #7 sintaksom. Isključiti ove testove kroz SuppressExploratoryTests u
/// .runsettings datoteci.
/// </summary>
///
/// <remarks>
/// <para>
///     Neke od ovih Ove metode (između ostalih) generiraju iste datoteke koje su pripremeljene AKDSH Signerom i opisane u <see
///     href="TestCase.md">TestCase.md</see> i ispišu njihov sadržaj. Testovi u <see cref="Exploratory_AkdshSigner"/>
///     ispišu verzije tih datoteka izrađene kroz AKDSH Signer, pa se sadržaji mogu pregledom usporediti.
///     </para>
/// </remarks>
///
[TestClass]
public class Exploratory_Pkcs7 : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Istraži ispravan način korištenja <see cref="EnvelopedCms"/> objekta.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, null )]
    public void ExploreEnveloping( int certNo, string? oid )
    {
        using var testCert = m_runSettings.GetTestCert(certNo, imported: true);

        // Omatanje.

        // Sadržaj.
        byte[] plainData = CreateMessage();

        // PKCS #7 ContentInfo tip sa sirovim podatkom.
        var rawContentInfo = new ContentInfo(plainData); // alt. new ContentInfo(Pkcs7.Oids.Data, rawData)
        Assert.AreEqual( Pkcs7.Oids.Data.Value, rawContentInfo.ContentType.Value );

        // Objekt za omatanje. Kroz konstruktor se inicijalizira samo sa sirovim podatkom.
        var rawCms
            = oid is null
            ? new EnvelopedCms(rawContentInfo) // prešutno odabere simetrični algoritam.
            : new EnvelopedCms(rawContentInfo, new AlgorithmIdentifier(new Oid(oid)));
        Print( 0, "Content encryption algorithm", null, rawCms.ContentEncryptionAlgorithm );

        // Primatelj, identificiran izdavačem i serijskim brojem certifikata (taj način identifikacije se i prešutno
        // bira).
        Print( 0, "Certificate Key Algorithm", null, new Oid( testCert.Cert.GetKeyAlgorithm() ) );
        var rawRecipient = new CmsRecipient(SubjectIdentifierType.IssuerAndSerialNumber, testCert.Cert);

        // Omatanje sadržaja.
        rawCms.Encrypt( rawRecipient );

        // Kodiranje u ASN.1.
        var envelopedData = rawCms.Encode();

        // Odmatanje.

        // Provjera tipa.
        var envelopedContentType = ContentInfo.GetContentType(envelopedData);
        Print( 0, "Enveloped content type", null, envelopedContentType );
        Assert.AreEqual( Pkcs7.Oids.EnvelopedData.Value, envelopedContentType.Value );

        // Objekt za omatanje iz kodiranog podatka.
        var envelopedCms1 = new EnvelopedCms();
        envelopedCms1.Decode( envelopedData );

        // Objekt za omatanje iz dekodiranog podatka.
        Assert.AreEqual( Pkcs7.Oids.Data.Value, envelopedCms1.ContentInfo.ContentType.Value );
        var envelopedCms2 = new EnvelopedCms(new ContentInfo(Pkcs7.Oids.Data, envelopedCms1.ContentInfo.Content));

        // Provjera primatelja.
        static void AssertRecipient( CmsRecipient expected, EnvelopedCms cms )
        {
            Assert.HasCount( 1, cms.RecipientInfos );
            var recipientInfo = cms.RecipientInfos[0];
            Assert.AreEqual( expected.RecipientIdentifierType, recipientInfo.RecipientIdentifier.Type );
            Assert.AreEqual( SubjectIdentifierType.IssuerAndSerialNumber, recipientInfo.RecipientIdentifier.Type );
            Assert.IsNotNull( recipientInfo.RecipientIdentifier.Value );
            var envelopedIssuerSerial = ( X509IssuerSerial )recipientInfo.RecipientIdentifier.Value;
            // X590IssuerSerial ne vraća naziv izdavača u kanonskom obliku koji bi se dobio kroz
            // rawRecipient.Certificate.Issuer.Format(false/true), već redoslijedom komponenti kako su navedene u samom
            // zapisu.
            Assert.AreEqual( expected.Certificate.Issuer, envelopedIssuerSerial.IssuerName );
            Assert.AreEqual( expected.Certificate.SerialNumber, envelopedIssuerSerial.SerialNumber );
        }

        // Provjera primatelja u obadva objekta.
        AssertRecipient( rawRecipient, envelopedCms1 );
        Assert.HasCount( 0, envelopedCms2.RecipientInfos );

        // Odmatanje sadržaja pomoću prvog primatelja čiji certifikat nađe u lokalnim spremištima.
        envelopedCms1.Decrypt();
        Assert.ThrowsExactly<InvalidOperationException>( () => envelopedCms2.Decrypt( envelopedCms1.RecipientInfos[0] ) );

        // Usporedba početnog i završnog stanja.
        CollectionAssert.AreEqual( plainData, envelopedCms1.ContentInfo.Content );
    }

    /// <summary>
    /// Istraži ispravan način korištenja <see cref="SignedCms"/> objekta.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void ExploreSigning( int certNo )
    {
        using var testCert = m_runSettings.GetTestCert(certNo, imported: true);

        // Potpisivanje.

        // Sadržaj.
        byte[] plainData = CreateMessage();

        // PKCS #7 ContentInfo tip sa sirovim podatkom.
        var rawContentInfo = new ContentInfo(plainData); // alt. new ContentInfo(Pkcs7.Oids.Data, rawData)
        Assert.AreEqual( Pkcs7.Oids.Data.Value, rawContentInfo.ContentType.Value );

        // Objekt za potpisivanje. Kroz konstruktor se inicijalizira samo sa sirovim podatkom.
        var rawCms = new SignedCms(rawContentInfo, detached: false);
        var rawSigner = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, testCert.Cert);
        Print( 0, "Signer digest algorithm", null, rawSigner.DigestAlgorithm );

        // Dodaje se neki atribut.
        rawSigner.SignedAttributes.Add( new Pkcs9SigningTime( DateTimeOffset.Now.DateTime ) );

        // Sadržaj se potpisuje.
        rawCms.ComputeSignature( rawSigner, silent: m_runSettings.SuppressTestsShowingUI );

        // Kodiranje u ASN.1.
        var signedData = rawCms.Encode();

        // Ovjera potpisa.

        // Provjera tipa.
        var signedContentType = ContentInfo.GetContentType(signedData);
        Print( 0, "Enveloped content type", null, signedContentType );
        Assert.AreEqual( Pkcs7.Oids.SignedData.Value, signedContentType.Value );

        // Objekt za potpisivanje iz kodiranog podatka.
        var signedCms1 = new SignedCms();
        signedCms1.Decode( signedData );

        // Objekt za potpisivanje iz dekodiranog podatka.
        Assert.AreEqual( Pkcs7.Oids.Data.Value, signedCms1.ContentInfo.ContentType.Value );
        var signedCms2 = new SignedCms(new ContentInfo(Pkcs7.Oids.Data, signedCms1.ContentInfo.Content), detached: false);

        // Provjera potpisnika.
        static void AssertSigner( CmsSigner expected, SignedCms cms )
        {
            Assert.HasCount( 1, cms.SignerInfos );
            var signerInfo = cms.SignerInfos[0];
            Assert.AreEqual( expected.SignerIdentifierType, signerInfo.SignerIdentifier.Type );
            Assert.AreEqual( SubjectIdentifierType.IssuerAndSerialNumber, signerInfo.SignerIdentifier.Type );
            Assert.IsNotNull( signerInfo.SignerIdentifier.Value );
            var envelopedIssuerSerial = ( X509IssuerSerial )signerInfo.SignerIdentifier.Value;
            // X590IssuerSerial ne vraća naziv izdavača u kanonskom obliku koji bi se dobio kroz
            // rawRecipient.Certificate.Issuer.Format(false/true), već redoslijedom komponenti kako su navedene u samom
            // zapisu.
            Assert.AreEqual( expected.Certificate?.Issuer, envelopedIssuerSerial.IssuerName );
            Assert.AreEqual( expected.Certificate?.SerialNumber, envelopedIssuerSerial.SerialNumber );
        }

        // Provjera potpisnika u obadva objekta.
        AssertSigner( rawSigner, signedCms1 );
        Assert.HasCount( 0, signedCms2.SignerInfos );

        // Verifikacija potpisa.
        signedCms1.CheckSignature( verifySignatureOnly: true );
        Assert.ThrowsExactly<InvalidOperationException>( () => signedCms2.CheckSignature( verifySignatureOnly: true ) );

        // Usporedba početnog i završnog stanja.
        CollectionAssert.AreEqual( plainData, signedCms1.ContentInfo.Content );
    }

    /// <summary>
    /// Potvrdi da <see cref="SignedCms"/> objekt zahtjeva barem jednan potpisni certifikat za svoje operacije.
    /// </summary>
    [TestMethod]
    public void CreateSignedDataWithoutSigner()
    {
        // Sadržaj.
        byte[] plainData = CreateMessage();

        // PKCS #7 ContentInfo tip sa sirovim podatkom.
        var rawContentInfo = new ContentInfo(plainData); // alt. new ContentInfo(Pkcs7.Oids.Data, rawData)
        Assert.AreEqual( Pkcs7.Oids.Data.Value, rawContentInfo.ContentType.Value );

        // Objekt za potpisivanje. Kroz konstruktor se inicijalizira samo sa sirovim podatkom.
        var rawCms = new SignedCms(rawContentInfo, detached: false);

        // Kodiranje u ASN.1.
        Assert.ThrowsExactly<InvalidOperationException>( () => rawCms.Encode() );

        var signer1 = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, null);
        Assert.ThrowsExactly<InvalidOperationException>( () => rawCms.ComputeSignature( signer1 ) );
    }

    /// <summary>
    /// Potvrdi da dekodiranje s <see cref="SignedCms.Decode(byte[])"/> odnosno <see
    /// cref="EnvelopedCms.Decode(byte[])"/> rekonstruira točno isto što je kodirano s <see cref="SignedCms.Encode"/>,
    /// odnosno <see cref="EnvelopedCms.Encode"/>. Također potvrdi da nije moguće iz ASN.1 sintakse dekodirati
    /// šifrirani sadržaj, pa ga opet kodirati pomoću <see cref="EnvelopedCms"/>, već se mora sadržaj po dekodiranju i
    /// dešifrirati i za puni round-trip opet šifrirati, pa kodirati u ASN.1.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void EncodeDecodeEquivalency( int certNo )
    {
        using var testCert = m_runSettings.GetTestCert(certNo, imported: true);

        byte[] plainData = CreateMessage(1);

        // SignedCMS.

        var signedCms1 = new SignedCms(new ContentInfo(plainData));
        signedCms1.ComputeSignature( new CmsSigner( SubjectIdentifierType.IssuerAndSerialNumber, testCert.Cert ), silent: m_runSettings.SuppressTestsShowingUI );
        var signedData = signedCms1.Encode();
        Print( 0, "signedData", null, signedData );

        var signedCms2 = new SignedCms();
        signedCms2.Decode( signedData );

        CollectionAssert.AreEqual( signedCms1.ContentInfo.Content, signedCms2.ContentInfo.Content );
        CollectionAssert.AreEqual( signedData, signedCms2.Encode() );

        // EnvelopedCMS.

        var envelopedCms1 = new EnvelopedCms(new ContentInfo(plainData));
        envelopedCms1.Encrypt( new CmsRecipient( SubjectIdentifierType.IssuerAndSerialNumber, testCert.Cert ) );
        var envelopedData = envelopedCms1.Encode();

        var envelopedCms2 = new EnvelopedCms();
        envelopedCms2.Decode( envelopedData );

        var envelopedCms3 = new EnvelopedCms();
        // Encode(Decode(data)) proizvodi sintaktički neispravan ASN.1, nije zamišljen round-trip, već se Encode može
        // zvati samo nakon zvanja Encrypt.
        Assert.ThrowsExactly<CryptographicException>( () => envelopedCms3.Decode( envelopedCms2.Encode() ) );

        var envelopedCms4 = new EnvelopedCms();
        envelopedCms4.Decode( envelopedData );
        envelopedCms4.Decrypt();

        CollectionAssert.AreEqual( plainData, envelopedCms4.ContentInfo.Content );
    }

    /// <summary>
    /// Potvrdi da je metode <see cref="SignedCms.Encode"/> i <see cref="EnvelopedCms.Encode"/> moguće pozvati
    /// višekratno nad istim objektom, tj. da ne prebace objekt u neko finalizirano stanje.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void EncodeImmutablity( int certNo )
    {
        using var testCert = m_runSettings.GetTestCert(certNo, imported: true);

        byte[] plainData = CreateMessage(1);

        // SignedCMS.

        var signedCms = new SignedCms(new ContentInfo(plainData));
        signedCms.ComputeSignature( new CmsSigner( SubjectIdentifierType.IssuerAndSerialNumber, testCert.Cert ), silent: m_runSettings.SuppressTestsShowingUI );
        CollectionAssert.AreEqual( signedCms.Encode(), signedCms.Encode() );

        // EnvelopedCMS.

        var envelopedCms = new EnvelopedCms(new ContentInfo(plainData));
        envelopedCms.Encrypt( new CmsRecipient( SubjectIdentifierType.IssuerAndSerialNumber, testCert.Cert ) );
        CollectionAssert.AreEqual( envelopedCms.Encode(), envelopedCms.Encode() );
    }

    /// <summary>
    /// Potvrdi da <see cref="EnvelopedCms"/> objekt ne može ciklički šifrirati, pa dektriptirati vlastiti sadržaj.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void EnvelopedRecrypting( int certNo )
    {
        using var testCert = m_runSettings.GetTestCert(certNo, imported: true);

        byte[] plainData = CreateMessage(1);

        var envelopedCms = new EnvelopedCms(new ContentInfo(plainData));
        envelopedCms.Encrypt( new CmsRecipient( SubjectIdentifierType.IssuerAndSerialNumber, testCert.Cert ) );
        Assert.Throws<CryptographicException>( () => envelopedCms.Decrypt() );
        CollectionAssert.AreEqual( envelopedCms.Encode(), envelopedCms.Encode() );
        Assert.Throws<CryptographicException>( () => envelopedCms.Decrypt() );
    }

    /// <summary>
    /// Provjeri koje tipove podataka proizvedu različite PKCS #7 operacije.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void ContentTypeTransformations( int certNo )
    {
        using var testCert = m_runSettings.GetTestCert(certNo, imported: true);
        byte[] content = CreateMessage(1);
        Print( 0, "SignedData Type", null, Pkcs7.GetContentTypeOid( SignMessage( content, testCert.Cert ) ) );
        Print( 0, "EnvelopedData Type", null, Pkcs7.GetContentTypeOid( EnvelopeMessage( content, testCert.Cert ) ) );
        Print( 0, "EnvelopedData(SignedData) Type", null, Pkcs7.GetContentTypeOid( EnvelopeMessage( SignMessage( content, testCert.Cert ), testCert.Cert ) ) );
        var signedCms = new SignedCms();
        signedCms.Decode( SignMessage( content, testCert.Cert ) );
        Print( 0, "Decoded SignedData Type", null, signedCms.ContentInfo.ContentType );
        Print( 0, "Decoded EnvelopedData(SignedData) Type", null, Pkcs7.GetContentTypeOid( ReadEnvelopedMessage( EnvelopeMessage( SignMessage( content, testCert.Cert ), testCert.Cert ), testCert.Cert ) ) );
    }

    /// <summary>
    /// Potpiše <see href="TestCase.txt">TestCase.txt</see> datoteku. Usporediti ispis ovog testa s ispisom testa <see
    /// cref="Exploratory_AkdshSigner.ExploreSignedFile(int)"/>.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCASE_SOFT_NONE )]  // Usporediti s priloženom odnosnom datotekom potpisanom od AKDSH Signera.
    [DataRow( RunSettings.TESTCASE_BLUE_NONE )]  // Usporediti s priloženom odnosnom datotekom potpisanom od AKDSH Signera.
    [DataRow( RunSettings.TESTCASE_WHITE_NONE )] // Testira potpisivanje novom bijelom karticom.
    public void SignTestData( int testNo )
    {
        m_runSettings.CancelIfExploratorySuppressed();

        var testCase = m_runSettings.GetTestCase(testNo);

        using var signCert = m_runSettings.GetTestCert(testCase.SignCertNo, imported: true);

        var plainData = File.ReadAllBytes( m_runSettings.GetDeployedFilePath( testCase.ContentFileName ) );
        var contentInfo = new ContentInfo(plainData);
        Assert.AreEqual( Pkcs7.Oids.Data.Value, contentInfo.ContentType.Value );

        var signedCms = new SignedCms(contentInfo, detached: false);
        Assert.AreEqual( Pkcs7.Oids.Data.Value, signedCms.ContentInfo.ContentType.Value );

        var cmsSigner = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, signCert.Cert)
        {
            IncludeOption = X509IncludeOption.EndCertOnly
        };
        cmsSigner.SignedAttributes.Add( new Pkcs9SigningTime( DateTimeOffset.Now.DateTime ) );
        if(testCase.SignAlg is not null)
            cmsSigner.DigestAlgorithm = Oid.FromFriendlyName( testCase.SignAlg, OidGroup.HashAlgorithm );
        signedCms.ComputeSignature( cmsSigner, silent: m_runSettings.SuppressTestsShowingUI );
        Assert.AreEqual( Pkcs7.Oids.Data.Value, signedCms.ContentInfo.ContentType.Value );

        var encodedData = signedCms.Encode();
        Assert.AreEqual( Pkcs7.Oids.Data.Value, signedCms.ContentInfo.ContentType.Value );

        contentInfo = new ContentInfo( encodedData );
        Assert.AreEqual( Pkcs7.Oids.Data.Value, contentInfo.ContentType.Value );
        Assert.AreEqual( Pkcs7.Oids.SignedData.Value, ContentInfo.GetContentType( encodedData ).Value );

        var cms = new SignedCms();
        cms.Decode( encodedData );

        Print( 0, "Signed CMS", null, cms );
    }

    /// <summary>
    /// Šifrira <see href="TestCase.txt">TestCase.txt</see> datoteku, bez potpisivanja reproducirajući testne datoteke
    /// kreirane AKDSH Signerom. Usporediti ispis ovog testa s ispisom testa <see
    /// cref="Exploratory_AkdshSigner.ExploreSignedAndEncryptedFile(int)"/>.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCASE_SOFT_BLUE )] // Usporediti s priloženom istoimenom datotekom potpisanom i šifriranom od AKDSH Signera.
    public void EnvelopeTestData( int testNo )
    {
        m_runSettings.CancelIfExploratorySuppressed();

        var testCase = m_runSettings.GetTestCase(testNo);

        Assert.IsNotNull( testCase.CryptCertNo );
        using var cryptCert = m_runSettings.GetTestCert(testCase.CryptCertNo.Value, imported: true);

        var plainData = File.ReadAllBytes( m_runSettings.GetDeployedFilePath( testCase.ContentFileName ) );
        var contentInfo = new ContentInfo(plainData);
        var envelopedCms = testCase.CryptAlg is null
            ? new EnvelopedCms(contentInfo)
            : new EnvelopedCms(contentInfo, new AlgorithmIdentifier(Oid.FromFriendlyName(testCase.CryptAlg, OidGroup.EncryptionAlgorithm)));

        var recipient = new CmsRecipient(SubjectIdentifierType.IssuerAndSerialNumber, cryptCert.Cert!);
        Console.WriteLine( "Key Algorithm = " + recipient.Certificate.GetKeyAlgorithm() );
        Console.WriteLine( "Signature Algorithm = " + recipient.Certificate.SignatureAlgorithm.Value );
        envelopedCms.Encrypt( recipient );
        var encodedData = envelopedCms.Encode();

        var cms = new EnvelopedCms();
        cms.Decode( encodedData );

        Print( 0, "Enveloped CMS", null, cms );
    }

    /// <summary>
    /// Izlista OID-ove PKCS #7 tipova podataka, zajedno s njihovim neslužbenim nazivima po kojima su poznati u .NET
    /// biblioteci.
    /// </summary>
    [TestMethod]
    public void ListContentTypeOids()
    {
        m_runSettings.CancelIfExploratorySuppressed();

        const string DataOid = "1.2.840.113549.1.7.1";
        const string SignedDataOid = "1.2.840.113549.1.7.2";
        const string EnvelopedDataOid = "1.2.840.113549.1.7.3";
        const string SignedAndEnvelopedDataOid = "1.2.840.113549.1.7.4";
        const string DigestedDataOid = "1.2.840.113549.1.7.5";
        const string EncryptedDataOid = "1.2.840.113549.1.7.6";

        Console.WriteLine( $"{nameof( DataOid )} = ({Oid.FromOidValue( DataOid, OidGroup.All ).FriendlyName}, {DataOid})" );
        Console.WriteLine( $"{nameof( SignedDataOid )} = ({Oid.FromOidValue( SignedDataOid, OidGroup.All ).FriendlyName}, {SignedDataOid})" );
        Console.WriteLine( $"{nameof( EnvelopedDataOid )} = ({Oid.FromOidValue( EnvelopedDataOid, OidGroup.All ).FriendlyName}, {EnvelopedDataOid})" );
        Console.WriteLine( $"{nameof( SignedAndEnvelopedDataOid )} = ({Oid.FromOidValue( SignedAndEnvelopedDataOid, OidGroup.All ).FriendlyName}, {SignedAndEnvelopedDataOid})" );
        Console.WriteLine( $"{nameof( DigestedDataOid )} = ({Oid.FromOidValue( DigestedDataOid, OidGroup.All ).FriendlyName}, {DigestedDataOid})" );
        Console.WriteLine( $"{nameof( EncryptedDataOid )} = ({Oid.FromOidValue( EncryptedDataOid, OidGroup.All ).FriendlyName}, {EncryptedDataOid})" );
    }

    /// <summary>
    /// Izlista ECC/RSA algoritme svih certifikata u korisnikovom spremištu osobnih certifikata.
    /// </summary>
    [TestMethod]
    public void ListUserCertificateAlgorithms()
    {
        m_runSettings.CancelIfExploratorySuppressed();

        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly);
        foreach(var cert in store.Certificates)
        {
            static string FormatOid( Oid? oid )
                => (oid?.FriendlyName is null ? oid?.Value : $"{oid.FriendlyName} ({oid.Value})") ?? "n/a";

            static string FormatOidFromValue( string? value )
            {
                if(string.IsNullOrEmpty( value ))
                    return "n/a";
                try
                {
                    return FormatOid( Oid.FromOidValue( value, OidGroup.All ) );
                }
                catch(Exception ex)
                {
                    return $"{value} ({ex.Message})";
                }
            }

            static Oid? ReadEccCurve( AsnEncodedData? data )
            {
                if(data?.RawData is null || 0 == data.RawData.Length)
                    return null;
                try
                {
                    var reader = new AsnReader(data.RawData, AsnEncodingRules.DER);
                    string curveOidValue = reader.ReadObjectIdentifier();
                    reader.ThrowIfNotEmpty();
                    return new Oid( curveOidValue );
                }
                catch(AsnContentException)
                {
                    return null;
                }
            }

            Console.WriteLine( $"== [{cert.FriendlyName}] ==" );
            Console.WriteLine( "  Friendly name            = " + cert.FriendlyName );
            Console.WriteLine( "  Issuer                   = " + cert.Issuer );
            Console.WriteLine( "  Serial number            = " + cert.SerialNumber );
            // Ovo je algoritam privatnog ključa.
            Console.WriteLine( "  Key algoritm             = " + FormatOidFromValue( cert.GetKeyAlgorithm() ) );
            // Ovo će vratiti isti OID kao i GetKeyAlgorithm():
            Console.WriteLine( "  Public key               = " + FormatOid( cert.PublicKey.Oid ) );
            // Ovo će vratiti isti OID kao i GetKeyAlgorithm();
            Console.WriteLine( "  Parameters OID           = " + FormatOid( cert.PublicKey.EncodedParameters?.Oid ) );
            Console.WriteLine( "  Parameters raw data      = " + cert.PublicKey.EncodedParameters?.Format( false ) ?? "n/a" );
            AsymmetricAlgorithm? key = null;
            if("ECC" == cert.PublicKey.Oid.FriendlyName)
            {
                Console.WriteLine( "  Curve                    = " + FormatOid( ReadEccCurve( cert.PublicKey.EncodedParameters ) ) );
                key = cert.GetECDsaPublicKey();
            }
            else if("RSA" == cert.PublicKey.Oid.FriendlyName)
                key = cert.GetRSAPublicKey();
            Console.WriteLine( "  Key size                 = " + key?.KeySize ?? "n/a" );
            Console.WriteLine( "  Key exchange algorithms  = " + key?.KeyExchangeAlgorithm );
            Console.WriteLine( "  Key signature algorithm  = " + key?.SignatureAlgorithm );
            // Ovo je algoritam (asimetričnog ključa i digitalnog sažetka) kojim je izdavač certifikata potpisao sâm
            // certifikat, nevažno za kritpografske funkcije podržane kriptografskim uređajem iza certifikata, važno za
            // validaciju izvornosti certifikata.
            Console.WriteLine( "  Cert signature algorithm = " + FormatOid( cert.SignatureAlgorithm ) );
        }
    }

    /// <summary>
    /// Potvrđuje da potpisivanje ne zahtjeva prisutnost potpisnog certifikata u spremištu i da u tom slučaju uključeni
    /// lanac povjerenja neće sadržavati više od potpisnog certifikata.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_ECDSA_SIGN, RunSettings.TESTCERT_CA, RunSettings.TESTCERT_ROOT )]
    public void SigningWithoutImportedCerts( int certNo, int intermCertNo, int rootCertNo )
    {
        m_runSettings.CancelIfExploratorySuppressed();

        // Briše sve certifikate iz spremišta.
        m_runSettings.RemoveTestCertFromStore( certNo );
        m_runSettings.RemoveTestCertFromStore( intermCertNo );
        m_runSettings.RemoveTestCertFromStore( rootCertNo );

        static SignedCms Sign( X509IncludeOption includeOption, X509Certificate2 cert, int expectedChainSize )
        {
            var cms = new SignedCms(new ContentInfo(CreateMessage()));
            var signer = new CmsSigner( SubjectIdentifierType.IssuerAndSerialNumber, cert )
            {
                IncludeOption = includeOption
            };
            cms.ComputeSignature( signer, silent: m_runSettings.SuppressTestsShowingUI );
            Assert.HasCount( expectedChainSize, cms.Certificates );
            return cms;
        }

        // Potpisuje bez da je ijedan certifikat prisutan u spremištima.
        using(var cert = m_runSettings.GetTestCert( certNo, imported: false ))
        {
            // Ne smije sadržavati niti jedan certifikat.
            Print( 0, "Signed CMS with no certificates", null, Sign( X509IncludeOption.None, cert.Cert, 0 ) );
            // Mora sadržavati samo krajnji certifikat jer on je dan parametrom.
            Print( 0, "Signed CMS with end cert only", null, Sign( X509IncludeOption.EndCertOnly, cert.Cert, 1 ) );
            // Mora sadržavati samo krajnji certifikat jer ostali nisu dostupni pošto ne postoje u spremištima.
            Print( 0, "Signed CMS without root cert", null, Sign( X509IncludeOption.ExcludeRoot, cert.Cert, 1 ) );
            Print( 0, "Signed CMS with whole chain", null, Sign( X509IncludeOption.WholeChain, cert.Cert, 1 ) );
        }
    }

    /// <summary>
    /// Potvrđuje da potpisani dokument sadrži cijeli lanac povjerenja ako se tako uputi <see cref="CmsSigner"/>.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_ECDSA_SIGN, RunSettings.TESTCERT_CA, RunSettings.TESTCERT_ROOT )]
    public void SignedDataIncludesTrustChain( int certNo, int intermCertNo, int rootCertNo )
    {
        m_runSettings.CancelIfExploratorySuppressed();

        // Osigurava da su dostupni korijenski i subordinirani certifikat.
        m_runSettings.GetTestCert( rootCertNo, imported: true );
        m_runSettings.GetTestCert( intermCertNo, imported: true );

        static SignedCms Sign( X509IncludeOption? includeOption, X509Certificate2 cert, int expectedChainSize )
        {
            var cms = new SignedCms(new ContentInfo(CreateMessage()));
            var signer = new CmsSigner( SubjectIdentifierType.IssuerAndSerialNumber, cert );
            if(includeOption is not null)
                signer.IncludeOption = includeOption.Value;
            cms.ComputeSignature( signer, silent: m_runSettings.SuppressTestsShowingUI );
            Assert.HasCount( expectedChainSize, cms.Certificates );
            return cms;
        }

        // Potpisuje sa svim certifikatima prisutnima u spremištu.
        using(var cert = m_runSettings.GetTestCert( certNo, imported: true ))
        {
            // Potvrda da lanac povjerenja ima ukupno 3 certifikata.
            using var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            Assert.IsTrue( chain.Build( cert.Cert ) );
            Assert.HasCount( 3, chain.ChainElements );
            // Ne smije sadržavati niti jedan certifikat.
            Print( 0, "Signed CMS with no certificates", null, Sign( X509IncludeOption.None, cert.Cert, 0 ) );
            // Mora sadržavati samo krajnji certifikat jer on je dan parametrom.
            Print( 0, "Signed CMS with end cert only", null, Sign( X509IncludeOption.EndCertOnly, cert.Cert, 1 ) );
            // Mora sadržavati krajnji i subordinirani certifikat.
            Print( 0, "Signed CMS without root cert", null, Sign( X509IncludeOption.ExcludeRoot, cert.Cert, 2 ) );
            // Mora sadržavati sve certifikate.
            Print( 0, "Signed CMS with whole chain", null, Sign( X509IncludeOption.WholeChain, cert.Cert, 3 ) );
            // Prešutna opcija mora biti ExcludeRoot.
            Print( 0, "Signed CMS with default chain", null, Sign( null, cert.Cert, 2 ) );
        }
    }

    /// <summary>
    /// Potvrđuje da je za za ovjeru potpisa bez provjere lanca povjerenja nužno i dovoljno da je potpisni certifikat
    /// prisutan u poruci.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_ECDSA_SIGN, RunSettings.TESTCERT_CA, RunSettings.TESTCERT_ROOT )]
    public void SignatureValidationRequiresOnlyRootCertificate( int certNo, int intermCertNo, int rootCertNo )
    {
        m_runSettings.CancelIfExploratorySuppressed();

        // Osigura da se u spremištu ne nalazi niti jedan certifikat.
        m_runSettings.RemoveTestCertFromStore( certNo );
        m_runSettings.RemoveTestCertFromStore( intermCertNo );
        m_runSettings.RemoveTestCertFromStore( rootCertNo );

        static byte[] Sign( X509Certificate2 cert, X509IncludeOption includeOption )
        {
            var cms = new SignedCms(new ContentInfo(CreateMessage()));
            var signer = new CmsSigner( SubjectIdentifierType.IssuerAndSerialNumber, cert )
            {
                IncludeOption = includeOption
            };
            cms.ComputeSignature( signer, silent: m_runSettings.SuppressTestsShowingUI );
            return cms.Encode();
        }

        byte[] signed;

        // Potpisuje bez da je ijedan certifikat prisutan u spremištima i bez certifikata u samoj poruci.
        using(var cert = m_runSettings.GetTestCert( certNo, imported: false ))
            signed = Sign( cert.Cert, X509IncludeOption.None );
        var cmsWithoutSigner = new SignedCms();
        cmsWithoutSigner.Decode( signed );

        // Ovjerava potpis bez dostupnosti ijednog certifikata, pa niti potpisnog. Nije moguće jer cerifikat nije
        // dostupan.
        Assert.ThrowsExactly<CryptographicException>( () => cmsWithoutSigner.CheckSignature( verifySignatureOnly: true ) );

        // Ovjerava potpis uz dostupnost potpisnog certifikata u spremištu. Nije moguće jer metoda CheckSignature
        // naprosto provjerava je li sadržaj doista potpisan certifikatom koji je priložen u SignedData strukturi.
        using(var cert = m_runSettings.GetTestCert( certNo, imported: true ))
            Assert.ThrowsExactly<CryptographicException>( () => cmsWithoutSigner.CheckSignature( verifySignatureOnly: true ) );

        // Potpisuje bez da je ijedan certifikat prisutan u spremištima, ali sa certifikatom u samoj poruci.
        using(var cert = m_runSettings.GetTestCert( certNo, imported: false ))
            signed = Sign( cert.Cert, X509IncludeOption.EndCertOnly );
        var cmsWithSigner = new SignedCms();
        cmsWithSigner.Decode( signed );

        // Ovjerava potpis, tj. uvrđuje da je podatak potpisan certifikatom sadržanim u samoj poruci.
        cmsWithSigner.CheckSignature( verifySignatureOnly: true );

        // Ovjera s verifySignatureOnly nije moguća jer samo-potpisni certifikat izrađen za ovo testiranje nema adresu
        // objave liste povučenih certifikata, pa CheckSignature ne može provjeriti tu listu i automatski završava
        // neuspješno.
    }
}

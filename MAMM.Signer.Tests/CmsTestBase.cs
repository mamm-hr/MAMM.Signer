using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace MAMM.Signer.Tests;

public class CmsTestBase : TestBase
{
    protected static void AssertEqual( EnvelopedCms expected, EnvelopedCms actual )
    {
        // Očekuje se da obadva sadržaja imaju asociran samo jedan certifikat. Obadva certifikata moraju biti ista.
        Assert.HasCount( 0, expected.Certificates, "Ne očekuju se asocirani certifikati." );
        Assert.HasCount( expected.Certificates.Count, actual.Certificates );

        // Usporedba opisnika algoritama.
        static void AssertEqualAlgorithm( AlgorithmIdentifier expected, AlgorithmIdentifier actual )
        {
            Assert.AreEqual( expected.KeyLength, actual.KeyLength );
            Assert.AreEqual( expected.Oid.Value, actual.Oid.Value );
            CollectionAssert.AreEqual( expected.Parameters, actual.Parameters );
        }

        // Algoritmi simetrične enkripcije su isti.
        AssertEqualAlgorithm( expected.ContentEncryptionAlgorithm, actual.ContentEncryptionAlgorithm );

        // Vrste sadržaja su iste.
        Assert.AreEqual( expected.ContentInfo.ContentType.Value, actual.ContentInfo.ContentType.Value );

        // Sadržaji ne moraju biti binarno identični. Sadržaj je SignedData tip podatka koji se treba dekodirati i onda
        // usporediti pomoću AssertEqual za SignedCms.

        // Usporedba podataka primatelja.
        static void AssertEqualRecipient( RecipientInfo expected, RecipientInfo actual )
        {
            // šifrirani tekst enkripcijskog ključa ne može biti isti jer pošiljatelj generira ključ slučajno.
            CollectionAssert.AreNotEqual( expected.EncryptedKey, actual.EncryptedKey );

            // Algoritmi asimetrične enkripcije ključeva su isti.
            AssertEqualAlgorithm( expected.KeyEncryptionAlgorithm, actual.KeyEncryptionAlgorithm );

            // Usporedba identiteta certifikata.
            static void AssertEqualSubjectIdentifier( SubjectIdentifier expected, SubjectIdentifier actual )
            {
                Assert.AreEqual( expected.Type, actual.Type );
                switch(expected.Type)
                {
                    case SubjectIdentifierType.IssuerAndSerialNumber:
                        Assert.IsExactInstanceOfType<X509IssuerSerial>( expected.Value );
                        Assert.IsExactInstanceOfType<X509IssuerSerial>( actual.Value );
                        Assert.AreEqual( ((X509IssuerSerial)expected.Value).IssuerName, ((X509IssuerSerial)actual.Value).IssuerName );
                        Assert.AreEqual( ((X509IssuerSerial)expected.Value).SerialNumber, ((X509IssuerSerial)actual.Value).SerialNumber );
                        break;
                    case SubjectIdentifierType.SubjectKeyIdentifier:
                        Assert.IsExactInstanceOfType<string>( expected.Value );
                        Assert.IsExactInstanceOfType<string>( actual.Value );
                        Assert.AreEqual( (string)expected.Value, (string)actual.Value );
                        break;
                }
            }

            // Identiteti primatelja su isti.
            AssertEqualSubjectIdentifier( expected.RecipientIdentifier, actual.RecipientIdentifier );

            // Protokoli za utvrđivanje ekcripcijskog ključa su isti.
            Assert.AreEqual( expected.Type, actual.Type );

            // Očekuje se protokol za utvrđivanje enkripcijskog ključa temeljen na transportu, tj. jedna strana generira
            // ključ i transportira ga drugoju uz poruku, ali šifrirnog javnim ključem druge strane. Alternativa bi
            // bila da obje strane sudjeluju u generiranju ključa i složne su oko njega iako ga nikad ne razmjene
            // (Diffie-Hellman algoritam)..
            Assert.AreEqual( RecipientInfoType.KeyTransport, actual.Type, "Očekuje se utvrđivanje ekripcijskog ključa transportom." );

            // Usporedba protokola za utvrđivanje ključa.
            var ktExpected = (KeyTransRecipientInfo)expected;
            var ktAtual = (KeyTransRecipientInfo)actual;
            Assert.AreEqual( ktExpected.Version, ktAtual.Version );
        }

        // Očekuje se jedan primatelj i primatelji su isti.
        Assert.HasCount( 1, expected.RecipientInfos, "Očekuje se samo jedan primatelj." );
        Assert.HasCount( expected.RecipientInfos.Count, actual.RecipientInfos );
        AssertEqualRecipient( expected.RecipientInfos[0], actual.RecipientInfos[0] );

        // Ne očekuje otvoreno slanje atributa.
        Assert.HasCount( 0, expected.UnprotectedAttributes, "Ne očekuju se atributi u otvorenom tekstu." );
        Assert.HasCount( expected.UnprotectedAttributes.Count, actual.UnprotectedAttributes );

        // Verzije podatkovne strukture datoteke su iste.
        Assert.AreEqual( expected.Version, actual.Version );
    }

    /// <summary>
    /// Uspoređuje bitne dijelove datoteke potpisane od AKDSH Signera i datoteke potpisane testiranom bibliotekom.
    /// </summary>
    ///
    /// <param name="expected">
    ///     Sadržaj potpisan od AKDSH Signera.</param>
    ///
    /// <param name="actual">
    ///     Sadržaj potpisan testiranom datotekaom.</param>
    ///
    /// <remarks>
    /// <para>
    ///     U usporedbi se očekuje da će AKDSH Signer dodati NULL-parametar u AlgorithmIdentifier tip podatka, dok ova
    ///     biblioteka to može i ne mora.</para>
    /// </remarks>
    ///
    protected static void AssertEqual( SignedCms expected, SignedCms actual )
    {
        // Očekuje se da obadva sadržaja imaju asociran samo jedan certifikat. Obadva certifikata moraju biti ista.
        Assert.HasCount( 1, expected.Certificates, "Očekuje se samo jedan asocirani certifikat." );
        Assert.HasCount( expected.Certificates.Count, actual.Certificates );
        Assert.AreEqual( expected.Certificates[0].Thumbprint, actual.Certificates[0].Thumbprint );

        // Tip sadržaja mora biti obilježen istim OID-om.
        Assert.AreEqual( expected.ContentInfo.ContentType.Value, actual.ContentInfo.ContentType.Value );

        // Provjerava da se radi o identičnim potpisanim sadržajima.
        CollectionAssert.AreEqual( expected.ContentInfo.Content, actual.ContentInfo.Content );

        // Provjerava da obje datoteke sadrže ili ne sadrže potpisani sadržaj.
        Assert.AreEqual( expected.Detached, actual.Detached );

        // Preduvjet testiranja je da postoji samo jedan potpisnik.
        Assert.HasCount( 1, expected.SignerInfos, "Očekuje se samo jedan potpisnik." );
        Assert.HasCount( expected.SignerInfos.Count, actual.SignerInfos );

        // Usporedba podataka potpisnika.
        static void AssertEqualSigner( SignerInfo expected, SignerInfo actual )
        {
            // Isti certifikat.
            Assert.AreEqual( expected.Certificate?.Thumbprint, actual.Certificate?.Thumbprint );

            // Nema supotpisnika (supotpisnik potpisuje potpis, a ne sadržaj).
            Assert.HasCount( 0, expected.CounterSignerInfos, "Ne očekuju se supotpisnici." );
            Assert.HasCount( 0, actual.CounterSignerInfos );

            // Isti algoritam digitalnog sažetka (npr SHA-1).
            Assert.AreEqual( expected.DigestAlgorithm.Value, actual.DigestAlgorithm.Value );

            // Isti algoritam potpisa (npr. RSA).
            Assert.AreEqual( expected.SignatureAlgorithm.Value, actual.SignatureAlgorithm.Value );

            // Isti broj atributa potpisanih skupa sa sadržajem.
            Assert.HasCount( 3, expected.SignedAttributes, "Očekuju se tri potpisana atributa." );
            Assert.HasCount( 3, actual.SignedAttributes );

            // Usporedba atributa.
            static void AssertEqualAttr( CryptographicAttributeObject expected, CryptographicAttributeObject actual )
            {
                // Atributi su iste vrste (OID) i broja podatkovnih elemenata.
                Assert.AreEqual( expected.Oid.Value, actual.Oid.Value );
                Assert.HasCount( expected.Values.Count, actual.Values );

                // Usporedba podatkovnih elementa potpisa.
                static void AssertEqual( AsnEncodedData expected, AsnEncodedData actual )
                {
                    // Elemnti su istog tipa (OID) i identičnog binarnog sadržaja.
                    Assert.AreEqual( expected.Oid?.Value, actual.Oid?.Value );
                    CollectionAssert.AreEqual( expected.RawData, actual.RawData );
                }

                // Atributi imaju iste podatkovne elemente i u istom poretku.
                for(int i = 0; i < actual.Values.Count; i++)
                    AssertEqual( expected.Values[i], actual.Values­[i] );
            }

            // Svi potpisani atributi su identični i u istom poretku.
            for(int i = 0; i < expected.SignedAttributes.Count; i++)
                AssertEqualAttr( expected.SignedAttributes[i], actual.SignedAttributes[i] );

            // Potpisnika se identificira na isti način.
            Assert.AreEqual( expected.SignerIdentifier.Type, actual.SignerIdentifier.Type );
            // Potpisnika se identificira po izdavaču i serijskom broju njegovog certifikata.
            Assert.IsExactInstanceOfType<X509IssuerSerial>( expected.SignerIdentifier.Value,
                "Očekuje se da je potpisnik identificiran izdavačem i serijskim brojem certifikata." );
            Assert.IsExactInstanceOfType<X509IssuerSerial>( actual.SignerIdentifier.Value );

            // Usporedba identiteta certifikata.
            static void AssertEqualSerial( X509IssuerSerial? expected, X509IssuerSerial? actual )
            {
                Assert.AreEqual( expected?.IssuerName, actual?.IssuerName );
                Assert.AreEqual( expected?.SerialNumber, actual?.SerialNumber );
            }

            // Potpisnici su isti.
            AssertEqualSerial( (X509IssuerSerial?)expected.SignerIdentifier.Value, (X509IssuerSerial?)actual.SignerIdentifier.Value );

            // Nema dodatnih (nepotpisanih) atributa.
            Assert.HasCount( 0, expected.UnsignedAttributes, "Ne očekuju se nepotpisani atributi." );
            Assert.HasCount( 0, actual.UnsignedAttributes );

            // Verzije podaktovne strukure potpisnika su iste.
            Assert.AreEqual( expected.Version, actual.Version );
        }

        // Isti podaci vezani uz potpisnike.
        AssertEqualSigner( expected.SignerInfos[0], actual.SignerInfos[0] );

        // Verzije podatkovne strukture datoteke su iste.
        Assert.AreEqual( expected.Version, actual.Version );
    }

    /// <summary>
    /// Kreira sadržaj poruke.
    /// </summary>
    /// <returns>
    ///     Vrati slučajno generiran niz okteta slučajno određene duljine.</returns>
    protected static byte[] CreateMessage( int length = -1 )
    {
        byte[] message = new byte[0 <= length ? length : (Random.Shared.Next(1024) + 1)];
        Random.Shared.NextBytes( message );
        return message;
    }

    /// <summary>
    /// Kuvertira poruku jednom primatelju.
    /// </summary>
    /// <param name="message">
    ///     Poruka.</param>
    /// <param name="cert">
    ///     Primateljev certifikat.</param>
    /// <returns>
    ///     EnvelopedData CMS podatak.</returns>
    protected static byte[] EnvelopeMessage( byte[] message, X509Certificate2 cert )
    {
        var cms = new EnvelopedCms(new ContentInfo(message));
        cms.Encrypt( new CmsRecipient( SubjectIdentifierType.IssuerAndSerialNumber, cert ) );
        return cms.Encode();
    }

    /// <summary>
    /// Otvara kuvertiranu poruku.
    /// </summary>
    /// <param name="message">
    ///     Poruka.</param>
    /// <param name="cert">
    ///     Primateljev certifikat.</param>
    /// <returns>
    ///     Sadržaj kuverirane poruke.</returns>
    protected static byte[] ReadEnvelopedMessage( byte[] message, X509Certificate2 certificate )
    {
        var cms = new EnvelopedCms();
        cms.Decode( message );
        cms.Decrypt( new X509Certificate2Collection( certificate ) );
        return cms.ContentInfo.Content;
    }

    /// <summary>
    /// Čita sadržaj potpisane poruke.
    /// </summary>
    /// <param name="message">
    ///     Poruka.</param>
    /// <returns>
    ///     Sadržaj potpisane poruke.</returns>
    protected static byte[] ReadSignedMessage( byte[] message )
    {
        var cms = new SignedCms();
        cms.Decode( message );
        return cms.ContentInfo.Content;
    }

    protected static void Print( int indent, string name, int? no, AlgorithmIdentifier value )
    {
        Print( indent++, name, no, "" );
        Print( indent, "Key Length", null, value.KeyLength );
        Print( indent, "Oid", null, value.Oid );
        Print( indent, "Parameters Count", null, value.Parameters.Length );
        for(int i = 0; i < value.Parameters.Length; i++)
            Print( indent, "Parameters", i, value.Parameters[i] );
    }

    protected static void Print( int indent, string name, int? no, AsnEncodedData value )
    {
        Print( indent++, name, no, "" );
        if(value.Oid is not null)
            Print( indent, "Oid", null, value.Oid );
        Print( indent, "Value", null, value.Format( false ) );
    }

    protected static void Print( int indent, string name, int? no, ContentInfo value )
    {
        Print( indent++, name, no, "" );
        Print( indent, "Type", null, value.ContentType );
        Print( indent, "Length", null, value.Content.Length );
    }

    protected static void Print( int indent, string name, int? no, CryptographicAttributeObject value )
    {
        Print( indent++, name, no, "" );
        Print( indent, "Oid", null, value.Oid );
        Print( indent, "Value Count", null, value.Values.Count );
        for(int i = 0; i < value.Values.Count; i++)
            Print( indent, "Value", i, value.Values[i] );
    }

    protected static void Print( int indent, string name, int? no, EnvelopedCms value )
    {
        Print( indent++, name, no, "" );

        Print( indent, "Certificate Count", no, value.Certificates.Count );
        for(int i = 0; i < value.Certificates.Count; i++)
            Print( indent, "Certificate", i, value.Certificates[i] );

        Print( indent, "Content Encryption Algorithm", null, value.ContentEncryptionAlgorithm );
        Print( indent, "Content", null, value.ContentInfo );

        Print( indent, "Recipient Count", null, value.RecipientInfos.Count );
        for(int i = 0; i < value.RecipientInfos.Count; i++)
            Print( indent, "Recipient", i, value.RecipientInfos[i] );

        Print( indent, "Unprotected Attributes Count", null, value.UnprotectedAttributes.Count );
        for(int i = 0; i < value.UnprotectedAttributes.Count; i++)
            Print( indent, "Unprotected Attribute", i, value.UnprotectedAttributes[i] );

        Print( indent, "Version", null, value.Version );
    }

    protected static void Print( int indent, string name, int? no, KeyAgreeRecipientInfo value )
    {
        Print( indent++, name, no, "" );
        Print( indent, "Date", null, value.Date );
        Print( indent, "Encrypted Key", null, value.EncryptedKey );
        Print( indent, "Key Encryption Algorithm", null, value.KeyEncryptionAlgorithm );
        Print( indent, "OriginatorIdentifierOrKey", null, value.OriginatorIdentifierOrKey );
        if(value.OtherKeyAttribute is not null)
            Print( indent, "OtherKeyAttribute", null, value.OtherKeyAttribute );
        Print( indent, "Recipient Identifier", null, value.RecipientIdentifier );
        Print( indent, "Recipient Type", null, value.Type.ToString() );
        Print( indent, "Version", null, value.Version );
    }

    protected static void Print( int indent, string name, int? no, KeyTransRecipientInfo value )
    {
        Print( indent++, name, no, "" );
        Print( indent, "Encrypted Key", null, value.EncryptedKey );
        Print( indent, "Key Encryption Algorithm", null, value.KeyEncryptionAlgorithm );
        Print( indent, "Recipient Identifier", null, value.RecipientIdentifier );
        Print( indent, "Recipient Type", null, value.Type.ToString() );
        Print( indent, "Version", null, value.Version );
    }

    protected static void Print( int indent, string name, int? no, RecipientInfo value )
    {
        switch(value.Type)
        {
            case RecipientInfoType.KeyAgreement:
                Assert.IsExactInstanceOfType<KeyAgreeRecipientInfo>( value );
                Print( indent, name, no, (KeyAgreeRecipientInfo)value );
                break;
            case RecipientInfoType.KeyTransport:
                Assert.IsExactInstanceOfType<KeyTransRecipientInfo>( value );
                Print( indent, name, no, (KeyTransRecipientInfo)value );
                break;
            default:
                Print( indent++, name, no, "" );
                Print( indent, "Encrypted Key", null, value.EncryptedKey );
                Print( indent, "Key Encryption Algorithm", null, value.KeyEncryptionAlgorithm );
                Print( indent, "Recipient Identifier", null, value.RecipientIdentifier );
                Print( indent, "Recipient Type", null, value.Type.ToString() );
                Print( indent, "Version", null, value.Version );
                break;
        }
    }

    protected static void Print( int indent, string name, int? no, SignedCms value )
    {
        Print( indent++, name, no, "" );

        Print( indent, "Certificate Count", null, value.Certificates.Count );
        for(int i = 0; i < value.Certificates.Count; i++)
            Print( indent, "Certificate", i, value.Certificates[i] );

        Print( indent, "Content", null, value.ContentInfo );

        Print( indent, "Detached", null, value.Detached );

        Print( indent, "Signer Count", null, value.SignerInfos.Count );
        for(int i = 0; i < value.SignerInfos.Count; i++)
            Print( indent, "Signer", i, value.SignerInfos[i] );

        Print( indent, "Version", null, value.Version );
    }

    protected static void Print( int indent, string name, int? no, SignerInfo value )
    {
        Print( indent++, name, no, "" );
        Print( indent, "Certificate", null, value.Certificate );
        Print( indent, "Counter Signer Count", null, value.CounterSignerInfos.Count );
        for(int i = 0; i < value.CounterSignerInfos.Count; i++)
            Print( indent, "Counter Signer", i, value.CounterSignerInfos[i] );
        Print( indent, "Digest Algorithm", null, value.DigestAlgorithm );
        Print( indent, "Signature Algorithm", null, value.SignatureAlgorithm );
        Print( indent, "Signed Attributes Count", null, value.SignedAttributes.Count );
        for(int i = 0; i < value.SignedAttributes.Count; i++)
            Print( indent, "Signed Attribute", i, value.SignedAttributes[i] );
        Print( indent, "Subject Identifier Type", null, value.SignerIdentifier );
        Print( indent, "Unsigned Attributes Count", null, value.UnsignedAttributes.Count );
        for(int i = 0; i < value.UnsignedAttributes.Count; i++)
            Print( indent, "Unsigned Attribute", i, value.UnsignedAttributes[i] );
        Print( indent, "Version", null, value.Version );
    }

    protected static void Print( int indent, string name, int? no, SubjectIdentifier value )
    {
        Print( indent++, name, no, "" );
        Print( indent, "Type", null, value.Type );
        switch(value.Type)
        {
            case SubjectIdentifierType.IssuerAndSerialNumber:
                Assert.IsExactInstanceOfType<X509IssuerSerial>( value.Value );
                Print( indent, "Value", null, (X509IssuerSerial)value.Value );
                break;
            case SubjectIdentifierType.SubjectKeyIdentifier:
                Assert.IsExactInstanceOfType<string>( value.Value );
                Print( indent, "Value", null, (string)value.Value );
                break;
            case SubjectIdentifierType.NoSignature:
            case SubjectIdentifierType.Unknown:
            default:
                Print( indent, "Value", null, $"({value.Value?.GetType().Name ?? "n/a"})" );
                break;
        }
    }

    protected static void Print( int indent, string name, int? no, SubjectIdentifierOrKey value )
    {
        Print( indent++, name, no, "" );
        Print( indent, "Type", null, value.Type );
        switch(value.Type)
        {
            case SubjectIdentifierOrKeyType.IssuerAndSerialNumber:
                Assert.IsExactInstanceOfType<X509IssuerSerial>( value.Value );
                Print( indent, "Value", null, (X509IssuerSerial)value.Value );
                break;
            case SubjectIdentifierOrKeyType.SubjectKeyIdentifier:
                Assert.IsExactInstanceOfType<string>( value.Value );
                Print( indent, "Value", null, (string)value.Value );
                break;
            case SubjectIdentifierOrKeyType.PublicKeyInfo:
            case SubjectIdentifierOrKeyType.Unknown:
            default:
                Print( indent, "Value", null, $"({value.Value?.GetType().Name ?? "n/a"})" );
                break;
        }
    }

    /// <summary>
    /// Potpiše poruku jednim potpisnikom.
    /// </summary>
    /// <param name="message">
    ///     Poruka.</param>
    /// <param name="cert">
    ///     Potpisni certifikat.</param>
    /// <returns>
    ///     SignedData CMS podatak.</returns>
    protected static byte[] SignMessage( byte[] message, X509Certificate2 cert )
    {
        var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, cert);
        signer.SignedAttributes.Add( new Pkcs9SigningTime( DateTimeOffset.Now.DateTime ) );
        var cms = new SignedCms(new ContentInfo(message), detached: false);
        cms.ComputeSignature( signer, silent: false );
        return cms.Encode();
    }
}

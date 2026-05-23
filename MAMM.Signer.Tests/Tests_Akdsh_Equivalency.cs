using MAMM.Signer.Pkcs;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira ekvivalentnost proizvoda ove biblioteke i AKDSH Signera.
/// </summary>
[TestClass]
public class Tests_Akdsh_Equivalency : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Rekonstruira datoteku potpisanu od AKDSH Signera i uspoređuje im sadržaje.
    /// </summary>
    ///
    [TestMethod]
    [DataRow( RunSettings.TESTCASE_SOFT_NONE )]
    [DataRow( RunSettings.TESTCASE_BLUE_NONE )]
    public void A01_ReCreateAkdshSignerSample_VerifyItIsEqual(
          int testNo
        )
    {
        var testCase = m_runSettings.GetTestCase(testNo);
        Assert.IsNotNull( testCase.MessageFileName );

        using var signCert = m_runSettings.GetTestCert(testCase.SignCertNo, imported: true);

        // Osigurava isto ponašanje testirane biblioteke kako radi AKDSH Signer, tj. za sve RSA certifikate koristi
        // SHA-1.
        var options = new Pkcs7Options
        {
            DefaultDigestAlgorithms = new()
            {
                RsaCsp = Oid.FromFriendlyName("sha1", OidGroup.HashAlgorithm),
                RsaKsp = Oid.FromFriendlyName("sha1", OidGroup.HashAlgorithm)
            }
        };

        // Čita otvorenu testnu datoteku i potpisuje je zadanim certifikatom pomoću testirane biblioteke. Možda zatraži
        // unos PIN-a ili prikaže drugo korisničko sučelje.
        var actualData = Pkcs7.SignData(
              File.ReadAllBytes(m_runSettings.GetDeployedFilePath(testCase.ContentFileName))
            , signCert.Cert!
            , testCase.SignDateTime
            , options
            );
        var actualContentType = ContentInfo.GetContentType( actualData );
        Print( 0, "Actual content type", null, actualContentType );
        Assert.AreEqual( Pkcs7.Oids.SignedData.Value, actualContentType.Value );

        // Dekodira upravo potpisani sadržaj.
        var actualCms = new SignedCms();
        actualCms.Decode( actualData );
        Print( 0, "Actual CMS", null, actualCms );

        // Čita testnu datoteku potpisanu od AKDSH Signera.
        var expectedData = File.ReadAllBytes( m_runSettings.GetDeployedFilePath( testCase.MessageFileName ) );
        var expectedContentType = ContentInfo.GetContentType( actualData );
        Print( 0, "Expected content type", null, expectedContentType );

        // Uspoređuje tipove sadržaja.
        Assert.AreEqual( expectedContentType.Value, actualContentType.Value );

        // Dekodira testnu datoteku potpisanu od AKDSH Signera.
        var expectedCms = new SignedCms();
        expectedCms.Decode( expectedData );
        Print( 0, "Expected CMS", null, expectedCms );

        // Ispisuje obadva sadržaja.
        Console.WriteLine( "Expected HEX = " + Convert.ToHexString( expectedData ) );
        Console.WriteLine( "Actual HEX   = " + Convert.ToHexString( actualData ) );

        // Uspoređuje sadržaje.
        AssertEqual( expectedCms, actualCms );
    }

    /// <summary>
    /// Rekonstruira datoteku potpisanu i šifriranu od AKDSH Signera i uspoređuje im sadržaje.
    /// </summary>
    ///
    [TestMethod]
    [DataRow( RunSettings.TESTCASE_SOFT_BLUE )]
    public void A02_ReCreateAkdshSignerSample_VerifyItIsEqual(
          int testNo
        )
    {
        var testCase = m_runSettings.GetTestCase(testNo);
        Assert.IsNotNull( testCase.MessageFileName );

        using var signCert = m_runSettings.GetTestCert(testCase.SignCertNo, imported: true);

        Assert.IsNotNull( testCase.CryptCertNo );
        using var cryptCert = m_runSettings.GetTestCert(testCase.CryptCertNo.Value, imported: true);

        // Osigurava isto ponašanje testirane biblioteke kako radi AKDSH Signer, tj. za sve RSA certifikate koristi
        // SHA-1.
        var options = new Pkcs7Options
        {
            DefaultDigestAlgorithms = new()
            {
                RsaCsp = Oid.FromFriendlyName("sha1", OidGroup.HashAlgorithm),
                RsaKsp = Oid.FromFriendlyName("sha1", OidGroup.HashAlgorithm)
            }
        };

        // Čita otvorenu testnu datoteku i potpisuje je zadanim certifikatom pomoću testirane biblioteke. Možda zatraži
        // unos PIN-a ili prikaže drugo korisničko sučelje.
        var signedData = Pkcs7.SignData(
              File.ReadAllBytes(m_runSettings.GetDeployedFilePath(testCase.ContentFileName))
            , signCert.Cert!
            , testCase.SignDateTime
            , options
            );

        // Šifrira potpisani podatak.
        Assert.IsNotNull( testCase.CryptAlg );
        var actualData = Pkcs7.EnvelopeData(
              signedData
            , cryptCert.Cert!
            , Oid.FromFriendlyName(testCase.CryptAlg, OidGroup.EncryptionAlgorithm)
            , options
            );
        var actualContentType = ContentInfo.GetContentType( actualData );
        Print( 0, "Actual content type", null, actualContentType );
        Assert.AreEqual( Pkcs7.Oids.EnvelopedData.Value, actualContentType.Value );

        // Dekodira upravo šifrirani podatak.
        var actualCms = new EnvelopedCms();
        actualCms.Decode( actualData );
        Print( 0, "Actual CMS", null, actualCms );

        // Čita testnu datoteku potpisanu od AKDSH Signera.
        var expectedData = File.ReadAllBytes( m_runSettings.GetDeployedFilePath( testCase.MessageFileName ) );
        var expectedContentType = ContentInfo.GetContentType( actualData );
        Print( 0, "Expected content type", null, expectedContentType );

        // Uspoređuje tipove sadržaja.
        Assert.AreEqual( expectedContentType.Value, actualContentType.Value );

        // Dekodira testnu datoteku potpisanu od AKDSH Signera.
        var expectedCms = new EnvelopedCms();
        expectedCms.Decode( expectedData );
        Print( 0, "Expected CMS", null, expectedCms );

        // Ispisuje obadva sadržaja.
        Console.WriteLine( "Expected HEX = " + Convert.ToHexString( expectedData ) );
        Console.WriteLine( "Actual HEX   = " + Convert.ToHexString( actualData ) );

        // Uspoređuje sadržaje.
        AssertEqual( expectedCms, actualCms );

        // Dešifrira obadva podatka.
        expectedCms.Decrypt();
        actualCms.Decrypt();

        // Uspoređivanje unutarnjih podataka.
        static void AssertInnerContentEqual( byte[] expectedInnerData, byte[] actualInnerData )
        {
            // Sadržani moraju biti potpisani podaci.
            Assert.AreEqual( Pkcs7.Oids.SignedData.Value, Pkcs7.GetContentTypeOid( expectedInnerData ).Value );
            Assert.AreEqual( Pkcs7.Oids.SignedData.Value, Pkcs7.GetContentTypeOid( actualInnerData ).Value );

            // Dekodira sadržane podatke.
            var expectedCms = new SignedCms();
            expectedCms.Decode( expectedInnerData );
            var actualCms = new SignedCms();
            actualCms.Decode( actualInnerData );

            // Uspoređuje sadržaje.
            AssertEqual( expectedCms, actualCms );
        }

        // Uspoređuje unutarnje podatke.
        AssertInnerContentEqual( expectedCms.ContentInfo.Content, actualCms.ContentInfo.Content );
    }
}

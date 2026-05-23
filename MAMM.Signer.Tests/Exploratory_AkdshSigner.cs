using System.Security.Cryptography.Pkcs;

namespace MAMM.Signer.Tests;

/// <summary>
/// Istraživanje sadržaja datoteka koje su kreirane AKDSH Signerom. Isključiti ove testove kroz SuppressExploratoryTests
/// u .runsettings datoteci.
/// </summary>
///
/// <remarks>
/// <para>
///     Ove metode ispišu sadržaj testnih datoteka opisanih u <see href="TestCase.md">TestCase.md</see>. U <see
///     cref="Exploratory_Pkcs7"/> postoje testovi ekvivalentni ovima, ali koji izrade iste takve testne datoteke kroz
///     .NET biblioteku i ispišu taj sadržaj, pa se sadržaji mogu pregledom usporediti i tako utvrditi kako kroz .NET
///     biblioteku producirati isti sadržaj kakav producira AKDSH Signer.
///     </para>
/// </remarks>
///
[TestClass]
public class Exploratory_AkdshSigner : CmsTestBase
{
    private static RunSettings m_runSettings = new();
    private static int m_oci = 0;

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.AreNotEqual( 1, m_oci++ );
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Prikaže sadržaj datoteka potpisanih pomoću HZZO Signera (koristi <see cref="SignedCms"/> za čitanje).
    /// Usporediti ispis ovog testa s ispisom testa <see cref="Exploratory_Pkcs7.SignTestData(int)"/>.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCASE_SOFT_NONE )]
    [DataRow( RunSettings.TESTCASE_BLUE_NONE )]
    public void ExploreSignedFile( int testNo )
    {
        m_runSettings.CancelIfExploratorySuppressed();

        var testCase = m_runSettings.GetTestCase(testNo);
        Assert.IsNotNull( testCase.MessageFileName );

        var encodedData = File.ReadAllBytes( m_runSettings.GetDeployedFilePath( testCase.MessageFileName ) );
        Print( 0, "Outer content type", null, ContentInfo.GetContentType( encodedData ) );

        var cms = new SignedCms();
        cms.Decode( encodedData );
        Print( 0, "Signed CMS", null, cms );
    }

    /// <summary>
    /// Prikaže sadržaj datoteka potpisanih i šifriranih pomoću HZZO Signera (koristi <see cref="EnvelopedCms"/> i <see
    /// cref="SignedCms"/> za čitanje). Usporediti ispis ovog testa s ispisom testa <see
    /// cref="Exploratory_Pkcs7.EnvelopeTestData(int)"/>.
    /// <remarks>
    /// <para>
    ///     Napomena: da bi se ovaj test mogao izvršiti u cijelosti, potrebno je testne datoteke izraditi vlastitim
    ///     karticama, odn. certifikatima, za koje su raspoloživi privatni ključevi.</para>
    /// </remarks>
    [TestMethod]
    [DataRow( RunSettings.TESTCASE_SOFT_BLUE )]
    public void ExploreSignedAndEncryptedFile( int testNo )
    {
        m_runSettings.CancelIfExploratorySuppressed();

        var testCase = m_runSettings.GetTestCase(testNo);
        Assert.IsNotNull( testCase.MessageFileName );

        Assert.IsNotNull( testCase.CryptCertNo );
        using var testCert = m_runSettings.GetTestCert(testCase.CryptCertNo.Value, imported: true);

        var encodedOuterData = File.ReadAllBytes( m_runSettings.GetDeployedFilePath( testCase.MessageFileName ) );
        Print( 0, "Outer content type", null, ContentInfo.GetContentType( encodedOuterData ) );

        var envelopedCms = new EnvelopedCms();
        envelopedCms.Decode( encodedOuterData );
        Print( 0, "Enveloped CMS", null, envelopedCms );

        // Moguće pita PIN, a certifikat kojim je izvršeno šifriranje mora biti dostupan.
        envelopedCms.Decrypt();

        Print( 0, "Inner content type", null, ContentInfo.GetContentType( envelopedCms.ContentInfo.Content ) );

        var signedCms = new SignedCms();
        signedCms.Decode( envelopedCms.ContentInfo.Content );
        Print( 0, "Signed CMS", null, signedCms );
    }
}

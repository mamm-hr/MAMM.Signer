using MAMM.Signer.Pkcs;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see cref="Pkcs7.VerifySignedData(byte[], Pkcs7Options?)"/>.
/// </summary>
[TestClass]
public class Tests_Pkcs7_VerifyData : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Ovjeri potpis uz podrazumijevano povjerenje u certifikate u lancu povjerenja.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A01_Signed_VerifyData_WithTrust(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var options = new Pkcs7Options() { TrustCertificates = true };
        CollectionAssert.AreEqual( content, Pkcs7.VerifySignedData( Pkcs7.SignData( content, signer.Cert, DateTimeOffset.UtcNow ), options ) );
    }
}

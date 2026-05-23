using MAMM.Signer.Pkcs;
using System.Security.Cryptography;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see cref="Pkcs7.SignData(byte[], System.Security.Cryptography.X509Certificates.X509Certificate2,
/// DateTimeOffset, Pkcs7Options?)"/>.
/// </summary>
[TestClass]
public class Tests_Pkcs7_SignData : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Potpiše poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A01_Plain_SignData(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        CollectionAssert.AreEqual( content, ReadSignedMessage( Pkcs7.SignData( content, signer.Cert, DateTimeOffset.UtcNow ) ) );
    }
}

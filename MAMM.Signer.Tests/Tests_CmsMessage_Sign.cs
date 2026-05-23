using MAMM.Signer.Pkcs;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see cref="CmsMessage.Sign(System.Security.Cryptography.X509Certificates.X509Certificate2,
/// DateTimeOffset)"/>.
/// </summary>
[TestClass]
public class Tests_CmsMessage_Sign : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Potpiše nepotpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A01_Plain_Sign(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false);
        subject.Sign( signer.Cert );
        CollectionAssert.AreEqual( content, ReadSignedMessage( subject.Encode() ) );
    }

    /// <summary>
    /// Potpiše potpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A02_Signed_Sign(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var subject = new CmsMessage(SignMessage(CreateMessage(), signer.Cert), isReceived: true);
        Assert.ThrowsExactly<InvalidMessageStateException>( () => subject.Sign( signer.Cert ) );
    }

    /// <summary>
    /// Potpiše poruku dva puta.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A03_Plain_Sign_Sign(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false);
        subject.Sign( signer.Cert );
        Assert.ThrowsExactly<InvalidMessageStateException>( () => subject.Sign( signer.Cert ) );
    }
}

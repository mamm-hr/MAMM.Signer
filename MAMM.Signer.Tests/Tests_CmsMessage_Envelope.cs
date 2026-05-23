using MAMM.Signer.Pkcs;
using System.Security.Cryptography;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see cref="CmsMessage.Envelope(System.Security.Cryptography.X509Certificates.X509Certificate2,
/// Oid?)"/>.
/// </summary>
[TestClass]
public class Tests_CmsMessage_Envelope : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Kuvertira nepotpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A01_Plain_Envelope(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false);
        subject.Envelope( recipient.Cert );
        CollectionAssert.AreEqual( content, ReadEnvelopedMessage( subject.Encode(), recipient.Cert ) );
    }

    /// <summary>
    /// Kuvertira potpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_RSA )]
    public void A02_Signed_Envelope(int recipientNo, int signerNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true);
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(SignMessage(content, signer.Cert), isReceived: true);
        subject.Envelope( recipient.Cert );
        CollectionAssert.AreEqual( content, ReadSignedMessage(ReadEnvelopedMessage( subject.Encode(), recipient.Cert )) );
    }

    /// <summary>
    /// Potpiše i kuvertira nepotpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_RSA )]
    public void A03_Plain_Sign_Envelope(int recipientNo, int signerNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true);
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false);
        subject.Sign( signer.Cert );
        subject.Envelope( recipient.Cert );
        CollectionAssert.AreEqual( content, ReadSignedMessage( ReadEnvelopedMessage( subject.Encode(), recipient.Cert ) ) );
    }

    /// <summary>
    /// Kuvertiranu poruku se ne može kuvertirati opet.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A04_Enveloped_Envelope(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        var content = CreateMessage();
        var cmsMessage = new CmsMessage(EnvelopeMessage(content, recipient.Cert), isReceived: true);
        Assert.ThrowsExactly<InvalidMessageStateException>( () => cmsMessage.Envelope( recipient.Cert ) );
    }

    /// <summary>
    /// Poruku se ne može kuvertirati dva puta.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A05_Plain_Envelope_Envelope(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        var content = CreateMessage();
        var cmsMessage = new CmsMessage(content, isReceived: false);
        cmsMessage.Envelope( recipient.Cert );
        Assert.ThrowsExactly<InvalidMessageStateException>( () => cmsMessage.Envelope( recipient.Cert ) );
    }
}

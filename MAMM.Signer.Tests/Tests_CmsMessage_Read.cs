using MAMM.Signer.Pkcs;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira konstruktor <see cref="CmsMessage.Read"/> objekta.
/// </summary>
[TestClass]
public class Tests_CmsMessage_Read : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Čita sadržaj poruke kojim je objekt inicijaliziran.
    /// </summary>
    [TestMethod]
    public void A01_Plain_Read()
    {
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false);
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Čita sadržaj potpisane poruke.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A02_Signed_Read(int signerNo)
    {
        using var signer =  m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(SignMessage(content, signer.Cert), isReceived: true);
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Otvara i čita sadržaj kuvertirane nepotpisane poruke.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A03_Enveloped_Read(int recipientNo)
    {
        using var recipient =  m_runSettings.GetTestCert(recipientNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(EnvelopeMessage(content, recipient.Cert), isReceived: true);
        subject.OpenEnvelope();
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Otvara i čita sadražaj potpisane kuvertirane poruke.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_RSA )]
    public void A04_EnvelopedSigned_Read(int recipientNo, int signerNo)
    {
        using var recipient =  m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        using var signer =  m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(EnvelopeMessage(SignMessage(content, signer.Cert), recipient.Cert), isReceived: true);
        subject.OpenEnvelope();
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Potpiše pa čita sadržaj poruke.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A05_Plain_Sign_Read(int signerNo)
    {
        using var signer =  m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false);
        subject.Sign(signer.Cert);
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Kuvertira pa čita sadržaj poruke.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A06_Plain_Envelope_Read(int recipientNo)
    {
        using var recipient =  m_runSettings.GetTestCert(recipientNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false);
        subject.Envelope(recipient.Cert);
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Potpiše i kuvertira pa čita sadržaj poruke.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_RSA )]
    public void A07_Plain_Sign_Envelope_Read(int recipientNo, int signerNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false);
        subject.Sign(signer.Cert);
        subject.Envelope(recipient.Cert);
        Assert.ThrowsExactly<InvalidMessageStateException>( () => subject.OpenEnvelope() );
    }
}

using MAMM.Signer.Pkcs;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see
/// cref="CmsMessage.OpenEnvelope(System.Security.Cryptography.X509Certificates.X509Certificate2?)"/>.
/// </summary>
[TestClass]
public class Tests_CmsMessage_OpenEnvelope : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Otvori kuvertiranu nepotpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A01_Enveloped_OpenEnvelope(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        var content = CreateMessage();
        var subject = new CmsMessage(EnvelopeMessage(content, recipient.Cert), isReceived: true);
        subject.OpenEnvelope();
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Otvori kuvertiranu potpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_RSA )]
    public void A02_EnvelopedSigned_OpenEnvelope(int recipientNo, int signerNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(EnvelopeMessage(SignMessage(content, signer.Cert), recipient.Cert), isReceived: true);
        subject.OpenEnvelope();
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Primatelj otvori kuvertiranu nepotpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A03_Enveloped_OpenEnvelopeForRecipient(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        var content = CreateMessage();
        var subject = new CmsMessage(EnvelopeMessage(content, recipient.Cert), isReceived: true);
        subject.OpenEnvelope( recipient.Cert );
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Primatelj otvori kuvertiranu potpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_RSA )]
    public void A04_EnvelopedSigned_OpenEvnelopeForRecipient(int recipientNo, int signerNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(EnvelopeMessage(SignMessage(content, signer.Cert), recipient.Cert), isReceived: true);
        subject.OpenEnvelope(recipient.Cert);
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Krivi primatelj otvori kuvertiranu nepotpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A05_Enveloped_OpenEnvelopeForUnknownRecipient(int recipientNo, int unknownNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        var content = CreateMessage();
        var subject = new CmsMessage(EnvelopeMessage(content, recipient.Cert), isReceived: true);
        using var unknown = m_runSettings.GetTestCert(unknownNo, imported: true, ignoreShowsUI: true);
        Assert.ThrowsExactly<UnknownRecipientException>( () => subject.OpenEnvelope( unknown.Cert ) );
    }

    /// <summary>
    /// Kuvertira i potom otvori nepotpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A06_Plain_Envelope_OpenEnvelope(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false);
        subject.Envelope( recipient.Cert );
        Assert.ThrowsExactly<InvalidMessageStateException>( () => subject.OpenEnvelope() );
    }
}

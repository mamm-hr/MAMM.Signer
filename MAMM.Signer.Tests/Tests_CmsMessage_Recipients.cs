using MAMM.Signer.Pkcs;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira svojstvo <see cref="CmsMessage.Recipients"/>.
/// </summary>
[TestClass]
public class Tests_CmsMessage_Recipients : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Objekt inicijaliziran sadržajem poruke nema primatelja.
    /// </summary>
    [TestMethod]
    public void A01_Plain_NoRecipients()
    {
        var subject = new CmsMessage(CreateMessage(), isReceived: false, null);
        Assert.IsEmpty( subject.Recipients );
    }

    /// <summary>
    /// Objekt inicijaliziran tekstom potpisane poruke nema primatelja.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A02_Signed_HasRecipients(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var subject = new CmsMessage(SignMessage(CreateMessage(), signer.Cert), isReceived: true);
        Assert.IsEmpty( subject.Recipients );
    }

    /// <summary>
    /// Objekt inicijaliziran tekstom potpisane poruke koju se kuvertira ima primatelja.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_RSA )]
    public void A03_Signed_Enveloped_HasRecipients(int recipientNo, int signerNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var subject = new CmsMessage(SignMessage(CreateMessage(), signer.Cert), isReceived: true);
        subject.Envelope( recipient.Cert );
        Assert.HasCount( 1, subject.Recipients );
        Assert.AreEqual( recipient.Cert.Issuer, subject.Recipients[0].IssuerName );
        Assert.AreEqual( recipient.Cert.SerialNumber, subject.Recipients[0].SerialNumber );
    }

    /// <summary>
    /// Objekt inicijaliziran tekstom nepotpisane kuvertirane poruke ima primatelja.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A04_Enveloped_HasRecipients(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        var subject = new CmsMessage(EnvelopeMessage(CreateMessage(), recipient.Cert), isReceived: true);
        Assert.HasCount( 1, subject.Recipients );
        Assert.AreEqual( recipient.Cert.Issuer, subject.Recipients[0].IssuerName );
        Assert.AreEqual( recipient.Cert.SerialNumber, subject.Recipients[0].SerialNumber );
    }

    /// <summary>
    /// Objekt inicijaliziranom tekstom potpisane kuvertirane poruke ima primatelja.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_RSA )]
    public void A05_EnvelopedSigned_HasRecipients(int recipientNo, int signerNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var subject = new CmsMessage(EnvelopeMessage(SignMessage(CreateMessage(), signer.Cert), recipient.Cert), isReceived: true);
        Assert.HasCount( 1, subject.Recipients );
        Assert.AreEqual( recipient.Cert.Issuer, subject.Recipients[0].IssuerName );
        Assert.AreEqual( recipient.Cert.SerialNumber, subject.Recipients[0].SerialNumber );
    }

    /// <summary>
    /// Objekt inicijaliziran sadržajem poruke po kuvertiranju ima primatelja.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A06_Plain_Envelope_HasRecipients(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        var subject = new CmsMessage(CreateMessage(), isReceived: false);
        subject.Envelope( recipient.Cert );
        Assert.HasCount( 1, subject.Recipients );
        Assert.AreEqual( recipient.Cert.Issuer, subject.Recipients[0].IssuerName );
        Assert.AreEqual( recipient.Cert.SerialNumber, subject.Recipients[0].SerialNumber );
    }

    /// <summary>
    /// Objekt inicijaliziran sadržajem poruke po potpisivanju nema primatelja.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A07_Plain_Sign_NoRecipients(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var subject = new CmsMessage(CreateMessage(), isReceived: false);
        subject.Sign( signer.Cert );
        Assert.IsEmpty( subject.Recipients );
    }

    /// <summary>
    /// Objekt inicijaliziran sadržajem poruke po potpisivanju i kuvertiranju ima potpisnika.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_RSA )]
    public void A0_Plain_Sign_Envelope_HasSigners(int recipientNo, int signerNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var subject = new CmsMessage(CreateMessage(), isReceived: false);
        subject.Sign( signer.Cert );
        subject.Envelope( recipient.Cert );
        Assert.HasCount( 1, subject.Recipients );
        Assert.AreEqual( recipient.Cert.Issuer, subject.Recipients[0].IssuerName );
        Assert.AreEqual( recipient.Cert.SerialNumber, subject.Recipients[0].SerialNumber );
    }
}

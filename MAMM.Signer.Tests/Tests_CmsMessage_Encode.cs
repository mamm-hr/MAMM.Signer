using MAMM.Signer.Pkcs;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodue <see cref="CmsMessage.Encode()"/>.
/// </summary>
[TestClass]
public class Tests_CmsMessage_Encode : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Po inicijalizaciji sadržajem poruke.
    /// </summary>
    [TestMethod]
    public void A01_Plain_Encode()
    {
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false);
        Assert.ThrowsExactly<InvalidMessageStateException>( () => subject.Encode() );
    }

    /// <summary>
    /// Po inicijalizaciji tekstom potpisane poruke.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A02_Signed_Encode(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var text = SignMessage(CreateMessage(), signer.Cert);
        var subject = new CmsMessage(text, isReceived: true);
        CollectionAssert.AreEqual( text, subject.Encode() );
    }

    /// <summary>
    /// Po inicijalizaciji tekstom kuvertirane poruke.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A03_Enveloped_Encode(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        var text = EnvelopeMessage(CreateMessage(), recipient.Cert);
        var subject = new CmsMessage(text, isReceived: true);
        Assert.ThrowsExactly<InvalidMessageStateException>( () => subject.Encode() );
    }
}

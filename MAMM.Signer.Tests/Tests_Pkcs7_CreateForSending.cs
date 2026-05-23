using MAMM.Signer.Pkcs;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see cref="Pkcs7.CreateForSending(byte[], Pkcs7Options?)"/>.
/// </summary>
[TestClass]
public class Tests_Pkcs7_CreateForSending : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Vraća isti sadržaj kojim je objekt konstruiran.
    /// </summary>
    [TestMethod]
    public void A01_Plain_CreateForSending()
    {
        var content = CreateMessage();
        var subject = Pkcs7.CreateForSending(content);
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Vraća isti sadržaj kojim je objekt konstruiran.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A02_Signed_CreateForSending(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var text = SignMessage(CreateMessage(), signer.Cert);
        var subject = Pkcs7.CreateForSending(text);
        CollectionAssert.AreEqual( text, subject.Read() );
    }

    /// <summary>
    /// Po inicijalizaciji tekstom kuvertirane poruke.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A03_Enveloped_CreateForSending(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        var text = EnvelopeMessage(CreateMessage(), recipient.Cert);
        var subject = Pkcs7.CreateForSending(text);
        CollectionAssert.AreEqual( text, subject.Read() );
    }
}

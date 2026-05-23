using MAMM.Signer.Pkcs;
using System.Security.Cryptography;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see cref="Pkcs7.CreateFromReceived(byte[], Pkcs7Options?)"/>.
/// </summary>
[TestClass]
public class Tests_Pkcs7_CreateFromReceived : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Mora se inicijalizirati sintaktički ispravnim tekstom.
    /// </summary>
    [TestMethod]
    public void A01_Plain_CreateFromReceived()
    {
        var content = CreateMessage();
        Assert.ThrowsExactly<UnsupportedCmsContentTypeException>( () => Pkcs7.CreateFromReceived( content ) );
    }

    /// <summary>
    /// Tekst potpisane poruke čita isti sadržaj.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A02_Signed_CreateFromReceived(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = Pkcs7.CreateFromReceived(SignMessage(content, signer.Cert));
        CollectionAssert.AreEqual( content, subject.Read() );
    }

    /// <summary>
    /// Sadržaj kuvertirane poruke je nedostupan prije otvaranja kuverte.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A03_Enveloped_CreateFromReceived(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        var content = CreateMessage();
        var subject = Pkcs7.CreateFromReceived(EnvelopeMessage(content, recipient.Cert));
        Assert.ThrowsExactly<InvalidMessageStateException>( () => subject.Read() );
    }
}

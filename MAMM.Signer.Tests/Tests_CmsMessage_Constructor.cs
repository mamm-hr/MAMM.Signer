using MAMM.Signer.Pkcs;
using System.Security.Cryptography;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira konstruktor <see cref="CmsMessage"/> objekta.
/// </summary>
[TestClass]
public class Tests_CmsMessage_Constructor : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Otvorena poruka ne može biti primljena jer nije u CMS sintaksi.
    /// </summary>
    [TestMethod]
    public void A01_Plain_CannotBeReceived()
    {
        Assert.ThrowsExactly<UnsupportedCmsContentTypeException>( () => _ = new CmsMessage( CreateMessage(), isReceived: true ) );
    }

    /// <summary>
    /// Potpisana poruka može biti primljena.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A02_Signed_CanBeReceived(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        _ = new CmsMessage( SignMessage( CreateMessage(), signer.Cert ), isReceived: true );
    }

    /// <summary>
    /// Kuvertirana poruka može biti primljena.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A03_Enveloped_CanBeReceived(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        _ = new CmsMessage(EnvelopeMessage(CreateMessage(), recipient.Cert), isReceived: true);
    }
}

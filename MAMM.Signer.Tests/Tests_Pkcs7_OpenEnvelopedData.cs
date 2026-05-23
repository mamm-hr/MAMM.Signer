using MAMM.Signer.Pkcs;
using System.Security.Cryptography;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see cref="Pkcs7.OpenEnvelopedData(byte[], Pkcs7Options?)"/>.
/// </summary>
[TestClass]
public class Tests_Pkcs7_OpenEnvelopedData : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Otvori sintaktički neispravnu poruku.
    /// </summary>
    [TestMethod]
    public void A01_Plain_OpenEnvelopedData()
    {
        Assert.ThrowsExactly<UnsupportedCmsContentTypeException>( () => Pkcs7.OpenEnvelopedData( CreateMessage() ) );
    }

    /// <summary>
    /// Otvori potpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A02_Signed_OpenEnvelopedData(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        Assert.ThrowsExactly<InvalidMessageStateException>( () => Pkcs7.OpenEnvelopedData( SignMessage(CreateMessage(), signer.Cert) ) );
    }

    /// <summary>
    /// Otvori kuvertiranu nepotpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A03_PlainEnveloped_OpenEnvelopedData(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true);
        var content = CreateMessage();
        CollectionAssert.AreEqual( content, Pkcs7.OpenEnvelopedData( EnvelopeMessage( content, recipient.Cert ) ) );
    }

    /// <summary>
    /// Otvori kuvertiranu potpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_RSA )]
    public void A04_SignedEnveloped_OpenEnvelopedData(int recipientNo, int signerNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true);
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        CollectionAssert.AreEqual( content, Pkcs7.OpenEnvelopedData( EnvelopeMessage( SignMessage( content, signer.Cert ), recipient.Cert ) ) );
    }
}

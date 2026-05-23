using MAMM.Signer.Pkcs;
using System.Security.Cryptography;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see cref="Pkcs7.EnvelopeData(byte[], System.Security.Cryptography.X509Certificates.X509Certificate2,
/// Oid?, Pkcs7Options?)"/>.
/// </summary>
[TestClass]
public class Tests_Pkcs7_EnvelopeData : CmsTestBase
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
    public void A01_Plain_EnvelopeData(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true);
        var content = CreateMessage();
        var text = Pkcs7.EnvelopeData(content, recipient.Cert);
        CollectionAssert.AreEqual( content, ReadEnvelopedMessage( text, recipient.Cert ) );
    }

    /// <summary>
    /// Kuvertira potpisanu poruku.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_RSA, RunSettings.TESTCERT_RSA )]
    public void A02_Signed_EnvelopeData(int recipientNo, int signerNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true);
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var text = Pkcs7.EnvelopeData(SignMessage(content, signer.Cert), recipient.Cert);
        CollectionAssert.AreEqual( content, ReadSignedMessage(ReadEnvelopedMessage( text, recipient.Cert )) );
    }
}

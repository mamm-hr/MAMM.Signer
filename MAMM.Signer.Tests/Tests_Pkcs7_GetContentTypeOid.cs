using MAMM.Signer.Pkcs;
using System.Security.Cryptography;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see cref="Pkcs7.GetContentTypeOid(byte[])"/>.
/// </summary>
[TestClass]
public class Tests_Pkcs7_GetContentTypeOid : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Za <see langword="null"/> baca <see cref="ArgumentNullException"/>.
    /// </summary>
    ///
    [TestMethod]
    public void A01_Null_GetContentTypeOid()
    {
        Assert.Throws<ArgumentNullException>( () => Pkcs7.GetContentTypeOid( null! ) );
    }

    /// <summary>
    /// Za prazni niz baca <see cref="CryptographicException"/>.
    /// </summary>
    ///
    [TestMethod]
    public void A02_Empty_GetContentTypeOid()
    {
        Assert.Throws<CryptographicException>( () => Pkcs7.GetContentTypeOid( [] ) );
    }

    /// <summary>
    /// Sintaktički neispravan tekst.
    /// </summary>
    [TestMethod]
    public void A03_Plain_GetContentTypeOid()
    {
        Assert.AreEqual( Pkcs7.Oids.Data.Value, Pkcs7.GetContentTypeOid( CreateMessage() ).Value );
    }

    /// <summary>
    /// Potpisana poruka.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A04_Signed_GetContentTypeOid(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        Assert.AreEqual( Pkcs7.Oids.SignedData.Value, Pkcs7.GetContentTypeOid( SignMessage( CreateMessage(), signer.Cert ) ).Value );
    }

    /// <summary>
    /// Kuvertirana nepotpisana poruka.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    public void A05_PlainEnveloped_GetContentTypeOid(int recipientNo)
    {
        using var recipient = m_runSettings.GetTestCert(recipientNo, imported: true, ignoreShowsUI: true);
        Assert.AreEqual( Pkcs7.Oids.EnvelopedData.Value, Pkcs7.GetContentTypeOid( EnvelopeMessage( CreateMessage(), recipient.Cert ) ).Value );
    }
}

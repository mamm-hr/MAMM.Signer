using MAMM.Signer.Pkcs;
using System.Security.Cryptography;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see cref="CmsMessage.Verify"/>.
/// </summary>
/// <remarks>
/// <para>
///     Ovi testovi ne testiraju kompletnu ovjeru koja uključuje provjeru valjanosti certifikata i provjeru povjerenja u
///     korijenski certifikat u lancu povjerenja zato što ta provjera zahtjeva mogućnost provjere opoziva certifikata
///     koja za testne certifikate nije moguća.</para>
/// </remarks>
[TestClass]
public class Tests_CmsMessage_Verify : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Potpiše poruku pa ovjeri potpis uz provjeru valjnosti cerifikata u lancu.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A01_Plain_Sign_Verify(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false);
        subject.Sign( signer.Cert );
        // Morao bi baciti iznimku "Certificate trust could not be established. The first reported error is: The
        // revocation function was unable to check revocation for the certificate." zbog toga što testni certikati ne
        // uključuju informacije o provjeri opoziva certifikata (CRL - certificate revocation list).
        Assert.ThrowsExactly<CryptographicException>( () => subject.Verify() );
    }

    /// <summary>
    /// Potpiše poruku pa ovjeri potpis uz podrazumijevano povjerenje u certifikate.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A02_Plain_Sign_Verify_WithTrust(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var options = new Pkcs7Options() { TrustCertificates = true };
        var content = CreateMessage();
        var subject = new CmsMessage(content, isReceived: false, options);
        subject.Sign( signer.Cert );
        subject.Verify();
    }

    /// <summary>
    /// Ovjeri potpis uz provjeru valjnosti cerifikata u lancu.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A03_Signed_Verify(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var subject = new CmsMessage(SignMessage(CreateMessage(), signer.Cert), isReceived: true);
        // Morao bi baciti iznimku "Certificate trust could not be established. The first reported error is: The
        // revocation function was unable to check revocation for the certificate." zbog toga što testni certikati ne
        // uključuju informacije o provjeri opoziva certifikata (CRL - certificate revocation list).
        Assert.ThrowsExactly<CryptographicException>( () => subject.Verify() );
}

    /// <summary>
    /// Ovjeri potpis uz podrazumijevano povjerenje u certifikate.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A04_Signed_Verify_WithTrust(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var options = new Pkcs7Options() { TrustCertificates = true };
        var subject = new CmsMessage(SignMessage(CreateMessage(), signer.Cert), isReceived: true, options);
        // Morao bi baciti iznimku "Certificate trust could not be established. The first reported error is: The
        // revocation function was unable to check revocation for the certificate." zbog toga što testni certikati ne
        // uključuju informacije o provjeri opoziva certifikata (CRL - certificate revocation list).
        subject.Verify();
    }
}

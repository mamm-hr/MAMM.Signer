namespace MAMM.Signer.Tests;

/// <summary>
/// Uvaža u odnosno briše generirane testne certifikate iz korsnikovog spremišta. Ovo nisu testovi, već samo metode za
/// uvažanje/brisanje certifikata priloženih u .pfx datotekama.
/// </summary>
[TestClass]
public class TestCert_Setup
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    /// <summary>
    /// Uvaža generirane testne certifikate u korisnikovo spremište.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_ROOT )] // Neće se izvršiti ako su u .runsettings isključeni testovi koji prikazuju korisničko sučelje.
    [DataRow( RunSettings.TESTCERT_CA )]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_ECDSA_SIGN )]
    public void A01_Import(int certNo)
    {
        if( m_runSettings.SuppressTestCertSetup )
            Assert.Inconclusive( "Testovi u ovom razredu su isključeni kroz .runsettings." );
        m_runSettings.GetTestCert(certNo, imported: true);
    }

    /// <summary>
    /// Briše generirane testne certifikate iz korisnikovog spremišta.
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_ROOT )] // Neće se izvršiti ako su u .runsettings isključeni testovi koji prikazuju korisničko sučelje.
    [DataRow( RunSettings.TESTCERT_CA )]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    [DataRow( RunSettings.TESTCERT_ECDSA_SIGN )]
    public void A02_Remove(int certNo)
    {
        if( m_runSettings.SuppressTestCertSetup )
            Assert.Inconclusive( "Testovi u ovom razredu su isključeni kroz .runsettings." );
        m_runSettings.RemoveTestCertFromStore( certNo );
    }
}

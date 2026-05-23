using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Tests;

internal class RunSettings( TestContext context )
{
    // V. TestCert.md za detaljnije pojašnjenje ovih konstanti!
    public const int TESTCERT_SOFT        = 1; // Soft certifikat izdan od HZZO-a za testiranje.
    public const int TESTCERT_BLUE        = 2; // Plava kartica izdana od HZZO-a za testiranje.
    public const int TESTCERT_WHITE       = 3; // Bijela kartica izdana od HZZO-a za testiranje.
    public const int TESTCERT_ROOT        = 4; // Generirani korijenski CA certifikat.
    public const int TESTCERT_CA          = 5; // Generirani subordinirani CA certifikat.
    public const int TESTCERT_RSA         = 6; // Generirani RSA certifikat.
    public const int TESTCERT_ECDSA_IDENT = 7; // Generirani ECDSA identifikacijski certifikat.
    public const int TESTCERT_ECDSA_SIGN  = 8; // Generirani ECDSA potpisni certifikat.

    // V. HelloWorld.md za detaljnije pojašnjenje ovih konstanti!
    public const int TESTCASE_SOFT_NONE  = 1; // Poptisano soft certifikatom.
    public const int TESTCASE_BLUE_NONE  = 2; // Potpisano plavom karticom.
    public const int TESTCASE_SOFT_BLUE  = 3; // Potpisano soft certifikatom i šifrirano plavom karticom.
    public const int TESTCASE_WHITE_NONE = 4; // Potpisani bijelom karticom.

    private readonly TestContext? m_context = context;

    public RunSettings() : this( null! ) { }

    public string DeploymentDirectory
        => m_context?.DeploymentDirectory ?? throw new( "Nema podatka o direktoriju s ulaznim datotekama testova." );

    public string RunResultsDirectory
        => m_context?.TestRunResultsDirectory ?? throw new( "Nema podatka o direktoriju za izlazne datoteke testova." );

    public bool SuppressTestsShowingUI
        => bool.TryParse( m_context?.Properties[nameof( SuppressTestsShowingUI )] as string ?? false.ToString(), out var parsedValue )
        ? parsedValue
        : throw new( $"Provjeriti u .runsettings što je upisano u: {nameof( SuppressTestsShowingUI )}." );

    public bool SuppressExploratoryTests
        => bool.TryParse( m_context?.Properties[nameof( SuppressExploratoryTests )] as string ?? false.ToString(), out var parsedValue )
        ? parsedValue
        : throw new( $"Provjeriti u .runsettings što je upisano u: {nameof( SuppressExploratoryTests )}." );

    public bool SuppressTestCertSetup
        => bool.TryParse( m_context?.Properties[nameof( SuppressTestCertSetup )] as string ?? false.ToString(), out var parsedValue )
        ? parsedValue
        : throw new( $"Provjeriti u .runsettings što je upisano u: {nameof( SuppressTestCertSetup )}." );

    public bool SuppressTestsUsingCezihCerts
        => bool.TryParse( m_context?.Properties[nameof( SuppressTestsUsingCezihCerts )] as string ?? false.ToString(), out var parsedValue )
        ? parsedValue
        : throw new( $"Provjeriti u .runsettings što je upisano u: {nameof( SuppressTestsUsingCezihCerts )}." );

    public void CancelIfExploratorySuppressed()
    {
        if(this.SuppressExploratoryTests)
            Assert.Inconclusive( "Eksploracijski testovi su isključeni kroz .runsettings." );
    }

    public void CancelIfShowingUIWhenSuppressed( bool showsUI )
    {
        if(this.SuppressTestsShowingUI && showsUI)
            Assert.Inconclusive( "Testovi koji prikazuju UI su isključeni kroz .runsettings." );
    }

    public void CancelIfCezihWhenSuppressed( bool isCezihCert )
    {
        if(this.SuppressTestsUsingCezihCerts && isCezihCert)
            Assert.Inconclusive( "Testovi koji koriste certifikate CEZIH-a su isključeni kroz .runsettings." );
    }

    public string GetDeployedFilePath( string fileName )
        => Path.Combine( this.DeploymentDirectory, fileName );

    public TestCase GetTestCase( int testNo )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan( testNo, 0 );

        var signDateTimeKey = $"TestCase.{testNo}.{nameof(TestCase.SignDateTime)}";
        var signCertNoKey = $"TestCase.{testNo}.{nameof(TestCase.SignCertNo)}";
        var signAlgKey = $"TestCase.{testNo}.{nameof(TestCase.SignAlg)}";
        var cryptCertNoKey = $"TestCase.{testNo}.{nameof(TestCase.CryptCertNo)}";
        var cryptAlgKey = $"TestCase.{testNo}.{nameof(TestCase.CryptAlg)}";
        var contentFileNameKey = $"TestCase.{testNo}.{nameof(TestCase.ContentFileName)}";
        var messageFileNameKey = $"TestCase.{testNo}.{nameof(TestCase.MessageFileName)}";

        var testCase = new TestCase()
        {
            SignDateTime = (DateTimeOffset)GetParameter<DateTimeOffset>( signDateTimeKey, mustExist: true )!,
            SignCertNo = (int)GetParameter<int>( signCertNoKey, mustExist: true )!,
            SignAlg = (string)GetParameter<string>( signAlgKey, mustExist: true )!,
            CryptCertNo = (int?)GetParameter<int>( cryptCertNoKey, mustExist: false ),
            CryptAlg = (string?)GetParameter<string>(cryptAlgKey, mustExist: false ),
            ContentFileName = (string)GetParameter<string>(contentFileNameKey, mustExist: true )!,
            MessageFileName = (string?)GetParameter<string>(messageFileNameKey, mustExist: false ),
        };

        return testCase;
    }

    /// <summary>
    /// Dohvaća opisnik jednog od TESTCERT_* certifikata. Parametar impor
    /// </summary>
    /// <param name="certNo">Jedna od TESTCERT_* konstanti.</param>
    /// <param name="imported">Ako je istina, osigurava da je certifikat u svom spremištu, a ako je laž osigurava da
    /// certifikata nema u spremištu. Ovo se odnosi samo na generirane certifikate.</param>
    /// <param name="ignoreShowsUI"></param>
    public TestCert GetTestCert( int certNo, bool imported, bool ignoreShowsUI = false )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan( certNo, 0 );

        Assert.IsTrue( TryGetTestCert( certNo, mustExist: true, out var props ) );

        CancelIfCezihWhenSuppressed(props.Cezih);

        // Traži certifikat u njegovom spremištu.
        var certs = Helpers.FindCertificate( props.StoreName, props.Thumbprint );
        // Ako je generirani certifikat već u spremištu, onda se briše iz spremišta.
        if(!imported && 0 < certs.Length && !props.Cezih)
        {
            // Briše se iz spremišta. Ako brisanje certifikata pokaže sučelje, radi se o korijenskom certifikatu i na
            // pokazivanje tog sučelja se ne može utjecati, pa se ne smije niti ignorirati ShowsUI svojstvo.
            CancelIfShowingUIWhenSuppressed( props.ShowsUI );
            Helpers.RemoveCertificateFromStore( props.StoreName, props.Thumbprint );

            // Kontrola i ažuriranje certs kolekcije u praznu.
            certs = Helpers.FindCertificate( props.StoreName, props.Thumbprint );
            Assert.IsEmpty( certs );
        }
        // Ako generirani certifikat nije pronađen, učita se.
        if(0 == certs.Length && !props.Cezih && props.FileName is not null)
        {
            // Učitava se iz priložene datoteke.
            var pfxPath = GetDeployedFilePath(props.FileName);
            var pfxPassword = (string)GetParameter<string>( "TestCertPfxPassword", mustExist: true )!;
            if(File.Exists( pfxPath ))
            {
                // Isto kao gore, učitavanje certifikata može pokazati sučelje na što se ne može utjecati.
                CancelIfShowingUIWhenSuppressed( props.ShowsUI );
                // Učitava se i opcinalno smješta u njegovo spremište.
                if(imported)
                {
                    Helpers.ImportPfx( pfxPath, pfxPassword, props.StoreName );
                    certs = Helpers.FindCertificate( props.StoreName, props.Thumbprint );
                }
                else certs = [Helpers.LoadPfx( pfxPath, pfxPassword )];
            }
        }
        if(1 < certs.Length)
            throw new( $"U spremištu {props.StoreName} pronađen je više nego jedan certifikat broj {certNo} digitalnog otiska '{props.Thumbprint}'." );
        if(0 == certs.Length)
            Assert.Inconclusive( $"Nije pronađen certifikat broj {certNo} digitalnog otiska '{props.Thumbprint}'." );

        if(!ignoreShowsUI)
            CancelIfShowingUIWhenSuppressed( props.ShowsUI );

        return new( props, certs[0] );
    }

    private object? GetParameter<T>( string key, bool mustExist )
    {
        var value = m_context?.Properties[key] as string;
        if(string.IsNullOrEmpty( value ))
            return !mustExist ? null : throw new( $"Provjeriti što je upisano u .runsettings: {key} nema vrijednost." );
        if(typeof( bool ) == typeof( T ) && bool.TryParse( value, out var boolVal ))
            return boolVal;
        else if(typeof( int ) == typeof( T ) && int.TryParse( value, out var intVal ))
            return intVal;
        else if(typeof( DateTimeOffset ) == typeof( T ) && DateTimeOffset.TryParse( value, out var dtmVal ))
            return dtmVal;
        else if(typeof( string ) == typeof( T ))
            return value!;
        else
            throw new( $"Nepodrđan tip podataka za {key}: {typeof( T ).Name}." );
    }

    public void RemoveTestCertFromStore( int certNo )
    {
        if(!TryGetTestCert( certNo, mustExist: false, out var props ))
            return;
        if(this.SuppressTestsShowingUI && props.ShowsUI)
            Assert.Inconclusive( $"Certifikat {certNo} nije moguće brisati jer su testovi koji prikazuju UI isključeni kroz .runsettings." );
        Helpers.RemoveCertificateFromStore( props.StoreName, props.Thumbprint );
    }

    private bool TryGetTestCert( int certNo, bool mustExist, [NotNullWhen( true )] out TestCert.Properties? props )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan( certNo, 0 );

        var thumbprintKey = $"TestCert.{certNo}.{nameof(TestCert.Properties.Thumbprint)}";
        var showsUIKey    = $"TestCert.{certNo}.{nameof(TestCert.Properties.ShowsUI)}";
        var cezihKey      = $"TestCert.{certNo}.{nameof(TestCert.Properties.Cezih)}";
        var fileNameKey   = $"TestCert.{certNo}.{nameof(TestCert.Properties.FileName)}";
        var importToKey   = $"TestCert.{certNo}.{nameof(TestCert.Properties.StoreName)}";

        var thumbprint = (string?)GetParameter<string>( thumbprintKey, mustExist: mustExist );
        if(thumbprint is null)
        {
            props = null;
            return false;
        }

        props = new TestCert.Properties(
            thumbprint: thumbprint,
            showsUI: (bool)GetParameter<bool>( showsUIKey, mustExist: true )!,
            cezih: (bool)GetParameter<bool>( cezihKey, mustExist: true )!,
            fileName: (string?)GetParameter<string>( fileNameKey, mustExist: false ),
            storeName: Enum.Parse<StoreName>((string)GetParameter<string>( importToKey, mustExist: true )!, ignoreCase: true)
            );
        return true;
    }
}

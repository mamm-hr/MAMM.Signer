using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Shared;

/// <summary>
/// Pomoćne metode za rad s certifikatima.
/// </summary>
///
internal static partial class CertHelpers
{
    /// <summary>
    /// Vrati sadržaj kolekcije certifikata kao privremeno spremište stvoreno u memoriji.
    /// </summary>
    ///
    /// <param name="coll">
    ///     Kolekcija certifikata kojom napuni spremište.</param>
    ///
    /// <returns>
    ///     Vrati spremište koje sadrži cerifikate iz kolekcije.</returns>
    ///
    /// <exception cref="Win32Exception">
    ///     Win32 pogreška tijekom izvođenja metode.</exception>
    ///
    public static unsafe X509Store AsStore(
          this X509Certificate2Collection coll
        )
    {
        var pStore = API.CertOpenStore( ( char* )API.CERT_STORE_PROV_MEMORY, 0, null, API.CERT_STORE_CREATE_NEW_FLAG, null );
        if(pStore is null)
            throw new Win32Exception();
        var store = new X509Store(new IntPtr(pStore));
        store.AddRange( coll );
        return store;
    }

    /// <summary>
    /// Nađe certifikat po digitalnom otisku u kolekciji certifikata; očekuje naći niti jedan ili najviše jedan
    /// certifikat.
    /// </summary>
    ///
    /// <param name="certs">
    ///     Kolekcija certifikata u kojoj traži.</param>
    ///
    /// <param name="thumbprint">
    ///     Digitalni otisak certifikata kojeg traži.</param>
    ///
    /// <param name="validOnly">
    ///     Istina da ignorira nevaljane certifikate.</param>
    ///
    /// <param name="cert">
    ///     Pronađeni certifikat.</param>
    ///
    /// <returns>
    ///     Prvi certifikat u kolekciji koji ima traženi digitalni otisak.</returns>
    ///
    public static bool FindCertificateByThumbprint(
          X509Certificate2Collection certs
        , string thumbprint
        , bool validOnly
#if NETFRAMEWORK || NETSTANDARD
        , out X509Certificate2? cert
#else
        , [NotNullWhen( true )] out X509Certificate2? cert
#endif
        )
    {
        if(thumbprint is null)
            throw new ArgumentNullException( nameof( thumbprint ) );
        thumbprint = thumbprint.ToUpperInvariant();
        // Nađe sve certifikate čiji tekst digitalnog otiska _započinje_ traženim.
        certs = certs.Find( X509FindType.FindByThumbprint, thumbprint, validOnly );
        // Iako bi nađeni certifikat morao (statistički) biti taj jedan, moguće je i da je dani tekst samo prefiks
        // otiska. Kako bilo ovdje se osigurava da se nađe baš certifikat koji ima otisak upravo jednak danom argumentom
        // funkcije. Također, ne bi smjelo biti moguće naći više od jednog certifikata po otisku, jer bi to bila dva
        // sadržajno identična certifikata (ponovno unašanje istog certifikata u spremište mora zamijeniti već
        // postojeći, a ne stvarati duplikate). Ipak, vrati prvog pronađenog u tom slučaju.
        cert = certs
            .Cast<X509Certificate2>()
            .FirstOrDefault( cert => cert.Thumbprint.Equals( thumbprint, StringComparison.OrdinalIgnoreCase ) );
        return cert is not null;
    }

    /// <summary>
    /// Vraća neslužbeni naziv certifikata ako je upisan, inače predmet certifikata.
    /// </summary>
    ///
    /// <param name="cert">
    ///     Certifikat u pitanju.</param>
    ///
    /// <returns>
    ///     Vraća string koji sadrži neslužbeni ili jednostavni naziv certifikata.</returns>
    public static string GetFriendlyOrSubjectName(
          this X509Certificate2 cert
        )
    {
        var friendlyName = cert.FriendlyName;
        return string.IsNullOrEmpty( friendlyName )
            ? cert.SubjectName.Format( multiLine: false ) //cert.GetNameInfo( X509NameType.SimpleName, forIssuer: false )
            : friendlyName
            ;
    }

    /// <summary>
    /// Vrati sve korisničke certifikate prijavljenog korisnika ili lokalnog računala ili sve certifikate na pametnim
    /// karticama umetnima u čitače.
    /// </summary>
    ///
    /// <param name="location">
    ///     Lokacija iz koje se dohvaćaju certifikati.</param>
    ///
    /// <param name="includeCsp">
    ///     Ovaj argument se koristi samo kad se certifikati dohvaćaju iz pametnih kartica i ako je <see
    ///     langword="true"/>, certifkati se dohvaćaju i kroz CSP. CSP treba konzultirati samo ako se očekuju starije
    ///     kartice.</param>
    ///
    /// <returns>
    ///     Vrati kolekciju certifikata prema kriteriju dohvata.</returns>
    ///
    public static X509Certificate2Collection GetUserCertificates(
          CertificateLocation location
        , bool includeCsp
        )
        => location switch
        {
            CertificateLocation.CurrentUser => GetUserStoreCertificates( StoreLocation.CurrentUser ),
            CertificateLocation.LocalMachine => GetUserStoreCertificates( StoreLocation.LocalMachine ),
            CertificateLocation.SmartCardReaders => CertHelpers.GetReaderCertificates( csp: includeCsp, ksp: true ),
            _ => throw new ArgumentOutOfRangeException( nameof( location ) )
        };

    /// <summary>
    /// Izabere certifikat za kriptografsku operaciju. V. <see cref="ICertificates"/> za detalje.
    /// </summary>
    public static X509Certificate2? SelectCertificate(
          X509Certificate2Collection certs
        , CertificatePurpose purpose
        , bool validOnly
        , string? title
        , string? message
        )
    {
        // Opcionlano ukloni nevaljane certifikate.
        if(validOnly)
        {
            var now = DateTime.Now;
            certs = [.. certs.Cast<X509Certificate2>().Where( c => c.NotBefore <= now && now <= c.NotAfter ).ToArray()];
        }
        // Prikaže popis certifikata. Popis može biti prazan!
        if(SelectSingleCertificateUI( certs.ForPurpose( purpose ), title, message, out var cert ))
            return cert;
        return null;
    }

    /// <summary>
    /// Izdvoji iz kolekcije certifikata samo one s traženom namjenom.
    /// </summary>
    ///
    /// <param name="coll">
    ///     Kolekcija certifikata koju pretraži.</param>
    ///
    /// <param name="purpose">
    ///     Namjena po kojoj izdvaja.</param>
    ///
    /// <returns>
    ///     Vrati kolekciju cerifikata za traženu namjnu.</returns>
    ///
    private static X509Certificate2Collection ForPurpose(
          this X509Certificate2Collection coll
        , CertificatePurpose purpose
        )
        => purpose switch
        {
            CertificatePurpose.Unspecified => coll,
            CertificatePurpose.Identification => coll.Find( X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, validOnly: false ),
            CertificatePurpose.Signature => coll.Find( X509FindType.FindByKeyUsage, X509KeyUsageFlags.NonRepudiation, validOnly: false ),
            _ => throw new ArgumentOutOfRangeException( nameof( purpose ) )
        };

    /// <summary>
    /// Vrati sve korisničke certifikate prijavljenog korisnika ili lokalnog računala.
    /// </summary>
    ///
    /// <param name="location">
    ///     Lokacija korisnikovog spremišta.</param>
    ///
    /// <returns>
    ///     Vrati sve certifikate iz spremišta.</returns>
    ///
    private static X509Certificate2Collection GetUserStoreCertificates(
          StoreLocation location
        )
    {
        using var store = new X509Store(StoreName.My, location);
        store.Open( OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly );
        var coll = store.Certificates;
        store.Close();
        return coll;
    }

    /// <summary>
    /// Prikaže sistemski dijalog za izbor jednog certifikata iz kolekcije certifikata.
    /// </summary>
    ///
    /// <param name="coll">
    ///     Kolekcija certifikata iz koje se izabire jedan.</param>
    ///
    /// <param name="title">
    ///     Naslov dijaloškog okvira.</param>
    ///
    /// <param name="message">
    ///     Poruka prikazana na dijaloškom okviru.</param>
    ///
    /// <param name="selectedCert">
    ///     Od korisnika izabrani certifikat kad je rezultat metode <see langword="true"/>, inače <see
    ///     langword="null"/>.</param>
    ///
    /// <returns>
    ///     Vrati <see langword="true"/> ako je korisnik izabrao certifikat, a <see langword="false"/> ako je
    ///     odustao.</returns>
    ///
    private static bool SelectSingleCertificateUI(
          X509Certificate2Collection coll
        , string? title
        , string? message
#if NETFRAMEWORK || NETSTANDARD
        , out X509Certificate2? selectedCert
#else
        , [NotNullWhen( true )] out X509Certificate2? selectedCert
#endif
        )
    {
        selectedCert = null;
        if(!Environment.UserInteractive)
            return false;
#if NETFRAMEWORK
        var certs = X509Certificate2UI.SelectFromCollection(coll, title, message, X509SelectionFlag.SingleSelection);
        if(0 < certs.Count)
            selectedCert = certs[0];
        return selectedCert is not null;
#else
        using var store = coll.AsStore();
        return SelectSingleCertificateUI( store, title, message, out selectedCert );
#endif
    }

    /// <summary>
    /// Pokaže sistemski dijalog za izbor certifikata iz danog spremišta certifikata i dopusti izbor jednog od
    /// certifikata iz tog spremišta.
    /// </summary>
    ///
    /// <param name="displayStore">
    ///     Spremište sa certifikatima iz kojeg se izabire jedan.</param>
    ///
    /// <param name="title">
    ///     Naslov dijaloškog okvira.</param>
    ///
    /// <param name="message">
    ///     Poruka korisniku na dijaloškom okviru.</param>
    ///
    /// <param name="selectedCert">
    ///     Izabrani certifikat ili <see langword="null"/> ako je korisnik odustao.</param>
    ///
    /// <returns>
    ///     Vraća <see langword="true"/> ako je korisnik izabrao certifikat, a <see langword="false"/> ako je
    ///     odustao.</returns>
    ///
    internal unsafe static bool SelectSingleCertificateUI(
          X509Store displayStore
        , string? title
        , string? message
#if NETFRAMEWORK
        , out X509Certificate2? selectedCert
#else
        , [NotNullWhen( true )] out X509Certificate2? selectedCert
#endif
        )
    {
        fixed(char* pszTitle = title)
        fixed(char* pszMessage = message)
        {
            void* pDisplayStore = (void*)displayStore.StoreHandle;
            void* pSelectedCert = API.CryptUIDlgSelectCertificateFromStore((void*)displayStore.StoreHandle, null, pszTitle, pszMessage, 0, 0, null);
            if(pSelectedCert is not null)
            {
                selectedCert = new X509Certificate2( new IntPtr( pSelectedCert ) );
                API.CertFreeCertificateContext( pSelectedCert );
            }
            else selectedCert = null;
        }
        return selectedCert is not null;
    }

    /// <summary>
    /// Očita certifikate iz kartica umetnutih u čitače preko njihovih implementacija CSP-a i KSP-a, uz uklanjanje
    /// duplikata (ako kartica implementira i CSP i KSP).
    /// </summary>
    ///
    /// <param name="readers">
    ///     Popis naziva pod kojim su čitači registrirani u sustavu.</param>
    ///
    /// <returns>
    ///     Vrati kolekciju certifikata očitanih s pametnih kartica u čitačima.</returns>
    ///
    /// <exception cref="Win32Exception">
    ///     Win32 pogreška tijekom izvršavanja metode.</exception>
    ///
    public static X509Certificate2Collection GetReaderCertificates(
          bool csp
        , bool ksp
        )
    {
        var readers = SCardListReaders();
        var cspColl =  csp ? CryptGetReaderCertificates( readers ) : null;
        var kspColl = ksp ? NCryptGetReaderCertificates( readers ) : null;
        if(null == cspColl && null == kspColl)
            return [];
        else if(null != cspColl && null == kspColl)
            return cspColl;
        else if(null == cspColl && null != kspColl)
            return kspColl;
        else
        {
            Debug.Assert( null != cspColl && null != kspColl );
            // Ključevi implementirani kroz Microsoftov KSP pojavljuju se i kroz Microsoftov ugrađeni CSP, pa ih
            // zato imamo dva puta.
            var coll = new X509Certificate2Collection();
            while(0 < cspColl!.Count)
            {
                var cspCert = cspColl[0];
                if(0 == kspColl!.Find( X509FindType.FindByThumbprint, cspCert.Thumbprint, false ).Count)
                    coll.Add( cspCert );
                else
                    cspCert.Dispose();
                cspColl.RemoveAt( 0 );
            }
            coll.AddRange( kspColl );
            return coll;
        }
    }

    /// <summary>
    /// Klonira certifikat na način da on ostaje asociran sa svojim privatnim ključem.
    /// </summary>
    ///
    /// <param name="cert">
    ///     Certifikat koji se klonira.</param>
    ///
    /// <returns>
    ///     Klonirani certifikat.</returns>
    ///
    /// <remarks>
    /// <para>
    ///     Koristiti ovu metodu kad se dobiveni certifikatni kontekst ne može naprosto duplicirati uvećanjem brojača
    ///     referenci (CertDuplicateCertificateContext).</para>
    /// </remarks>
    ///
    private static unsafe X509Certificate2 CloneCertificate( X509Certificate2 cert )
    {
        uint cbyBuffer = 0;
        if(!API.CertSerializeCertificateStoreElement( (void*)cert.Handle, 0, null, &cbyBuffer ))
            throw new Win32Exception();
        byte[] buffer = new byte[cbyBuffer];
        fixed(byte* pbyBuffer = buffer)
        {
            if(!API.CertSerializeCertificateStoreElement( (void*)cert.Handle, 0, pbyBuffer, &cbyBuffer ))
                throw new Win32Exception();
            uint dwContextType = 0;
            void* pvClonedContext = null;
            if(!API.CertAddSerializedElementToStore( null, pbyBuffer, cbyBuffer, API.CERT_STORE_ADD_ALWAYS, 0,
                API.CERT_STORE_CERTIFICATE_CONTEXT_FLAG, &dwContextType, &pvClonedContext ))
                throw new Win32Exception();
            Debug.Assert( API.CERT_STORE_CERTIFICATE_CONTEXT == dwContextType );
            var clonedCert = new X509Certificate2( new IntPtr( pvClonedContext ) );
            return new X509Certificate2( new IntPtr( pvClonedContext ) );
        }
    }

    /// <summary>
    /// Očita certifikate iz kartica umetnutih u čitače preko njihove implementacije CSP-a.
    /// </summary>
    ///
    /// <param name="readers">
    ///     Popis naziva pod kojim su čitači registrirani u sustavu.</param>
    ///
    /// <returns>
    ///     Vrati kolekciju certifikata očitanih s pametnih kartica u čitačima.</returns>
    ///
    /// <exception cref="Win32Exception">
    ///     Win32 pogreška tijekom izvršavanja metode.</exception>
    ///
    private static unsafe X509Certificate2Collection CryptGetReaderCertificates(
          IReadOnlyList<string> readers
        )
    {
        var coll = new X509Certificate2Collection();

        uint dwIndex = 0;
        uint dwProvType = 0;
        uint cbyProvName = 0;
        while(API.CryptEnumProviders( dwIndex, null, 0, &dwProvType, null, &cbyProvName ))
        {
            char[] achProvName = new char[cbyProvName];
            fixed(char* pszProvName = achProvName)
            {
                if(!API.CryptEnumProviders( dwIndex, null, 0, &dwProvType, pszProvName, &cbyProvName ))
                    continue;
                foreach(string readerName in readers)
                {
                    string containerName = $@"\\.\{readerName}\";
                    fixed(char* pszContainerName = containerName)
                    {
                        IntPtr hProv = IntPtr.Zero;
                        if(!API.CryptAcquireContext( &hProv, pszContainerName, pszProvName, dwProvType, API.CRYPT_SILENT_FLAG /*| API.CRYPT_VERIFYCONTEXT*/ ))
                            continue;
                        try
                        {
                            void* hCertStore = null;
                            uint dwDataLen = ( uint )sizeof(void*);
                            if(!API.CryptGetProvParam( hProv, API.PP_USER_CERTSTORE, (byte*)&hCertStore, &dwDataLen, 0 ))
                                continue;
                            try
                            {
                                for(void* pCertContext = API.CertEnumCertificatesInStore( hCertStore, null ); null != pCertContext; pCertContext = API.CertEnumCertificatesInStore( hCertStore, pCertContext ))
                                {
                                    try
                                    {
                                        // Enumerirani certifikat se mora otpustiti prije nego se pozove CertCloseStore,
                                        // pa ga za vraćanje pozivatelju treba na ovaj ili onaj način klonirati.
                                        using var cert = new X509Certificate2(new IntPtr(pCertContext));
                                        coll.Add( CloneCertificate( cert ) );
                                    }
                                    catch { }
                                }
                            }
#if DEBUG
                            catch { }
                            if(!API.CertCloseStore( hCertStore, API.CERT_CLOSE_STORE_CHECK_FLAG ))
                                throw new Win32Exception();
#else
                            finally
                            {
                                API.CertCloseStore( hCertStore, 0 );
                            }
#endif
                        }
                        finally
                        {
                            API.CryptReleaseContext( hProv, 0 );
                        }
                    }
                }
            }
            dwIndex++;
        }

        return coll;
    }

    /// <summary>
    /// Očita certifikate iz kartica umetnutih u čitače kroz KSP.
    /// </summary>
    ///
    /// <param name="readers">
    ///     Popis naziva pod kojim su čitači registrirani u sustavu.</param>
    ///
    /// <returns>
    ///     Vrati kolekciju certifikata očitanih s pametnih kartica u čitačima.</returns>
    ///
    /// <exception cref="Win32Exception">
    ///     Win32 pogreška tijekom izvršavanja metode.</exception>
    ///
    private static unsafe X509Certificate2Collection NCryptGetReaderCertificates(
          IReadOnlyList<string> readers
        )
    {
        var coll = new X509Certificate2Collection();

        uint dwProviderCount = 0;
        API.NCryptProviderName* pProviderList = null;
        int status = API.NCryptEnumStorageProviders(&dwProviderCount, &pProviderList, API.NCRYPT_SILENT_FLAG);
        if(API.ERROR_SUCCESS != status) throw new Win32Exception( status );
        try
        {
            for(int providerIter = 0; providerIter < dwProviderCount; providerIter++)
            {
                foreach(string readerName in readers)
                {
                    IntPtr hProvider = IntPtr.Zero;
                    if(API.ERROR_SUCCESS != API.NCryptOpenStorageProvider( &hProvider, pProviderList[providerIter].pszName, 0 ))
                        continue;
                    try
                    {
                        fixed(char* pszReaderName = readerName)
                        fixed(char* pszNCRYPT_READER_PROPERTY = API.NCRYPT_READER_PROPERTY)
                        {
                            uint cbyInput = ( uint )(API.lstrlen(pszReaderName) + 1) * sizeof(char);
                            if(API.ERROR_SUCCESS == API.NCryptSetProperty( hProvider, pszNCRYPT_READER_PROPERTY, (byte*)pszReaderName, (uint)(API.lstrlen( pszReaderName ) + 1) * sizeof( char ), API.NCRYPT_SILENT_FLAG ))
                            {
                                fixed(char* pszNCRYPT_USER_CERTSTORE_PROPERTY = API.NCRYPT_USER_CERTSTORE_PROPERTY)
                                {
                                    void* hCertStore = null;
                                    uint cbyOutput = ( uint )sizeof(void*);
                                    uint cbyExpected = 0;
                                    status = API.NCryptGetProperty( hProvider, pszNCRYPT_USER_CERTSTORE_PROPERTY, (byte*)&hCertStore, cbyOutput, &cbyExpected, API.NCRYPT_SILENT_FLAG );
                                    if(API.ERROR_SUCCESS == status)
                                    {
                                        try
                                        {
                                            for(void* pCertContext = API.CertEnumCertificatesInStore( hCertStore, null ); null != pCertContext; pCertContext = API.CertEnumCertificatesInStore( hCertStore, pCertContext ))
                                            {
                                                try
                                                {
                                                    // Enumerirani certifikat se mora otpustiti prije nego se pozove
                                                    // CertCloseStore, pa ga za vraćanje pozivatelju treba na ovaj ili
                                                    // onaj način klonirati.
                                                    using var cert = new X509Certificate2(new IntPtr(pCertContext));
                                                    coll.Add( CloneCertificate( cert ) );
                                                }
                                                catch { }
                                            }
                                        }
#if DEBUG
                                        catch { }
                                        if(!API.CertCloseStore( hCertStore, API.CERT_CLOSE_STORE_CHECK_FLAG ))
                                            throw new Win32Exception();
#else
                                        finally
                                        {
                                            API.CertCloseStore( hCertStore, 0 );
                                        }
#endif
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        var result = API.NCryptFreeObject( hProvider );
                        Debug.Assert( API.ERROR_SUCCESS == result );
                    }
                }
            }
        }
        catch
        {
            foreach(var cert in coll)
                cert.Dispose();
        }
        finally
        {
            var result = API.NCryptFreeBuffer( pProviderList );
            Debug.Assert( API.ERROR_SUCCESS == result );
        }

        return coll;
    }

    /// <summary>
    /// Dohvati popis čitača pametnih kartica poznatih sustavu.
    /// </summary>
    ///
    /// <returns>
    ///     Vrati niz s nazivima pod kojima su čitači poznati sustavu.</returns>
    ///
    /// <exception cref="Win32Exception">
    ///     Win32 iznimka tijekom izvođenja metode.</exception>
    ///
    private static unsafe List<string> SCardListReaders()
    {
        var readers = new List<string>();

        IntPtr hContext = IntPtr.Zero;
        var status = API.SCardEstablishContext(API.SCARD_SCOPE.SCARD_SCOPE_USER, null, null, &hContext);
        if(API.SCARD_S_SUCCESS != status) throw new Win32Exception( status );
        try
        {
            uint cchReaders = 0;
            status = API.SCardListReaders( hContext, null, null, &cchReaders );
            if(API.SCARD_E_NO_READERS_AVAILABLE != status)
            {
                if(API.SCARD_S_SUCCESS != status) throw new Win32Exception( status );

                char[] buffer = new char[cchReaders];
                fixed(char* mszReaders = buffer)
                {
                    status = API.SCardListReaders( hContext, null, mszReaders, &cchReaders );
                    if(API.SCARD_E_NO_READERS_AVAILABLE != status)
                    {
                        if(API.SCARD_S_SUCCESS != status) throw new Win32Exception( status );
                        char* pszReaderName = mszReaders;
                        while(0 != *pszReaderName)
                        {
                            readers.Add( new string( pszReaderName ) );
                            pszReaderName += API.lstrlen( pszReaderName ) + 1;
                        }
                    }
                }
            }
        }
        finally
        {
            var result = API.SCardReleaseContext( hContext );
            Debug.Assert( API.SCARD_S_SUCCESS == result );
        }

        return readers;
    }

    /// <summary>
    /// Operacije jezgre potrebne za implementaciju 'unsafe' metoda ovog razreda.
    /// </summary>
    ///
    private static partial class API
    {
        public const int ERROR_SUCCESS = 0;

        public const uint CERT_CLOSE_STORE_CHECK_FLAG         = 0x00000002;
        public const uint CERT_STORE_CREATE_NEW_FLAG          = 0x00002000;
        public const uint CERT_STORE_CERTIFICATE_CONTEXT      = 1;
        public const uint CERT_STORE_PROV_MEMORY              = 2;
        public const uint CERT_STORE_ADD_ALWAYS               = 4;
        public const uint CERT_STORE_CERTIFICATE_CONTEXT_FLAG = 1u << 1;

        public const uint CRYPT_SILENT_FLAG   = 0x00000040;
        public const uint CRYPT_VERIFYCONTEXT = 0xF0000000;
        public const uint PP_USER_CERTSTORE   = 42;

        public const uint   NCRYPT_SILENT_FLAG             = 0x00000040;
        public const string NCRYPT_READER_PROPERTY         = "SmartCardReader";
        public const string NCRYPT_USER_CERTSTORE_PROPERTY = "SmartCardUserCertStore";

        public const int SCARD_S_SUCCESS              = 0;
        public const int SCARD_E_NO_READERS_AVAILABLE = unchecked((int)0x8010002EL);

        public enum SCARD_SCOPE : uint
        {
            SCARD_SCOPE_USER = 0U,
            SCARD_SCOPE_SYSTEM = 2U,
        }

        [StructLayout( LayoutKind.Sequential )]
        public unsafe struct NCryptProviderName
        {
            public char* pszName;
            public char* pszComment;
        }


#if NETFRAMEWORK

        [DllImport( CRYPT_LIB, ExactSpelling = true, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe bool CertAddSerializedElementToStore( void* hCertStore, byte* pbElement, uint cbElement, uint dwAddDisposition, uint dwFlags, uint dwContextTypeFlags, uint* pdwContextType, void** ppvContext );

        [DllImport( CRYPT_LIB, ExactSpelling = true, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe bool CertCloseStore( void* hCertStore, uint dwFlags );

        [DllImport(CRYPT_LIB, ExactSpelling = true, SetLastError = true), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe void* CertDuplicateCertificateContext(void* pCertContext);

        [DllImport( CRYPT_LIB, ExactSpelling = true, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe void* CertEnumCertificatesInStore( void* hCertStore, void* pPrevCertContext );

        [DllImport( CRYPT_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe bool CertGetCertificateContextProperty( IntPtr pCertContext, uint dwPropId, void* pvData, uint* pcbData );

        [DllImport( CRYPT_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe bool CertFreeCertificateContext( void* pCertContext );

        [DllImport( CRYPT_LIB, ExactSpelling = true, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe void* CertOpenStore( char* lpszStoreProvider, uint dwEncodingType, void* hCryptProv, uint dwFlags, void* pvPara );

        [DllImport( CRYPT_LIB, ExactSpelling = true, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe bool CertSerializeCertificateStoreElement( void* pCertContext, uint dwFlags, byte* pbElement, uint* pcbElement );

        [DllImport( CRYPT_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe bool CertSetCertificateContextProperty( IntPtr pCertContext, uint dwPropId, uint dwFlags, IntPtr pvData );

        [DllImport( CRYPT_LIB, ExactSpelling = true, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe bool CryptAcquireCertificatePrivateKey( IntPtr pCert, uint dwFlags, void* pvParameters, IntPtr* phCryptProvOrNCryptKey, uint* pdwKeySpec, bool* pfCallerFreeProvOrNCryptKey );

        [DllImport( ADVAPI_LIB, ExactSpelling = true, EntryPoint = "CryptAcquireContextW", SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe bool CryptAcquireContext( IntPtr* phProv, char* szContainer, char* szProvider, uint dwProvType, uint dwFlags );

        [DllImport( ADVAPI_LIB, ExactSpelling = true, EntryPoint = "CryptEnumProvidersW", SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe bool CryptEnumProviders( uint dwIndex, uint* pdwReserved, uint dwFlags, uint* pdwProvType, char* szProvName, uint* pcbProvName );

        [DllImport( ADVAPI_LIB, ExactSpelling = true, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe bool CryptGetProvParam( IntPtr hProv, uint dwParam, byte* pbData, uint* pdwDataLen, uint dwFlags );

        [DllImport( ADVAPI_LIB, ExactSpelling = true, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern bool CryptReleaseContext( IntPtr hProv, uint dwFlags );

        [DllImport( CRYPTUI_LIB, ExactSpelling = true, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe void* CryptUIDlgSelectCertificateFromStore( void* hCertStore, void* hwnd, char* pwszTitle, char* pwszDisplayString, uint dwDontUseColumn, uint dwFlags, void* pvReserved );

        [DllImport( KERNEL_LIB, ExactSpelling = true, EntryPoint = "lstrlenW" ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe int lstrlen( char* lpString );

        [DllImport( NCRYPT_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe int NCryptEnumStorageProviders( uint* pdwProviderCount, NCryptProviderName** ppProviderList, uint dwFlags );

        [DllImport( NCRYPT_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe int NCryptFreeBuffer( void* pvInput );

        [DllImport( NCRYPT_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern int NCryptFreeObject( IntPtr hObject );

        [DllImport( NCRYPT_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe int NCryptGetProperty( IntPtr hObject, char* pszProperty, byte* pbOutput, uint cbOutput, uint* pcbResult, uint dwFlags );

        [DllImport( NCRYPT_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe int NCryptOpenKey( IntPtr hProvider, IntPtr* phKey, char* pszKeyName, uint dwLegacyKeySpec, uint dwFlags );

        [DllImport( NCRYPT_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe int NCryptOpenStorageProvider( IntPtr* phProvider, char* pszProviderName, uint dwFlags );

        [DllImport( NCRYPT_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe int NCryptSetProperty( IntPtr hObject, char* pszProperty, byte* pbInput, uint cbInput, uint dwFlags );

        [DllImport( WINSCARD_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe int SCardEstablishContext( SCARD_SCOPE dwScope, void* pvReserved1, void* pvReserved2, IntPtr* phContext );

        [DllImport( WINSCARD_LIB, ExactSpelling = true, EntryPoint = "SCardListReadersW" ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static extern unsafe int SCardListReaders( IntPtr hContext, char* mszGroups, char* mszReaders, uint* pcchReaders );

        [DllImport( WINSCARD_LIB, ExactSpelling = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        internal static extern int SCardReleaseContext( IntPtr hContext );

#else

        [LibraryImport( CRYPT_LIB, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static unsafe partial bool CertAddSerializedElementToStore( void* hCertStore, byte* pbElement, uint cbElement, uint dwAddDisposition, uint dwFlags, uint dwContextTypeFlags, uint* pdwContextType, void** ppvContext );

        [LibraryImport( CRYPT_LIB, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static unsafe partial bool CertCloseStore( void* hCertStore, uint dwFlags );

        [LibraryImport( CRYPT_LIB, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial void* CertDuplicateCertificateContext( void* pCertContext );

        [LibraryImport( CRYPT_LIB, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial void* CertEnumCertificatesInStore( void* hCertStore, void* pPrevCertContext );

        [LibraryImport( CRYPT_LIB ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static unsafe partial bool CertFreeCertificateContext( void* pCertContext );

        [LibraryImport( CRYPT_LIB, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial void* CertOpenStore( char* lpszStoreProvider, uint dwEncodingType, void* hCryptProv, uint dwFlags, void* pvPara );

        [LibraryImport( CRYPT_LIB, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static unsafe partial bool CertSerializeCertificateStoreElement( void* pCertContext, uint dwFlags, byte* pbElement, uint* pcbElement );

        [LibraryImport( ADVAPI_LIB, EntryPoint = "CryptAcquireContextW", SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static unsafe partial bool CryptAcquireContext( IntPtr* phProv, char* szContainer, char* szProvider, uint dwProvType, uint dwFlags );

        [LibraryImport( ADVAPI_LIB, EntryPoint = "CryptEnumProvidersW", SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static unsafe partial bool CryptEnumProviders( uint dwIndex, uint* pdwReserved, uint dwFlags, uint* pdwProvType, char* szProvName, uint* pcbProvName );

        [LibraryImport( ADVAPI_LIB, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static unsafe partial bool CryptGetProvParam( IntPtr hProv, uint dwParam, byte* pbData, uint* pdwDataLen, uint dwFlags );

        [LibraryImport( ADVAPI_LIB, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static partial bool CryptReleaseContext( IntPtr hProv, uint dwFlags );

        [LibraryImport( CRYPTUI_LIB, SetLastError = true ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial void* CryptUIDlgSelectCertificateFromStore( void* hCertStore, void* hwnd, char* pwszTitle, char* pwszDisplayString, uint dwDontUseColumn, uint dwFlags, void* pvReserved );

        [LibraryImport( KERNEL_LIB, EntryPoint = "lstrlenW" ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial int lstrlen( char* lpString );

        [LibraryImport( NCRYPT_LIB ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial int NCryptEnumStorageProviders( uint* pdwProviderCount, NCryptProviderName** ppProviderList, uint dwFlags );

        [LibraryImport( NCRYPT_LIB ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial int NCryptFreeBuffer( void* pvInput );

        [LibraryImport( NCRYPT_LIB ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static partial int NCryptFreeObject( IntPtr hObject );

        [LibraryImport( NCRYPT_LIB ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial int NCryptGetProperty( IntPtr hObject, char* pszProperty, byte* pbOutput, uint cbOutput, uint* pcbResult, uint dwFlags );

        [LibraryImport( NCRYPT_LIB ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial int NCryptOpenStorageProvider( IntPtr* phProvider, char* pszProviderName, uint dwFlags );

        [LibraryImport( NCRYPT_LIB ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial int NCryptSetProperty( IntPtr hObject, char* pszProperty, byte* pbInput, uint cbInput, uint dwFlags );

        [LibraryImport( WINSCARD_LIB ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial int SCardEstablishContext( SCARD_SCOPE dwScope, void* pvReserved1, void* pvReserved2, IntPtr* phContext );

        [LibraryImport( WINSCARD_LIB, EntryPoint = "SCardListReadersW" ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static unsafe partial int SCardListReaders( IntPtr hContext, char* mszGroups, char* mszReaders, uint* pcchReaders );

        [LibraryImport( WINSCARD_LIB ), DefaultDllImportSearchPaths( DllImportSearchPath.System32 )]
        public static partial int SCardReleaseContext( IntPtr hContext );

#endif

        private const string ADVAPI_LIB   = "advapi32.dll";
        private const string CRYPTUI_LIB  = "cryptui.dll";
        private const string CRYPT_LIB    = "crypt32.dll";
        private const string KERNEL_LIB   = "kernel32.dll";
        private const string NCRYPT_LIB   = "ncrypt.dll";
        private const string WINSCARD_LIB = "winscard.dll";
    }
}

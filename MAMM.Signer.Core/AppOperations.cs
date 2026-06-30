using MAMM.Signer.Pkcs;
using MAMM.Signer.Shared;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Core;

/// <summary>
/// Implementira operacije koje čine funkcionalnost programa. Središnja
/// </summary>
public static class AppOperations
{
    /// <summary>
    /// Enumerira datoteke prema specifikaciji ulaznih datoteka.
    /// </summary>
    ///
    /// <param name="inputSpec">
    ///     Specifikacija ulaznih datoteka kako je zadana programskim argumentom.</param>
    ///
    /// <param name="outputExt">
    ///     Ekstenzija izlaznih datoteka. Datoteke s ovom ekstenzijom će se preskočiti tijekom enumeracije.</param>
    ///
    /// <returns>
    ///     Vraća enumerator ulaznih datoteke.</returns>
    ///
    /// <exception cref="InvalidOutputFileExtensionException">
    ///     Ekstenzija izlaznih datoteka ne započinje točkom ili je samo točka ili sadrži nedopuštene znakove.</exception>
    ///
    public static IEnumerable<FileInfo> EnumerateInputFilesFromSpec(
          FileInfo inputSpec
        )
    {
        if(inputSpec.Exists)
            yield return inputSpec;
        else
        {
            var (dir, pattern)
                = inputSpec.Directory is not null
                ? (inputSpec.Directory, inputSpec.Name)
                : (new DirectoryInfo( inputSpec.FullName ), "*");
            foreach(var file in dir.EnumerateFiles( pattern ))
                yield return file;
        }
    }

    /// <summary>
    /// Enumerira datoteke iz popisa ulaznih datoteka.
    /// </summary>
    ///
    /// <param name="listFile">
    ///     Datoteka popisa zadana programskim argumentom.</param>
    ///
    /// <returns>
    ///     Vraća enumerator ulaznih datoteke.</returns>
    ///
    public static IEnumerable<FileInfo> EnumerateInputFilesFromList(
          FileInfo listFile
        )
    {
        if(!listFile.Exists)
            throw new InputListFileDoesNotExistException( listFile.FullName );
        using var reader = listFile.OpenText();
        for(var line = reader.ReadLine(); line is not null; line = reader.ReadLine())
            yield return new( line );
    }

    /// <summary>
    /// Poništi ishod operacije izvršene metodom <see cref="RunOperationAsync(AppOptions, Pkcs7Options?, AppResult,
    /// AppResult.OpResult, CancellationToken)"/>.
    /// </summary>
    ///
    /// <param name="opResult">
    ///     Objekt koji prati operaciju i koji ažurira.</param>

    /// <returns>
    ///     Vrati <see langword="true"/> ako je operaciju uspio poništiti u cijelosti, a inače vrati <see
    ///     langword="false"/> pri čemu ažurira objekt ishoda tako da reflektira poništeno i neponišteno.</returns>
    ///
    public static bool RollbackOperation(
          AppResult.OpResult opResult
        )
    {
        try
        {
            opResult.OutputFile?.Delete();
            opResult.OutputFile = null;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Izvrši operaciju ovog programa nad jednom od datoteka.
    /// </summary>
    ///
    /// <param name="appOptions">
    ///     Programske opcije koje utvrđuju simetrični algoritam za šifriranje.</param>
    ///
    /// <param name="pkcs7Options">
    ///     Opcije koje upravljaju operacijom potpisivanja.</param>
    ///
    /// <param name="appResult">
    ///     Objekt koji prati rad i sadrži izabrani certifikat za enkripciju ključa.</param>
    ///
    /// <param name="opResult">
    ///     Objekt koji prati operaciju i koji ažurira.</param>
    ///
    /// <param name="cancellationToken">
    ///     Objekt kojim se može zatražiti prekid operacije.</param>
    ///
    /// <returns>
    ///     Vrati potpisani podatak.</returns>
    ///
    public static async Task RunOperationAsync(
          AppOptions appOptions
        , Pkcs7Options? pkcs7Options
        , AppResult appResult
        , AppResult.OpResult opResult
        , CancellationToken cancellationToken
        )
    {
        // Datoteke koje se potpisuju/šifriraju ne smiju imati PKCS eksenziju, dok je ulazne datoteke koje se ovjeravaju
        // moraju imati.
        bool inputFileHasPkcs7Ext = appOptions.Ext.Equals( opResult.InputFile.Extension, StringComparison.OrdinalIgnoreCase );

        // Učitava sadržaj ulazne datoteke.
        if(!opResult.InputFile.Exists)
            throw new InputListFileDoesNotExistException( opResult.InputFile.FullName );
        var data = File.ReadAllBytes( opResult.InputFile.FullName );

        Debug.Assert( !opResult.DataProduced );

        // Prvo potpisuje stvarajući SignedData(data).
        if(appOptions.Sign && !inputFileHasPkcs7Ext)
            data = SignData( data, appOptions, pkcs7Options, appResult, opResult );

        // Potom šifrira stvarajući EnvelopedData(data) ili EnvelopedData(SignedData(data)).
        if(appOptions.Encrypt && !inputFileHasPkcs7Ext)
            data = EncryptData( data, appOptions, pkcs7Options, appResult, opResult );

        // Onda pohrani.
        if(opResult.DataProduced)
        {
            // Snimi producirani potpisani/omotani podatak.
            SaveProducedData( data, appOptions, opResult );

            // Ponovo očita upravo produciranu datoteku ako je zatraženo verificiranje.
            Debug.Assert( opResult.OutputFile is not null );
            if(appOptions.Verify)
                data = File.ReadAllBytes( opResult.OutputFile!.FullName );
        }

        // Konačno ovjeri podatak, ako je zatraženo. Ovjeri ulaznu datoteku ako iz nje nije producirana izlazna,
        // inače ovjeri produciranu izlaznu datoteku.
        if(appOptions.Verify && (opResult.DataProduced || inputFileHasPkcs7Ext))
        {
            // Ovjeri potpis uz brisanje producirane datoteke u slučaju da nije uspješno ovjerena. Koristi certifikat
            // kojim je producirani podatak šifrirao, inače izvršnim opcijama opcionalno zadani certifikat za šifriranje
            // koristi kao certifikat za dešifriranje.
            data = VerifyData( data, opResult.EncryptCert ?? appResult.EncryptCert, pkcs7Options, appResult, opResult );

            // Spremi ovjereni podatak ako nije ovjeren upravo producirani podatak (jer u tom je slučaju ovjereni
            // podatak upravo ulazna datoteka, samo bi se pregazio ulazni podatak).
            if(!opResult.DataProduced)
                SaveVerifiedData( data, appOptions, opResult );
        }
    }

    /// <summary>
    /// Izvrši operaciju ovog programa nad svakom datotekom u listi.
    /// </summary>
    ///
    /// <param name="inputFiles">
    ///     Datoteke koje se obrađuju.</param>
    ///
    /// <param name="appOptions">
    ///     Izvršne opcije programa, zadane kroz korisničko sučelje.</param>
    ///
    /// <param name="pkcs7Options">
    ///     Opcionalna svojstva koja usmjeravaju izvođenje PKCS #7 operacija.</param>
    ///
    /// <param name="certManager">
    ///     Implementacija sučelja s operacijama nad certifikatima.</param>
    ///
    /// <returns>
    ///     Vrati opisnik ishoda rada aplikacije.</returns>
    public static async Task<AppResult> RunOperationsAsync(
          IEnumerable<FileInfo> inputFiles
        , AppOptions appOptions
        , Pkcs7Options? pkcs7Options
        , ICertificateManager certManager
        , CancellationToken cancellationToken = default
        )
    {
        // Akumulira ishod rada.
        AppResult appResult = new();
        try
        {
            // Provjeri da je format ekstenzije izlazne datoteke valjan i da započinje točkom.
            ValidateOutputFileExtension( appOptions.Ext );

            // Izbor potpisnog certifikata ako je zatraženo potpisivanje.
            if(appOptions.Sign)
                SelectSignCertificate( appOptions, appResult, certManager, canUseIdent: true );

            // Izbor "enkripcijskog" certifikata ako je zatražena enkripcija ili ovjera.
            if(appOptions.Encrypt)
                SelectEncryptCertificate( appOptions, appResult, certManager );

            // Izbor opcinalno već zadanog certifikata primatelja za dešifriranje ako se radi o ovjeri ulaznih datoteka,
            // a ne kontrolnoj ovjeri izlaznih datoteka. Ovdje samo digitalni otisak certifikata pretvori u traženi
            // objekt certifikata.
            if(appOptions.Verify && !appOptions.Encrypt && appOptions.EncryptCert is not null)
                SelectEncryptCertificate( appOptions, appResult, certManager, forceSilentUi: true );

            // Vrši zatražene operacije nad svakom specificiranom ulaznom datotekom.
            foreach(var inputFile in inputFiles)
            {
                // Ishod ove operacije.
                var opResult = new AppResult.OpResult(inputFile);

                // Doda ishod operacije u ishod rada prije operacije u slučaju da ostane zabilježen i u slučaju da
                // operacija parcijalno bude izvršena prije nego završi iznimkom.
                appResult.OpResults.Add( opResult );

                // Izvršava operaciju nad tekućom ulaznom datotekom.
                await RunOperationAsync( appOptions, pkcs7Options, appResult, opResult, cancellationToken );
            }
        }
        catch(Exception ex)
        {
            // Neuspješni ishod rada.
            appResult.Exception = ex;
            appResult.FailedOp = appResult.OpResults.LastOrDefault();

            // U slučaju pogreške, poništava sve ishode rada. Iterira preko kopije liste ishoda jer tijekom iteriranja
            // ukloni iz originalne liste uspješno poništeni ishod, tako da ostanu koje nije mogao ponišiti u cjelosti.
            foreach(var opResult in appResult.OpResults.ToList())
            {
                if(RollbackOperation( opResult ))
                    appResult.OpResults.Remove( opResult );
            }
        }

        // Vraća ishod rada.
        return appResult;
    }

    /// <summary>
    /// Potpiše i vrati potpisani podatak ažurirajući status operacije.
    /// </summary>
    ///
    /// <param name="data">
    ///     Podatak koji potpiše.</param>
    ///
    /// <param name="appOptions">
    ///     Programske opcije koje utvrđuju simetrični algoritam za šifriranje.</param>
    ///
    /// <param name="pkcs7Options">
    ///     Opcije koje upravljaju operacijom potpisivanja.</param>
    ///
    /// <param name="appResult">
    ///     Objekt koji prati rad i sadrži izabrani certifikat za enkripciju ključa.</param>
    ///
    /// <param name="opResult">
    ///     Objekt koji prati operaciju i koji ažurira.</param>
    ///
    /// <returns>
    ///     Vrati potpisani podatak.</returns>
    ///
    private static byte[] EncryptData(
          byte[] data
        , AppOptions appOptions
        , Pkcs7Options? pkcs7Options
        , AppResult appResult
        , AppResult.OpResult opResult
        )
    {
        Debug.Assert( appResult.EncryptCert is not null );
        data = Pkcs7.EnvelopeData( data, appResult.EncryptCert!, appOptions.EncryptAlg, pkcs7Options );

        opResult.DataProduced = true;
        opResult.EncryptCert = appResult.EncryptCert;

        return data;
    }

    /// <summary>
    /// Snimi producirani potpisani/omotani podatak.
    /// </summary>
    ///
    /// <param name="data">
    ///     Podatak koji potpiše.</param>
    ///
    /// <param name="appOptions">
    ///     Programske opcije koje utvrđuju simetrični algoritam za šifriranje.</param>
    ///
    /// <param name="opResult">
    ///     Objekt koji prati operaciju i koji ažurira.</param>
    ///
    /// <returns>
    ///     Vrati potpisani podatak.</returns>
    ///
    private static void SaveProducedData(
          byte[] data
        , AppOptions appOptions
        , AppResult.OpResult opResult
        )
    {
        // Konstruira stazu izlazne datoteke. Nazivu ulazne datoteke doda (doda, ne zamijeni) ekstenziju izlazne
        // datoteke (započinje točkom, provjereno ranije) i izlaznu datoteku spremi u direkorij gdje je ulazna,
        // osim ako je programskom opcijom specificiran izlazni direktorij.
        var outDir = appOptions.OutDir is not null ? new DirectoryInfo(appOptions.OutDir) : opResult.InputFile.Directory;
        opResult.OutputFile = new FileInfo(
              Path.Combine( outDir.FullName, opResult.InputFile.Name ) + appOptions.Ext
            );

        // Zapiše, odnosno prepiše izlaznu datoteku.
        File.WriteAllBytes( opResult.OutputFile.FullName, data );
    }

    /// <summary>
    /// Snimi ovjereni podatak.
    /// </summary>
    ///
    /// <param name="data">
    ///     Podatak koji potpiše.</param>
    ///
    /// <param name="appOptions">
    ///     Programske opcije koje utvrđuju simetrični algoritam za šifriranje.</param>
    ///
    /// <param name="opResult">
    ///     Objekt koji prati operaciju i koji ažurira.</param>
    ///
    /// <returns>
    ///     Vrati potpisani podatak.</returns>
    ///
    private static void SaveVerifiedData(
          byte[] data
        , AppOptions appOptions
        , AppResult.OpResult opResult
        )
    {
        Debug.Assert( opResult.OutputFile is null );

        // Utvrdi ima li ulazna datoteka očekivanu ekstenziju izlazne, pa tu ekstenziju ukloni. Inače ekstenziju
        // ne ukloni.
        var outputFileName
            = string.Equals(appOptions.Ext, opResult.InputFile.Extension)
            ? Path.GetFileNameWithoutExtension( opResult.InputFile.Name )
            : opResult.InputFile.Name;

        // Konstruira stazu datoteke sa sadržanim podatkom. Spremi u direkorij gdje je ulazna datoteka,
        // osim ako je programskom opcijom specificiran izlazni direktorij.
        var outDir = appOptions.OutDir is not null ? new DirectoryInfo(appOptions.OutDir) : opResult.InputFile.Directory;
        opResult.OutputFile = new FileInfo(
              Path.Combine( outDir.FullName, outputFileName )
            );

        // Zapiše, odnosno prepiše već postojeću datoteku.
        File.WriteAllBytes( opResult.OutputFile.FullName, data );
    }

    /// <summary>
    /// Izabere certifikat za enkripciju simetričnog enkripcijskog ključa, tj. izabere primatelja omotnice, po potrebi
    /// kroz interakciju s korisnikom.
    /// </summary>
    ///
    /// <param name="appOptions">
    ///     Programske opcije.</param>
    ///
    /// <param name="appResult">
    ///     Objekt koji prati ishod rada.</param>
    ///
    /// <param name="certMngr">
    ///     Implementacija sučelja s operacijama nad certifikatima.</param>
    ///
    /// <param name="forceSilentUi">
    ///     Forsira potiskivanje sistemskog dijalog za izbor certifikata, bez obzira na <paramref
    ///     name="appOptions"/>. U tom slučaju ignorira i restrikciju po namjeni.</param>
    ///
    /// <param name="isOptional">
    ///     Neće baciti iznimke ako certifikat nije izbaran, nego vrati <see langword="null"/>.</param>
    ///
    /// <exception cref="EncryptCeriticateNotFoundException">
    ///     Certifikat za enkripciju ključa je zadan kroz <paramref name="appOptions"/>, ali nije nađen.</exception>
    ///
    /// <exception cref="EncryptCeriticateNotSelectedException">
    ///     Certifikat za enkripciju ključa nije izabran.</exception>
    ///
    public static void SelectEncryptCertificate(
          AppOptions appOptions
        , AppResult appResult
        , ICertificateManager certMngr
        , bool forceSilentUi = false
        , bool isOptional = false
        )
    {
        Debug.Assert( appResult.EncryptCert is null );

        // Je li naznačeno da se i za šifriranje koristi potpisni certifikat?
        if("*" == appOptions.EncryptCert)
        {
            // Ako je naznačeno, pa izabere potpisni certifikat ako je on već izabran. Ako nije,
            // ostaje i certifikat za šifriranje neizabran.
            appResult.EncryptCert = appResult.SignCert;
            if(appResult.EncryptCert is not null)
                return;
        }
        else appResult.EncryptCert = SelectCertificate<EncryptCeriticateNotFoundException, EncryptCeriticateNotSelectedException>(
              certMngr: certMngr
            , location: appOptions.EncryptLoc
            , includeCsp: appOptions.IncludeCsp
            , thumbprint: appOptions.EncryptCert
            , silentUi: appOptions.SilentUi || forceSilentUi
            , ignorePurpose: forceSilentUi || appOptions.IgnorePurpose
            , purpose: CertificatePurpose.Identification
            , allowInvalid: appOptions.AllowInvalid
            , isOptional: isOptional
            , title: Resources.SelectEncryptCertificateTitle
            , message: Resources.SelectEncryptCertificateMessage
            );
    }

    /// <summary>
    /// Izabere potpisni certifikat, po potrebi u interakciji s korisnikom.
    /// </summary>
    ///
    /// <param name="appOptions">
    ///     Programske opcije.</param>
    ///
    /// <param name="appResult">
    ///     Objekt koji prati ishod rada.</param>
    ///
    /// <param name="certMngr">
    ///     Implementacija sučelja s operacijama nad certifikatima.</param>
    ///
    /// <param name="forceSilentUi">
    ///     Forsira potiskivanje sistemskog dijaloga za izbor certifikata, bez obzira na <paramref
    ///     name="appOptions"/>. U tom slučaju ignorira i restrikciju po namjeni.</param>
    ///
    /// <param name="isOptional">
    ///     Neće baciti iznimke ako certifikat nije izbaran, nego vrati <see langword="null"/>.</param>
    ///
    /// <param name="canUseIdent">
    ///     Za operaciju se može koristiti i identifikacijski certifikat.</param>
    ///
    /// <exception cref="SignCeriticateNotFoundException">
    ///     Potpisni certifikat je zadan kroz <see cref="appOptions"/>, ali nije nađen.</exception>
    ///
    /// <exception cref="SignCeriticateNotSelectedException">
    ///     Korisnik je prekinuo interakciju bez izbora potpisnog certifikata..</exception>
    ///
    public static void SelectSignCertificate(
          AppOptions appOptions
        , AppResult appResult
        , ICertificateManager certMngr
        , bool forceSilentUi = false
        , bool isOptional = false
        , bool canUseIdent = false
        )
    {
        Debug.Assert( appResult.SignCert is null );
        appResult.SignCert = SelectCertificate<SignCeriticateNotFoundException, SignCeriticateNotSelectedException>(
              certMngr: certMngr
            , location: appOptions.SignLoc
            , includeCsp: appOptions.IncludeCsp
            , thumbprint: appOptions.SignCert
            , silentUi: appOptions.SilentUi || forceSilentUi
            , ignorePurpose: forceSilentUi || appOptions.IgnorePurpose
            , purpose: appOptions.PreferIdent && canUseIdent ? CertificatePurpose.Identification : CertificatePurpose.Signature
            , allowInvalid: appOptions.AllowInvalid
            , isOptional: isOptional
            , title: Resources.SelectSignCertificateTitle
            , message: Resources.SelectSignCertificateMessage
            );
    }

    /// <summary>
    /// Izabire certifikat, prema potrebi i kroz interakcijsu s korisnikom.
    /// </summary>
    ///
    /// <param name="certMngr">
    ///     Implementacija sučelja s operacijama nad certifikatima.</param>
    ///
    /// <param name="location">
    ///     Lokacija iz koje izabire certifikat.</param>
    ///
    /// <param name="includeCsp">
    ///     Hoće li u izbor iz čitača kartica uključiti i stare kartice implementirane kroz CSP?</param>
    ///
    /// <param name="thumbprint">
    ///     Digitalni otisak certifikata kojeg dohvati iz lokacije <paramref name="location"/>. Ne nađe li ga tamo, baci
    ///     iznimku <typeparamref name="NotFoundException"/>. Ako je ovo <see langword="null"/> certifikat odredi kroz
    ///     interakcijsu s korisnikom.</param>
    ///
    /// <param name="silentUi">
    ///     Spriječava interakciju s korisnikom i ako je ona bila potrebna, umjesto nje vraća <see
    ///     href="null"/>.</param>
    ///
    /// <param name="ignorePurpose">
    ///     Ignorira namjenu certifikata koji se traži i prikaže u interakciji s korisnikom sve na lokaciji raspoložive
    ///     certifikate.</param>
    ///
    /// <param name="purpose">
    ///     Namjena certifikata. Ako se ne ignorira, prikaže korisniku na izbor samo certifikate te namjene.</param>
    ///
    /// <param name="allowInvalid">
    ///     Omogući korisniku da izabere i nevaljani certifikat.Nevaljani je certifikat koji je istekao ili još nije
    ///     niti započeo.</param>
    ///
    /// <param name="isOptional">
    ///     Ne nađe li ceritifikat zadan s <paramref name="thumbprint"/> neće baciti iznimku <typeparamref
    ///     name="NotFoundException"/>, nego vrati <see langword="null"/>. Isto tako, odustane li tijekom interkacije
    ///     korisnik od izbora certifikata vrati <see langword="null"/> umjesto da baci
    ///     <typeparamref name="NotSelectedException"/>.</param>
    ///
    /// <param name="title">
    ///     Naslov dijaloškog okvira za izbor certifikata.</param>
    ///
    /// <param name="message">
    ///     Tekst opisa/upute u dijaloškom okiru za izbor certifikata.</param>
    ///
    /// <returns>
    ///     Vrati certifikat ili <see langword="null"/> ako certifikat nije izabran,</returns>
    ///
    /// <exception cref="NotFoundException">
    ///     Certifikat je zadan argumentom <paramref name="thumbprint"/>, ali nije ga bilo moguće naći. </exception>
    ///
    /// <exception cref="NotSelectedException">
    ///     Korisnik je prekinuo interakciju bez izbora certifikata.</exception>
    ///
    private static X509Certificate2? SelectCertificate<NotFoundException, NotSelectedException>(
          ICertificateManager certMngr
        , CertificateLocation location
        , bool includeCsp
        , string? thumbprint
        , bool silentUi
        , bool ignorePurpose
        , CertificatePurpose purpose
        , bool allowInvalid
        , bool isOptional
        , string title
        , string message
        )
        where NotFoundException : SignerException, new()
        where NotSelectedException : SignerException, new()
    {
        certMngr.LoadCertificates( location, includeCsp );
        if(thumbprint is not null)
        {
            var cert = certMngr.FindCertificate( thumbprint, validOnly: false );
            if(cert is not null || isOptional)
                return cert;
            throw new NotFoundException();
        }
        else if(silentUi)
            return null;
        else
        {
            var cert = certMngr.SelectCertificate(
                  ignorePurpose ? CertificatePurpose.Unspecified : purpose
                , !allowInvalid
                , title
                , message
                );
            if(cert is not null || isOptional)
                return cert;
            throw new NotSelectedException();
        }
    }

    /// <summary>
    /// Potpiše i vrati potpisani podatak ažurirajući status operacije.
    /// </summary>
    ///
    /// <param name="data">
    ///     Podatak koji potpiše.</param>
    ///
    /// <param name="appOptions">
    ///     Programske opcije.</param>
    ///
    /// <param name="pkcs7Options">
    ///     Opcije koje upravljaju operacijom potpisivanja.</param>
    ///
    /// <param name="appResult">
    ///     Objekt koji prati rad i sadrži izabrani potpisni certifikat.</param>
    ///
    /// <param name="opResult">
    ///     Objekt koji prati operaciju i koji ažurira.</param>
    ///
    /// <returns>
    ///     Vrati potpisani podatak.</returns>
    ///
    private static byte[] SignData(
          byte[] data
        , AppOptions appOptions
        , Pkcs7Options? pkcs7Options
        , AppResult appResult
        , AppResult.OpResult opResult
        )
    {
        DateTimeOffset signingTime = appOptions.SignTime ?? DateTimeOffset.Now;

        Debug.Assert( appResult.SignCert is not null );
        data = Pkcs7.SignData( data, appResult.SignCert!, signingTime, pkcs7Options );

        opResult.DataProduced = true;
        opResult.SignCert = appResult.SignCert;
        opResult.SignDateTime = signingTime.LocalDateTime;

        return data;
    }

    /// <summary>
    /// Validira format ekstenzije.
    /// </summary>
    ///
    /// <param name="ext">
    ///     Ekstenzija mora započinjati točkom, imati barem još jedan znak i ne smije sadržavati znakove nedopuštene u
    ///     nazivima datoteka (ova posljednja provjera najviše je zbog osiguravanja da korisnik ne upotrijebi
    ///     joker-znakove misleći da može specificirati klasu ekstenzija).</param>
    ///
    /// <exception cref="InvalidOutputFileExtensionException">
    ///     Ekstenzija nije ispravna.</exception>
    ///
    public static void ValidateOutputFileExtension( string ext )
    {
        if(ext.Length < 2 || !ext.StartsWith( "." ) || 0 <= ext.IndexOfAny( Path.GetInvalidFileNameChars() ))
            throw new InvalidOutputFileExtensionException( ext );
    }

    /// <summary>
    /// Ovjeri potpisani podatak ažurirajući status operacije.
    /// </summary>
    ///
    /// <param name="data">
    ///     Podatak koji verificira.</param>
    ///
    /// <param name="certificate">
    ///     Certifikat koji se koristi za dešifriranje. Nije li zadan, a podatak je šifriran, certifikat se traži u
    ///     korisnikovom spremištu certifikata.</param>
    ///
    /// <param name="pkcs7Options">
    ///     Opcije koje upravljaju operacijom potpisivanja.</param>
    ///
    /// <param name="appResult">
    ///     Objekt koji prati rad i sadrži izabrani certifikat za dekripciju ključa.</param>
    ///
    /// <param name="opResult">
    ///     Objekt koji prati operaciju i koji ažurira.</param>
    ///
    /// <returns>
    ///     Vrati ovjereni podatak.</returns>
    ///
    /// <remarks>
    /// <para>
    ///     Ako je u operaciji već ranije korišten certifikat za šifriranje</para>
    /// </remarks>
    ///
    ///
    private static byte[] VerifyData(
          byte[] data
        , X509Certificate2? certificate
        , Pkcs7Options? pkcs7Options
        , AppResult appResult
        , AppResult.OpResult opResult
        )
    {
        // Očekuje isključivo SignedData ili EnvelopedData ili EnvelopedData(SignedData) format, pa prvo otvara
        // kuvertirani SignedData podatak.
        if(Pkcs7.Oids.EnvelopedData.Value == Pkcs7.GetContentTypeOid( data ).Value)
        {
            opResult.DecryptCert = certificate;
            data = Pkcs7.OpenEnvelopedData( data, opResult.DecryptCert, pkcs7Options );
            opResult.DataDecrypted = true;
        }

        // Očekuje SignedData (kojeg ovjeri) ili sintaksno nesipravan podatak (za kojeg pretpostavi da je sadržaj).
        var dataTypeOid = Pkcs7.GetContentTypeOid(data);
        if(Pkcs7.Oids.SignedData.Value == dataTypeOid.Value)
            data = Pkcs7.VerifySignedData( data, pkcs7Options );
        else if(Pkcs7.Oids.Data.Value != dataTypeOid.Value)
            throw new WrongSignVerificationDataTypeException( dataTypeOid.Value ?? "" );

        // Vrati ovjereni podatak.
        opResult.DataVerified = true;

        return data;
    }
}

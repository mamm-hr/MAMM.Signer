# MAMM.Signer.Interop

Unutarprocesni COM poslužitelj /_in process server_/, tj. COM biblioteka koja izlaže biblioteku
[MAMM.Signer.Pkcs](../MAMM.Signer.Pkcs/README.md) kroz `IUnknown` i `IDispatch` (automatizacijsko) sučelje.

Objektna hijerarhija prati razrednu organizaciju biblioteke [MAMM.Signer.Pkcs](../MAMM.Signer.Pkcs/README.md),
preciznije organizirana je oko razreda [Pkcs7](../MAMM.Signer.Pkcs/Pkcs7.cs). Razred
[CmsMessage](../MAMM.Signer.Pkcs/CmsMessage.cs) nije izložen, ali lako se nadogradi jer biblioteka implementira sve
potrebne rekvizite. Komponenta [Certificates](./ICertificates.cs) implementira metodu za dohvat certifikata iz spremišta
korisnika ili s čitača kartica i može prikazati sistemski dijalog za izbor certifikata.

Objektna hijerarhija dana je IDL-om u datoteci [MAMM.Signer.idl](./MAMM.Signer.idl). Metode COM sučelja ukratko su
opisane anotacijama objekata i metoda u IDL datoteci, pa su opisi vidljivi u preglednicima biblioteka tipova kao što je
_Object Browser_ u IDE-u Visual Basica 6.

Objekt se gradi za .NET Framework 4.8 i .NET 10 na Windowsima za 32-bitnu AnyCPU platformu. Koristiti se može izgradnja
za prikladniji od dva kostura koja neće dovesti do konflikta pri učitavanju u procesni prostor klijenta. Prema potrebi,
izgradnja za .NET može se i spustiti na ranije verzije kostura intervencijom u projektnu datoteku.

## Registriranje

Projekt ne registira komponente automatski prilikom izgradnje. Preporučeno je registrirati ih samo na razvojnom
računalu, dok je u produkciji preporučeno koristiti aktiviranje bez registriranja /_registration-free activation_/ kako
bi korištena biblioteka mogla koegzistirati s njenim drugim instalacijama.

### .NET Framework

COM biblioteka izgrađena za .NET Framework registrira se alatom **regasm** koji se tipično nalazi u direktoriju

	C:\Windows\Microsoft.NET\Framework\v4.0.30319

Direktorij će biti u PATH varijabli okružja ako se otvori **Developer Command Prompt** Visual Studija.

#### Registriranje

	regasm /codebase MAMM.Signer.Interop.dll

Nije potrebno i nije preporučeno stvarati .tlb datoteku opcijom **/tlb** jer je projekt prilikom izgradnje proizvede iz
IDL datoteke.

#### Deregistriranje

	regasm /unregister MAMM.Signer.Interop.dll

### .NET

COM biblioteka izgrađena za .NET sadrži tehničku komponentu koja se registrira na uobičajeni način pomoću **regsvr32**
alata.

#### Registriranje

	regsvr32 MAMM.Signer.Interop.comhost.dll

##### Deregistriranje

	regsvr32 /u MAMM.Signer.Interop.comhost.dll

## Aktiviranje

### Konfiguriranje povezivanja 

#### .NET Framework

U .NET Frameworku se prilikom povezivanja /_binding_/ učitanih sklopova /_assembly_/ provjerava da snažno ime /_strong
name_/ učitanog sklopa odgovara onom s kojim je program uvezan kod izgradnje. Ovo uključuje i kompletan broj verzije.
Može doći do razlike između verzije s kojom MSBuild uvezuje izgrađeni program od verzije u NuGet paketu koja je
referencirana projektom. Nesukladnost se onda razrješava eksplicitnim preusmjeravanjem povezivanja kroz konfiguracijsku
datoteku koju se priloži uz izvršnu datoteku programa i nazove po njoj kao **Program.exe.config**. Za programe
konfiguracijska datoteka bude automatski proizvedena kod izgradnje u Visual Studiju, a za biblioteke se ona ne proizvede
automatski jer će je proizvesti izgradnja svakog programa u kojem se biblioteka koristi. U slučaju COM biblioteke proces
izgradnje klijentskoga programa ne može proizvesti konfiguracijsku datoteku, pa se kod izgradnje COM biblioteke za .NET
Framework automatska proizvodnja kofiguracijske datoteke eksplicira kroz projektne postavke.

Ovaj projekt prilikom izgradnje za .NET Framework cilj proizvede odgovarajuću konfiguracijsku datoteku kao
*MAMM.Signer.Interop.dll.config*. Kod instaliranja tu datoteku treba iskopirati uz izvršnu datoteku klijenta koji
aktivira njene COM objeke, ali ju je potrebno i preimenovati sukladno nazivu izvršne datoteke klijentskog programa: ako
je klijent *Client.exe*, datoteka se mora zvati *Client.exe.config*. Radi se o XML-datoteci i u slučaju da klijentski
program koristi i neke druge COM biblioteke i možda već ima konfiguracijsku datoteku, sadržaj ove treba umetnuti u nju.

Datoteka izgleda ovako, s time da će se brojevi verzija na koje se preusmjerava u proizvedenoj datoteci razlikovati od
ovdje prikazanih kad se korišteni NuGet paketi ažuriraju novijim verzijama:

```xml
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Security.Cryptography.Cng" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-5.0.0.0" newVersion="5.0.0.0" />
      </dependentAssembly>
    </assemblyBinding>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Security.Cryptography.Pkcs" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-10.0.0.7" newVersion="10.0.0.7" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
```

Ova konfiguracija će, međutim, funkcionirati samo ako se datoteke COM biblioteke stave neposredno u instalacijski
direktorij. Da bi se mogle staviti u poddirektorij, recimo *lib\net48*, potrebno je elementom `probing` specificirati
poddirektorije u kojima će CLR tražiti datoteke. U tom slučaju u konfiguracijsku datoteku treba dodati i unos za glavnu
izvršnu datoteku biblioteke, da CLR i nju može pronaći prilikom aktivacije:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="MAMM.Signer.Interop" culture="neutral" />
      </dependentAssembly>
      <probing privatePath="lib\net48"/>
    </assemblyBinding>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Security.Cryptography.Pkcs" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-10.0.0.7" newVersion="10.0.0.7" />
      </dependentAssembly>
      <probing privatePath="lib\net48"/>
    </assemblyBinding>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="System.Security.Cryptography.Cng" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-5.0.0.0" newVersion="5.0.0.0" />
      </dependentAssembly>
      <probing privatePath="lib\net48"/>
    </assemblyBinding>
  </runtime>
</configuration>
```

Ogledna konfiguracija klijenta [MammSignerVb6.exe.config](../MAMM.Signer.Vb6/bin/MammSignerVb6.exe.config) dana je u
projektu demonstracijskog klijenta i dodatno u odnosu na niže tumačenje opisana u komentarima sadržaja datoteke.

Problemi povezivanja u .NET Frameworku analizirati se mogu alatom **fuslogvw.exe** (v. niže).

Izvori:

- https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/may/understanding-the-clr-binder
- https://learn.microsoft.com/en-us/windows/win32/sbscs/application-configuration-files
- https://learn.microsoft.com/en-us/dotnet/framework/configure-apps/file-schema/runtime/assemblybinding-element-for-runtime
- https://learn.microsoft.com/en-us/dotnet/framework/configure-apps/file-schema/runtime/probing-element

#### .NET

Izgradnja COM biblioteke za .NET automatski proizvede datoteku konfiguracije povezivanja naziva
*MAMM.Signer.Interop.deps.json* i nju se instalira uz izvršnu datoteku biblioteke *MAMM.Signer.Interop.dll* odakle se
prilikom povezivanja i učitava. Nisu potrebni nikakvi drugi postupci u sklopu instaliranja.

### Aktiviranje bez registriranja 

Da bi se objekti COM biblioteke mogli aktivirati bez registriranja, uz izvršnu datoteku klijenta isporuči se i datoteka
manifesta koju treba nazvati po izvršnoj datoteci programa klijenta kao *Client.exe.manifest*. Iz nje se onda kroz
`dependentAssembly` element referencira manifest COM biblioteke koji opisuje koje se komponente nalaze u biblioteci i
gdje se nalazi izvršna datoteka biblioteka. Mainfest COM biblioteke se instalira uz datoteku manifesta klijenta. 

Ogledni manifest klijenta [MammSignerVb6.exe.manifest](../MAMM.Signer.Vb6/bin/MammSignerVb6.exe.manifest) dan je u
projektu demonstracijskog klijenta i detaljno opisan u komentarima sadržaja datoteke.

Problemi s aktivacijom analizirati se mogu alatom **sxstrace.exe** (v. niže) i uvidom u Windowsov aplikacijski dnevnik
događaja kroz _Event Viewer_.

Izvori:

- https://learn.microsoft.com/en-us/windows/win32/sbscs/application-manifests

#### .NET Framework

Objekti COM biblioteke izgrađene za .NET Framework aktiviraju se kroz CLR aktivacijski model .NET Frameworka. Ne postoji
alat za automatsko generiranje datoteke manifesta COM biblioteke, već je ona uključena u ovaj projekt kao datoteka
[MAMM.Signer.Interop.mainfest](./MAMM.Signer.Interop.manifest), a ručno je treba ažurirati promijene li se bitni
identitetski elementi, tj. naziv sklopa, verija, programski identifikator, itd. Sadržaj datoteke odgovara ovdje
prikazanom:

```xml
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0">
	<assemblyIdentity name="MAMM.Signer.Interop" version="1.0.0.0"/>
	<clrClass clsid="{1efda486-414c-4273-b81e-a9fc8c715432}" progid="MAMM.Signer.Application" threadingModel="Both" name="MAMM.Signer.Interop.CoApplication" runtimeVersion="v4.0.30319"/>
</assembly>
```

Vrijednost atributa `name` elementa `assemblyIdentity` mora odgovarati nazivu DLL-a sklopa i datoteke mainfesta,
dakle u ovom slučaju se datoteka manifesta s tim sadržajem mora zvati *MAMM.Signer.Interop.manifest*. Atributi `clsid`,
`progid`, i `name` moraju odgovarati implementaciji u razredu [CoApplication](.\CoApplication.cs), odnosno njegovim
atributima `Guid`, `ProgId` i punom nazivu razreda implementacijskog kojeg CLR mora instancionirati.

Ogledni manifest COM biblioteke za .NET Framework izgradnju
[MAMM.Signer.Interop.manifest](../MAMM.Signer.Vb6/bin/MAMM.Signer.Interop.manifest) dan je u projektu demonstracijskog
klijenta.

Izvori:

- https://learn.microsoft.com/en-us/dotnet/framework/interop/configure-net-framework-based-com-components-for-reg
- https://learn.microsoft.com/en-us/windows/win32/sbscs/assembly-manifests
- https://learn.microsoft.com/en-us/windows/win32/sbscs/supported-microsoft-side-by-side-assemblies
- https://learn.microsoft.com/en-us/windows/win32/sbscs/side-by-side-assembly-development-tools
- https://stackoverflow.com/questions/42858409/in-need-of-simple-and-surely-working-demo-of-registration-free-com-example-with
- https://stackoverflow.com/questions/74758743/com-reg-free-and-net5-6-7-version-is-it-possible


#### .NET

Objekti COM biblioteke izgrađene za .NET aktiviraju se kroz _Side-by-Side_ (SxS) aktivacijski sustav kao i bilo koji
drugi COM objekti pošto se pri izgradnji proizvede nativni COM poslužitelj .comhost.dll.

Projekt proizvede datoteku manifesta biblioteke *MAMM.Signer.Interop.X.manifest* automatski prilikom izgradnje. Sadržaj
datoteke odgovara ovdje prikazanom:

```xml
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <assemblyIdentity type="win32" name="MAMM.Signer.Interop.X" version="1.0.0.0" />
  <file name="MAMM.Signer.Interop.comhost.dll">
    <comClass clsid="{1efda486-414c-4273-b81e-a9fc8c715432}" threadingModel="Both" progid="MAMM.Signer.Application" />
  </file>
</assembly>
```

Vrijednost atributa `name` elementa `assemblyIdentity` može se proizvoljno odrediti, ali mora biti ista kao naziv
datoteke mainfesta komponente, dakle u slučaju iz gornjeg primjera se datoteka manifesta s tim sadržajem mora zvati
*MAMM.Signer.Interop.X.manifest*. Atributi `clsid` i `progid` moraju odgovarati implementaciji u razredu
[CoApplication](.\CoApplication.cs), odnosno njegovim atributima `Guid` i `ProgId`.

Instaliraju li se datoteke COM biblioteke u poddirektorij instalacijskog direktorija programa klijenta (što je uredniji
razmještaj) onda se atribut `name` elementa `file` shodno ažurira. Primjerice, ovdje je dan element `file` ako je
biblioteka instalirana u poddirektorij **libs\pkcs7**:

```xml
  ...
  <file name="lib\net10\MAMM.Signer.Interop.comhost.dll">
  ...
```

Ogledni manifest COM biblioteke za .NET izgradnju
[MammSignerLibNet10.manifest](../MAMM.Signer.Vb6/bin/MammSignerLibNet10.manifest) dan je u projektu demonstracijskog
klijenta, a namjerno je imenovan proizvoljno.

# fuslogvw.exe

Program `fuslogvw.exe` nalazi se tipično u direktoriju:
	
	C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools

dok se 64-bitna verzija nalazi u njegovom poddirektoriju `x64`. Pokrenuti treba odgovarajuću verziju pod
**administratorskim** ovlastima (program koji se dijagnosticira može se pokrenuti pod bilo kojim drugim računom).

Kad se pokrene, kroz gump `Settings..` izabrati `Log bind failures to disk`, `Enable custom log path` i upisati
proizvoljni direktorij pod `Custom log path`.

Nakon što se pojavi pogreška učitavanja ili povezivanja na .NET Framework sklop gumbom `Refresh` osvježiti prikaz i
dvokliknuti stavku odnosnu na sklop koji se ne učitava da se otvori izvještaj. 

# sxstrace.exe

Pokrenuti praćenje aktivacije:

    sxstrace trace -logfile:sxstrace.etl -nostop

Nakon što se problem desti, prekinuti praćenje i pretvoriti snimljeni dnevnik u čitljivi oblik:

    sxstrace stoptrace
    sxstrace parse -logfile:sxstrace.etl -outfile:sxstrace.txt

## Ažuriranje manifesta

Datoteke manifesta izvršnih programa operacijski sustav kopira se u međuspremnik /_cache_/ koji se nalazi u radnoj
memoriji računala i kod aktivacije objekata koristi se tako predmemorirana datoteka. Ona se osvježi tek kad operacijski
sustav detektira da je došlo do promjene izvršne datoteke, što zaključuje temeljem pune staze do izvršne datoteke i
datuma i vremena njene zadnje izmjene. Zbog toga ažuriranje datoteke manifesta može biti bez učinka. Da se prisili
osvježavanje predmemorije potrebno je ili ponovo pokrenuti računalo kako bi se manifest iznova predmemorirao ili
promijeniti vrijeme zadnje izmjene izvršne datoteke njenim ponovnim kompajliranjem ili pomoću alata poput `touch`.

U Windowsima, vrijeme zadnje izmjene datoteke može se aktualizirati slijedećom naredbom:

    copy /b filename.ext +,,

Predmemoriranje se može isključiti, što nije preporučljivo činiti u produkcijkom okružju. U ključu:

    HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options

treba osigurati da postoji unos `DevOverrideEnable` tipa `REG_DWORD` i njegova vrijednost različita od `0`. Aktiviranje
ove opcije može imati utjecaj na rad drugih programa i treba je normalno držati isključenom.

Izvori:

- https://stackoverflow.com/questions/741726/diagnosing-windows-application-manifests
- https://web.archive.org/web/20140423082154/http://blogs.msdn.com/b/junfeng/archive/2006/10/25/touch-the-exe-after-you-added-a-manifest-for-it-in-vista.aspx
- https://web.archive.org/web/20160702070403/https://blogs.msdn.microsoft.com/junfeng/2007/10/01/windows-vista-sxs-activation-context-cache/
- https://web.archive.org/web/20160701101333/https://blogs.msdn.microsoft.com/junfeng/2006/01/24/dotlocal-local-dll-redirection/

---

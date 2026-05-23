# MAMM.Signer.Vb6

Projekt koji demonstrira korištenje [MAMM.Signer.Interop](../MAMM.Signer.Interop/README.md) COM biblioteke.

## Priprema za demonstracijskog klijenta

Po izgradnji projekta COM biblioteke (MAMM.Signer.Interop), u direktoriju ovog projekta, uz datoteku projekta
*MammSignerVb6.vbp* pojavi se *.tlb* datoteka COM bibliteke naziva *MAMM.Signer.tlb*. Nema li je, pronađe se u izlaznom
direktoriju projekta COM biblioteke. Također se u poddirektoriju bin/lib pojave sve producirane datoteke COM biblioteke
u NET Framework 4.8 i .NET 10 izgradnjama locirane tako prikladno za aktivaciju bez registracije.

Projektna datoteka *MammSignerVb6.vbp* referencira i očekuje *MAMM.Signer.tlb* datoteku u istom direktoriju. Po
otvaranju projekta u IDE-u vidi se referenca za MAMM.Signer.Interop biblioteku (Project > References ili F2). Nema li
je, referencira se traženjem .tlb datoteke kroz Project > References > Browse...

Igradi se projekt u *.\bin* poddirektorij u kojem se već nalaze prikladni manifesti i konfiguracije. Izvršna datoteka
klijenta mora se zvati *MammSignerVb6.exe*. Njen manifest je podešen za aktivaciju objekata u .NET Framework 4.8
implementaciji biblioteke, a lako se promijeni na .NET 10.

### Aktivacija s registacijom

Registrira se komponente MAMM.Signer.Interop bibliteke. Za implementaciju komponenti u .NET 10 izgradnji registrira se
iz direktorija te izgradnje:

	regsvr32 MAMM.Signer.Interop.comhost.dll

Alternativno, za implementaciju .NET Framework 4.8 izgradnji registrira se iz direktorija te izgradnje:

	regasm /codebase MAMM.Signer.Interop.dll

Najbolje je u potonjem slučaju registrirati iz Developer Command Prompta za Visual Studio da se *regasm* pojavi u PATH
varijabli okružja. Uobičajeno se nalazi u *C:\Windows\Microsoft.NET\Framework\v4.0.30319*.

Time se testirati klijenta može s registriranim komponentama. Da bi se testirala aktivacija bez registracije, prikladno
je komponente prvo deregistrirati:

	regsvr32 /u MAMM.Signer.Interop.comhost.dll

odnosno

	regasm /unregister MAMM.Signer.Interop.dll

Potom se u poddirektorij *.\bin\lib\net10.0-windows* kopira sve datoteke MAMM.Signer.Interop biblioteke izgrađene za
.NET 10, a u poddirektorij *.\bin\lib\net48* sve datoteke izgrađene za .NET Framework 4.8.

Datoteku mainfesta klijenta *MammSignerVb6.exe.manifest* ažurira se vezom na manifest komponente jedne ili druge
izgradnje. Uputa je dana u samoj datoteci, zamjeni se naprosto odgovarajući `assemblyIdentity` element jednim ili drugim
danim u njegovom komentaru.

# Priprema za vlastitog klijenta 

U poddirektoriju *.\bin* nalaze se datoteke konfiguracije i manifesta nužne za aktivaciju objekata
MAMM.Signer.Interop biblioteke. Svrha datoteka opisana je u [README.md](../MAMM.Signer.Interop/README.md) datoteci
biblioteke, a niže su potrebne datoteke ukratko opisane.

U ostatku teksta pretpostavi se da je izvršna datoteka vlastitog klijenta *Client.exe*.

## Aktivacija s registracijom

### .NET 10

Koristi li se .NET 10 implementacija, nikakve dodatne datoteke nisu potrebne.

### .NET Framework

Koristli se .NET Framework implementacija, potrebna je *Client.exe.config* konfiguracijska datoteka. Može se naprosto
uzeti priloženi *MammSignerVb6.exe.config* demonstracijskog klijenta i preimenovati ga u *Client.exe.config*. Iz
datoteke se izbaci sve `probing` elemente, kao i `assemblyBinding` element koji se odnosi na samu izvršnu datoteku
biblioteke *MAMM.Signer.Interop*, pošto nisu potrebni.

## Aktivacija bez registracije

### .NET 10

Potrebna je datoteka manifesta komponente, a iskoristiti se može priložena datoteka *MammSignerLibNet10.manifest*.
Datoteku se može imenovati proizvoljno, ali isto se ime onda mora koristiti u `name` atributu `assemblyIdentity`
elementa manifesta klijenta iz kojeg se manifest komponente referencira. 

U datoteci manifesta komponente kroz `name` atribut elementa `file` uputiti SxS podsustav gdje se nalazi izvršna
datoteka biblioteke upisom njene staze relativno u odnosu na instalacijski direktorij izvršne datoteke *Client.exe*.

### .NET Framework

Slično kao i kod aktivacije uz registraciju, potreban je *Client.exe.config*, ali se u ovom slučaju `probing` elemente i
`assemblyBinding` element odnosan na izvršnu datoteku biblioteke *MAMM.Signer.Interop* izbaciti može samo ako se
datoteke komponente smjeste u isti direktorij u kojem je i *Client.exe* ili poddirektorij nazvan po komponenti, tj.
*MAMM.Signer.Interop*. Inače ove elemente treba zadržati, a kroz `probing` CLR-u indicirati u kojem se poddirektoriju
instalacije nalaze datoteke komponente.

Potrebna je i datoteka manifesta komponente, a iskoristiti se može priložena datoteka *MAMM.Signer.Interop.manifest* koja
se mora baš tako zvati, tj. zvati se mora isto kao izvršna datoteka biblioteke. 

## Aktivacija iz IDE-a Visual Basica

Najbolje je koristiti aktivaciju s registracijom. U tom slučaju će .NET 10 komponenta biti uredno dostupna i prilikom
izvršavanja programa u IDE-u. Za .NET Framework komponentu dovoljno je uz VB6.EXE staviti VB6.EXE.config datoteku
koja se pripremi na isti način kako je opisano za pripremu vlastitog klijenta. Još jednostavnije, naprosto se iskoristi
u *.\bin* poddirektoriju priloženu datoteku VB6.EXE.config.

Visual Basic 6 instaliran je tipično u *C:\Program Files (x86)\Microsoft Visual Studio\VB98*.

## Popis datoteka

**[MammSignerVb6.exe.manifest](./bin/MammSignerVb6.exe.manifest)**

- Manifest klijenta je potreban kad se koristi aktivacija COM objekata bez registracije. 
- Manifest klijenta naprosto povezuje datoteku manifesta COM komponente.
- Potrebno je modificirati manifest ovisno o izgradnji koju se koristi.

**[MammSignerLibNet10.manifest](./bin/MammSignerLibNet10.manifest)**

- Manifest .NET 10 izgradnje komponente. 
- Potreban kad se koristi aktivacija COM objekata bez registracije.
- Sadrži relativnu stazu do izvršne datoteke komponente.

**[MAMM.Signer.Interop.manifest](./bin/MAMM.Signer.Interop.manifest)**

- Manifest .NET Framework izgradnje komponente.
- Potreban kad se koristi aktivacija COM objekata bez registracije.

**[MammSignerVb6.exe.config](./bin/MammSignerVb6.exe.config)**

- Konfiguracija povezivanja za .NET Framework izgradnju komponente.
- Uvijek potrebna datoteka.
- Sadrži relativnu stazu do direktorija s datotekama komponente.

**[VB6.EXE.config](./bin/VB6.EXE.config)**

- Konfiguracija povezivanja za VB6 IDE kad se koristi .NET Framework izgradnju komponente.
- Uvijek potrebna datoteka da se objekti komponente mogu aktivirati iz VB6 IDE-a.
- Koristiti uz registraciju komponente.

---

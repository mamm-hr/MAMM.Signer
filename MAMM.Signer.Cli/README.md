# MAMM.Signer.Cli

Program naredbenog retka (CLI) kojim se mogu izrađivati datoteke po specifikaciji PKCS #7 / CMS s potpisanim i/ili
kuvertiranim (šifriranim) dokumentima, rađen s ciljem da zamijeni AKDSHCard Data Signer and Verifier. Program može
također otvarati kuverte (dešifrirati) i ovjeravati potpise u takvim datotekama uz izvlačenje u njima sadržanog
dokumenta.

Naredbeni redak je oblika:

	MAMM.Signer.Cli specifikacija opcije

Ovdje `specifikacija` stoji za specifikaciju ulaznih datoteka, tj. dokumenata koji se potpisuju i/ili kuvertiraju,
odnosno datoteka s kuvertiranim i/ili potpisanim dokumentima. Specifikacija može biti:

1.	Ulazna datoteka.
1.	Uzorak za traženje korištenjem zamjenskih znakova (* i ?) u nazivu datoteke.
1.	Datoteka s popisom ulaznih datoteka.

U sva tri slučaja moguće je direktorij u stazi specificirati relativno ili apsolutno, a relativna se staza razrješava u
odnosu na tekući direktorij programa.

Navodi li se datoteka s popisom ulaznih datoteka, svaka ulazna datoteka treba biti navedena u zasebnom retku.
Specifikacija se interpretira kao datoteka s popisom ako je navedena i opcija `/specList`.

## Potpisivanje i/ili kuvertiranje

Program za svaku ulaznu datoteku dokumenta generira izlaznu datoteku sadržaja u CMS, tj. PKCS #7 sinatksi. Sadržaj bude
`SignedData`, `EnvelopedData` ili `EnvelopedData(SignedData)` tipa prema PKCS #7 specifikaciji i uključuje i sadržaj
dokumenta. U `SignedData` podatak program uvijek stavi datum i vrijeme potpisivanja kao potpisani atribut, te sve
certifikate iz lanca povjerenja potpisnog certifikata osim korijenskog (da bi drugi certifikati osim potpisnog doista
bili stavljeni, moraju biti dostupni, inače će biti izostavljeni).

Izlaznu datoteku program smjesti u isti direktorij iz kojeg očita ulaznu, osim ako programskom opcijom nije zadan
izlazni direktorij. Izlaznu datoteku imenuje cijelim imenom ulazne datoteke (uključujući i njenu ekstenziju) na koju
još doda svoju ekstenziju (prešutno `.p7m`). Već postojeću izlaznu datoteku bezuvjetno prepiše.

Ako se dokumenti potpisuju, zadati ili izabrati treba potpisni certifikat. Potpisni certifikat mora imati privatni
ključ. Kuvertiraju li se dokumenti, zadati ili izabrati treba certifikat primatelja. Certifikat primatelja ne treba
imati privatni ključ, ali implementacija mora podržavati razmjenu simetričnih ključeva za šifriranje. Certifikati se
zadaju naredbenim retkom ili izabiru kroz sistemski dijalog. Sistemski dijaloški okvir za izbor certifikata program
pokaže ako potrebni certifikat nije zadan naredbenim retkom ili ako jest zadan, ali nije pronađen u spremištu koje
program pregledava.

Program prešutno pregledava, odnosno prikazuje na popisu za izbor certifikate iz spremišta osobnih certifikata korisnika
pod čijim se računom program izvršava. Programskim opcijama može se program usmjeriti da koristi spremište osobnih
certifikata lokalnog računala ili da izravno koristi certifikate na trenutno u lokalnom računalu prisutnim
kriptografskim uređajima (na pametnim karticama umetnutima u spojene čitače, na umetnutim USB tokenima, itd.).

## Otvaranje i ovjeravanje

Program očekuje da je datoteka koju otvara i ovjerava u CMS, tj. PKCS #7 sinatksi i sadrži podatak tipa `SignedData`,
`EnvelopedData` ili `EnvelopedData(SignedData)` tipa prema PKCS #7 specifikaciji.

Da bi program otvorio kuvertu (dešifrirao datoteku), u spremištu osobnih certifikata korisnika pod čijim se računom
program izvršava dostupan mora biti certifikat jednog od primatelja i taj certifikat mora imati privatni ključ. 

Za ovjeravanje potpisa nužno je i dovoljno da u datoteku bude uključen potpisni certifikat s javnim ključem. Program
ovjeri potpis sadržanog dokumenta i svih potpisanih atributa. Uz to, program provjeri i valjanost u datoteku uključenih
certifikata, ali da bi provjera uspjela, mora se raditi o propisno izdanim certifikatima koji uključuju i adrese za
dohvat liste povučenih certifikata. Ako to nije slučaj, program se može kroz opcije uputiti da ne radi provjeru lanca
povjerenja.

## Programske opcije

Nazivi programskih opcija započinju znakom kose crte (`/`). Opcije koje imaju vrijednosti se razmakom odvajaju od svoje
vrijednosti, npr. `/Opcija Vrijednost`. Pisana i tiskana slova se ne razlikuju u nazivima opcija. Vrijednosti koje
sadrže razmake se stave u dvostruke navodnike (`"`).

Programske se opcije dijele u izvršne opcije i opcije za usmjeravanje rada PKCS #7 operacija. 

### Izvršne opcije

#### /allowInvalid

Navede li se ovaj prekidač, prikaže li program popis certifikata, on će uključiti i nevaljane (npr. istekle)
certifikate.

#### /encrypt

Navede li se ovaj prekidač, program kuvertira dokument, stvarajući tako datoteku PKCS #7 tipa `EnvelopedData`, odnosno
kombinacije tipova `EnvelopedData(SignedData)` ako se dokument i potpisuje. 

#### /encryptAlg:[value|friendlyName]

OID ili dobro poznato ime simetričnog algoritma za šifriranje. Ovu opcija program ignorira ako nije naveden prekidač
**/encrypt**. Ako je prekidač naveden, a ova opcija ispuštena, pokuša prešutni algoritam određen .NET implementacijom
koja je za .NET Framework od v. 4.8 i .NET od v. 4.6.0 NuGet paketa AES-256, dok ranije inačice kostura koriste
DES3-EDE.

Primjeri:

	/encryptAlg:value 1.2.840.113549.3.4
	/encryptAlg:friendlyName rc4

#### /encryptCert

Digitalni otisak /_thumbnail_/ certifikata primatelja. Ovu opciju program ignorira ako nije naveden prekidač
**/encrypt**. Ako je naveden prekidač **/encrypt**, a ova opcija ispuštena, program pokaže sistemski dijaloški okvir s
popisom certifikata za izbor primatelja.

Primjer:

	/encryptCert 186adbc283ccd93a4900c635db3740b3048c023d

#### /encryptLoc

Lokacija u kojoj program potraži certifikat primatelja zadan opcijom **/encryptCert**, odnosno iz koje prikaže
certifikat kad se izabire sistemskim dijalogom. Moguće vrijednosti popisane su niže u tablici lokacija certifikata.

Primjer:

	/encryptLoc SmartCardReaders

#### /ext

Ekstenzija (s točkom) koju program doda na puno ime (uključujući i ekstenziju) ulazne datoteke dokumenta kojeg
potpiše/kuvertira, odnosno koju ukloni iz imena ulazne datoteke kako bi imenovao datoteku dokumenta koji iz nje izvadi.
Prešutna ekstezija je `.p7m` jer program prilikom potpisivanja/kuvertiranja u izlaznu datoteku uključi sadržaj
dokumenta, a ovom se opcijom ona može promijeniti u, recimo, `.p7s` ako se preferira ta ekstenzija.

Primjer:

	/ext .p7s

#### /ignorePurpose

Navede li se ovaj prekidač, prikaže li program popis certifikata, on će sadržavati sve dostupne certifikate, bez obzira
na njihovu namjenu. Bez ovog prekidača, popis sadrži isključivo certifikate namijenjene potpisivanju kad se izabire
potpisni certifikat, odnosno isključivo certifikate namijenjene identifikaciji kad se izabire certifikat primatelja.

#### /preferIdent

Ovaj prekidač se ignorira ako je zadan **/ignorePurpose**. U suprotnom, ako je zadan, prikaže li program popis
certifikata koji bi trebao sadržavati potpisne certifikate, a za operaciju se može koristiti i identifikacijski,
prikazat će u popisu identifikacijske umjesto potpisnih certifikata.

#### /includeCsp

Navede li se ovaj prekidač, uključi u pregled ili traženje certifikata na kriptografskim uređajima i certifikate
dostupne isključivo kroz CSP /_Cryptographic Service Provider_/. Ovaj je prekidač potrebno zadati da dostupne postanu i
starije implementacije modula kriptografskih uređaja, poput HZZO-ovih "plavih" kartica zdravstvenih radnika. Prekidač
ima svrhu kad izvršna opcija **/encryptLoc**, odnosno **/signLoc** ima vrijednost `SmartCardReaders`, a inače se
ignorira, tj. svi su certifikati dostupni bez ovog prekidača kad program koristi korisnikovo spremište certifikata ili
spremište lokalnog računala.

#### /specList

Navede li se ovaj prekidač, program očekuje datoteku s popisom ulaznih datoteka kao prvi argument.

#### /outDir

Direktorij u koji program zapiše izlazne datoteke. Ako se ova opcija ne navede, izlaznu datoteku zapiše u direktorij iz
kojeg je očitana ulazna. Ako se opcija navede, a ulazne su datoteke specificirane popisom i nalaze se u različitim
direktorijima, sve će izlazne datoteke biti zapisane u isti direktorij što će dovesti do prepisivanja različitih
izlaznih datoteka koje nose iste nazive, a smještene su u različitim direktorijima.

Primjer:

	/outDir "C:\Potpisane datoteke"

#### /sign

Navede li se ovaj prekidač, program potpiše dokument, stvarajući tako datoteku PKCS #7 tipa `SignedData`, odnosno
kombinacije tipova `EnvelopedData(SignedData)` ako se dokument i kuvertira. 

#### /signCert

Digitalni otisak /_thumbnail_/ potpisnog certifikata. Ovu opciju program ignorira ako nije naveden prekidač
**/sign**. Ako je prekidač naveden, a ova opcija ispuštena, program pokaže sistemski dijaloški okvir s popisom
certifikata za izbor.

Primjer:

	/signCert 186adbc283ccd93a4900c635db3740b3048c023d

#### /signLoc

Lokacija u kojoj program potraži potpisni certifikat zadan opcijom **/signCert**, odnosno iz koje prikaže certifikat
kad se izabire sistemskim dijalogom. Moguće vrijednosti popisane su niže u tablici lokacija certifikata.

Primjer:

	/signLoc SmartCardReaders

#### /signTime

Datum i vrijeme potpisivanja. Ovu opciju program ignorira ako nije naveden prekidač **/sign**. Ako je prekidač naveden,
ova opcija ispuštena, kao datum i vrijeme potpisivanja program uzme trenutno sistemsko vrijeme računala na kojem se
izvršava. Vrijednost se zadaje u [ISO 8601](https://en.wikipedia.org/wiki/ISO_8601) formatu.

Primjeri:

	/signTime 2026-12-01T01:02:03
	/signTime 2026-12-01T01:02:03Z
	/signTime 2026-12-01T01:02:03+0200
	/signTime 2026-12-01T01:02:03+02:00
	/signTime 01:02:03
	/signTime 2026-12-01
	/signTime "2026-12-01 01:02:03"

#### /silentUi

Navede li se ovaj prekidač, program ne prikaže sistemski dijaloški okvir za izbor certifikata. U tom slučaju potrebni se
certifikati moraju zadati kroz izvršne opcije programa **/encryptCert**, odnosno **/signCert** i program ih doista mora
pronaći u spremištima koje pregleda.

#### /verify

Navede li se ovaj prekidač, program vrši otvaranje kuverte (dešifriranje) i ovjeravanje potpisa. Ako je uz ovaj zadan i
prekidač **/encrypt** i/ili prekidač **/sign** program obrađuje upravo producirane izlazne datoteke, a ako je zadan samo
ovaj prekidač, onda obrađuje ulazne datoteke.

Program očekuje da su datoteke koje obrađuje u CMS, tj. PKCS #7 sintaksi i da sadrže podatak tipa `SignedData`,
`EnvelopedData` ili `EnvelopedData(SignedData)`, tj. da sadrže potpisani, kuvertirani ili kuvertirani potpisani
dokument.

Ako ulazna datoteka sadrži kuvertu, tj. `EnvelopedData` ili `EnvelopedData(SignedData)` tip podatka, program dešifrira
sadržaj certifikatom primatelja kojeg pronađe u spremištu osobnih certifikata korisnika pod čijim se računom program
izvršava. Tijekom dešifriranja može biti pokazano korisničko sučelje za unos PIN-a, a operacija može rezultati
neuspjehom ako je proces u stanju u kojem se sučelje ne smije prikazivati (npr. izvršava se po računom koji nema radnu
površinu). Program nema utjecaja, niti se opcijama programa može utjecati na prikazivanje sučelja za unos PIN-a.

Ako program obrađuje svoje izlazne datoteke, obriše svaku izlaznu datoteku za koju ova obrada ne završi uspješno. Ako
obrađuje ulazne datoteke, onda su izlazne datoteke u ulaznim datotekama sadržani dokumenti.

### Opcije za usmjeravanje rada PKCS #7 operacija

#### /defaultDigestAlgorithms:rsaCsp:[value|friendlyName]

OID ili dobro poznato ime algoritma digitalnog sažetka koji program koristi kad potpisuje RSA privatnim ključem kroz CSP
implementaciju koja **nije** AKDSHCard CSP. Prešutno koristi algoritam koji je pretpostavljan od .NET-a, a u vrijeme
pisanja to je SHA-256.

Primjeri:

	/defaultDigestAlgorithms:rsaCsp:value 1.3.14.3.2.26
	/defaultDigestAlgorithms:rsaCsp:friendlyName sha1

#### /defaultDigestAlgorithms:rsaKsp:[value|friendlyName]

OID ili dobro poznato ime algoritma digitalnog sažetka koji program treba koristiti kad se potpisuje RSA privatnim
ključem kroz modernu KSP implementaciju. Prešutno koristi algoritam koji je pretpostavljan od .NET-a, a u vrijeme
pisanja to je SHA-256.

Primjeri:

	/defaultDigestAlgorithms:rsaKsp:value 2.16.840.1.101.3.4.2.3
	/defaultDigestAlgorithms:rsaKsp:friendlyName sha512

#### /defaultDigestAlgorithms:ecdsa256:[value|friendlyName]

OID ili dobro poznato ime algoritma digitalnog sažetka kad se potpisuje ECDSA privatnim ključem na krivulji P-256.
Prešutno koristi koristi SHA-256.

	/defaultDigestAlgorithms:ecdsa256:value 2.16.840.1.101.3.4.2.1
	/defaultDigestAlgorithms:ecdsa256:friendlyName sha256

#### /defaultDigestAlgorithms:ecdsa384:[value|friendlyName]

OID ili dobro poznato ime algoritma digitalnog sažetka kad se potpisuje ECDSA privatnim ključem na krivulji P-384.
Prešutno koristi koristi SHA-384.

	/defaultDigestAlgorithms:ecdsa256:value 2.16.840.1.101.3.4.2.2
	/defaultDigestAlgorithms:ecdsa256:friendlyName sha384

#### /defaultDigestAlgorithms:ecdsa521:[value|friendlyName]

OID ili dobro poznato ime algoritma digitalnog sažetka kad se potpisuje ECDSA privatnim ključem na krivulji P-521.
Prešutno koristi koristi SHA-512.

	/defaultDigestAlgorithms:rsaKsp:value 2.16.840.1.101.3.4.2.3
	/defaultDigestAlgorithms:rsaKsp:friendlyName sha512

---

# MAMM.Signer

Ovaj repozitorij sadrži biblioteke i prateće programe za izradu .p7m (odnosno .p7s) datoteka potpisivanjem /_signing_/ i
kuvertiranjem /_enveloping_/ (tj. šifriranjem, kriptiranjem) podataka po PKCS #7, odnosno CMS specifikaciji korištenjem
.NET razreda `SignedCms` i `EnvelopedCms`, a u svrhu zamjene programa AKDSHCard Data Signer and Verifier kojim se
potpisuju datoteke za slanje na HZZO-ov portal za razmjenu datoteka i podataka.

Nove "bijele" kartice zdravstvenih radnika podržavaju algoritam za potpisivanje sadržaja, ali ne podržavaju algoritam za
razmjenu simetričnog ključa za šifriranje, zbog čega se mogu koristiti za potpisivanje (generiranje PKCS #7 `SignedData`
tipa podatka), ali ne i za šifriranje sadržaja (generiranje PKCS #7 `EnvelopedData` tipa podatka). To znači da
funkcionalnost šifriranja AKDSHCard Data Signer and Verifier programa principijelno nije moguća s novim karticama.

## Projekti

* [MAMM.Signer.Pkcs](./MAMM.Signer.Pkcs/README.md) je biblioteka za potpisivanje i kuvertiranje podataka ciljana na .NET
Standard 2.0 specifikaciju.

* [MAMM.Signer.Interop](./MAMM.Signer.Interop/README.md) je COM biblioteka (_in process server_) koja izlaže
MAMM.Signer.Pkcs biblioteku COM automatizaciji. Projekt gradi biblioteku za .NET Framework 4.8 i .NET 10 kosture.

* [MAMM.Signer.Cli](./MAMM.Signer.Cli/README.md) je program naredbenog retka (CLI) za potpisivanje i kuvertiranje
datoteka, funkcionalnosti slične programu AKDSHCard Data Signer and Verifier, za što koristi biblioteku
MAMM.Signer.Pkcs. Projekt gradi program za .NET 10 kostur.

* [MAMM.Signer.Gui](./MAMM.Signer.Gui/README.md) je Windows Forms program (GUI) za potpisivanje i kuvertiranje
datoteka, funkcionalnosti slične programu AKDSHCard Data Signer and Verifier, za što koristi biblioteku
MAMM.Signer.Pkcs. Projekt gradi program za .NET Framework 4.8 kostur.

* [MAMM.Signer.Core](./MAMM.Signer.Core/README.md) je biblioteka koja sadrži središnju funkcionalnost programa
MAMM.Signer.Cli i MAMM.Signer.Gui, ciljana na .NET Standard 2.0 specifikaciju.

* [MAMM.Signer.Certs](./MAMM.Signer.Certs) i [MAMM.Signer.CertsRef](./MAMM.Signer.CertsRef) projekti sadrže programski
kôd za baratanje certifikatima koji se dijeli između projekata MAMM.Signer.Cli i MAMM.Signer.Interop. Radi se o Visual
Studio Shared vrsti projekta.

* [MAMM.Signer.Tests](./MAMM.Signer.Tests/README.md) testni su slučajevi za biblioteku MAMM.Signer.Pkcs. Grade se za
.NET 10 kostur.

* [MAMM.Signer.Vb6](./MAMM.Signer.Vb6/README.md) je demonstracijski klijent za MAMM.Signer.Interop COM biblioteku
napisan u Visual Basicu 6.

## Izgradnja

Izgrade se projekti iz Visual Studija 2026 ili pomoću naredbe `dotnet build` iz vrha repozitorija. Preporučeno je
`dotnet build` koristiti iz Developer Command Prompta Visual Studija pošto projekt MAMM.Signer.Interop pokreće MIDL za
kreiranje .tlb datoteke. Ne izgradi li se tako, neće se producirati .tlb datoteka. Ona je bitna samo za učitavanje
tipova u VB6 IDE.

Publicira se normalno s `dotnet publish`, ali neće tako uspjeti izgradnja projekta MAMM.Signer.Interop pošto se ne mogu
publicirati obje izgradnje, već za svaki kostur treba publicirati zasebno. Skripta `publish.cmd` zapravo izvrši `dotnet
publish` za svaki projekt zasebno.

## Licence

Tumačenje [Unlicense](https://choosealicense.com/licenses/unlicense/) i [GNU GPL
v3](https://choosealicense.com/licenses/gpl-3.0/) licenci može se pronaći na
[choosealicense.com](https://choosealicense.com/).

| Projekt              | Licenca                                       |
|----------------------|-----------------------------------------------|
| MAMM.Signer.Pkcs     | [Unlicense](MAMM.Signer.Pkcs/LICENSE.txt)     |
| MAMM.Signer.Interop  | [Unlicense](MAMM.Signer.Interop/LICENSE.txt)  |
| MAMM.Signer.Cli      | [GNU GPL v3](MAMM.Signer.Cli/LICENSE.txt)     |
| MAMM.Signer.Gui      | [GNU GPL v3](MAMM.Signer.Gui/LICENSE.txt)     |
| MAMM.Signer.Core     | [GNU GPL v3](MAMM.Signer.Core/LICENSE.txt)    |
| MAMM.Signer.Tests    | [Unlicense](MAMM.Signer.Tests/LICENSE.txt)    |
| MAMM.Signer.Vb6      | [GNU GPL v3](MAMM.Signer.Vb6/LICENSE.txt)     |

---

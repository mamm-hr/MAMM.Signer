# Testne datoteke TestCase

Ove datoteke služe da se usporedi proizvod programa AKDSH Signer s proizvodom ove biblioteke, kao i za neka
eksplorativna testiranja.

Datoteka [TestCase.txt](TestCase.txt) sadrži kratki tekst i služi potpisivanju i šifriranju za testne scenarije.

Pomoću AKDSH Signera treba pripremiti tri datoteke: dvije potpisane i jednu potpisanu i šifriranu. Koriste se vlastiti
soft certifikat i plava kartica izdana za CEZIH. Korištene certifikate treba specificirati u
[.runsettings](../.runsettings) kako je opisano u [TestCert.md](TestCert.md).

Ako se datoteke ne pripreme i vlastiti certifikati CEZIH-a ne konfiguriraju, testovi koji koriste te scenarije, odnosno
te certifikate završit će neuspješnim ishodom. Umjesto toga mogu se ti testovi isključiti parametrom
`SuppressTestsUsingCezihCerts` u [.runsettings](../.runsettings) datoteci, pa će završiti bez ishoda.

Testni scenariji s pripadnim datotekama konfiguriraju su u [.runsettings](../.runsettings) kroz parametre čiji nazivi
započinju s `TestCase.n`, gdje je `n` indeks scenarija definiran konstantom `RunSettings.TESTCASE_*` u
[RunSettings.cs](RunSettings.cs) prema slijedećoj tablici:

| `n` | Opis |
|-----|------|
| TESTCASE_SOFT_NONE  | TestCase.txt potpisana AKDSH Signerom pomoću soft ceritifikata. |
| TESTCASE_BLUE_NONE  | TestCase.txt potpisana AKDSH Signerom pomoću plave kartice. |
| TESTCASE_SOFT_BLUE  | TestCase.txt potpisana AKDSH Signerom pomoću soft certifikata i šifrirana plavom karticom. |
| TESTCASE_WHITE_NONE | Eksplorativni slučaj potpisivanja bijelom karticom. |

Datoteke potpisane/šifrirane AKDSH Signerom u kombinaciji s konfiguriranim testnim scenarijima koriste testovi u
razredu [Tests_Akdsh_Equivalency.cs](Tests_Akdsh_Equivalency.cs) da generiraju ekvivalentne potpisane/šifrirane
datoteke i usporede ih s prvima. Testne scenarije se također koristi i u razredima
[Exploratory_AkdshSigner.cs](Exploratory_AkdshSigner.cs) i [Exploratory_Pkcs7.cs](Exploratory_Pkcs7.cs)

Značenje pojedinih [.runsettings](../.runsettings)  parametara koji opisuju certifikate je kako slijedi:

| Parametar | Opis |
|-----------|------|
| SignDateTime	  | UTC vrijeme potpisa datoteke. |
| SignCertNo	  | Indeks n certifikata `TestCert.n` (v. [TestCert.md](TestCert.md)) kojim je datoteka potpisana. |
| SignAlg		  | Asimetrični algoritam za potpisivanje ili prazno za prešutni. |
| CryptCertNo	  | Indeks n certifikata `TestCert.n` kojim je datoteka šifrirana ili prazno ako se ne šifrira. |
| CryptAlg		  | Simetrični algoritam za šifriranje ili prazno za prešutni. |
| ContentFileName | Naziv u projektu priložene datoteke koja je potpisana ('TestCase.txt'). |
| MessageFileName | Naziv priložene od AKDSH Signera potpisane/šifrirane datoteke ovog slučaja testiranja ili prazno ako slučaj ne služi uspoređivanju. |

## Sadržaj datoteka potpisanih od AKDSH Signera

Sadržaj AKDSH Signerom potpisanih/šifriranih datoteka može se istražiti testovima u razredu
[AkdshSigner_Exploratory.cs](AkdshSigner_Exploratory.cs). Testovi ispisuju sadržaj datoteka pomoću metode
`Console.WriteLine` pa će se pokazati u sažetku ishoda testa Test Explorera.

---

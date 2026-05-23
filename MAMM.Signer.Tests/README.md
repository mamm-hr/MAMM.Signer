# MAMM.Signer.Tests

Ova biblioteka sadrži testove u razredima `Tests_*.cs`, a eksplorativne testove u razredima `Exploratory_*.cs`.

Testovi koriste generirane certifikate koji svojstvima prate certifikate izdane za CEZIH na plavim karticama, soft
certifikate i na bijelim karticama. Ti su certifikati priloženi u .pfx datotekama. Neki testovi koriste proizvođaču
dodijeljenu plavu karticu i soft certifikat u svrhu istraživanja načina rada AKDSH Signera i u svrhu provjere da ova
biblioteka za te certifikate producira AKDSH Signeru identičan izlaz. Da bi se ti testovi mogli provesti, potrebno je
testove konfigurirati vlastitim dobivenim certifikatmima i pripremiti određene testne datoteke pomoću AKDSH Signera. Ti
su scenariji opisani u datoteci [TestCase.md](TestCase.md), a konfiguriranje certifikata u [TestCert.md](TestCert.md).
Konfiguriranje vlastitih certifikata i pripremu datoteka može se izbjeći isključivanjem testova koji koriste te
certifikate parametrom `SuppressTestsUsingCezihCerts` u [.runsettings](../.runsettings) datoteci.

Za daljnje proučavanje ove biblioteke ishodišne su datoteke [.runsettings](../.runsettings), [TestCase.md](TestCase.md)
i [TestCert.md](TestCert.md).

---

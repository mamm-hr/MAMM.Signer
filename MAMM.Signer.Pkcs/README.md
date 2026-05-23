# MAMM.Signer.Pkcs

Ova biblioteka sadrži metode potpisivanje /_signing_/ i kuvertiranje /_enveloping_/ (tj. šifriranje, kriptiranje)
podataka po PKCS #7, odnosno CMS specifikaciji i sintaksi (RFC 2315, odnosno RFC 5652). Implementacija se temelji
na rezredima `SignedCms` i `EnvelopedCms` dostupnima unutar .NET Standard 2.0 specifikacije.

Dizajn biblioteke usmjeren je zamjeni od HZZO-a distribuiranog programa AKDSHCard Data Signer and Verifier (dalje
skraćeno AKDSH Signer). Očekuje se u prvom redu korištenje certifikata s HZZO-ove "plave" kartice, HZZO-ovog soft
certifikata koji je prijelazno izdavan zdravstvenim radnicima ili certifikata s nove AKD-ove "bijele" kartice. S drugim
RSA certifikatima biblioteka bi uglavnom trebala raditi iako duljina digitalnog sažetka ne mora biti prikladno izabrana.
Ograničeno će raditi s ECC certifikatima, sukladno ograničenjima podrške CMS specifikaciji u .NET-u.

Dok je "plave" RSA kartice moguće koristiti i za potpisivanje i za šifriranje podataka, nove "bijele" ECDSA kartice
moguće je koristiti samo za potpisivanje jer je ECDSA potpisni algoritam i ne može se koristiti za razmjenu /_key
exchange_/, odnosno dogovaranje /_key agreement_/ simetričnog ključa između komunicirajućih strana.

Potpisivanje "bijelim" karticama će raditi na krivuljama P-256, P-384 i P-521 kada je kroz opcije metoda pravilno
izabran algoritam digitalnog sažetka.

Središnji razred je [CmsMessage](./CmsMessage.cs), a pomoćni razred je [Pkcs7](./Pkcs7.cs). Razred
[Pkcs7Options](./Pkcs7Options.cs) služi za usmjeravanje rada metoda prethodna dva razreda. Programski kôd je
dokumentiran.

## Kompatibilnost s AKDSH Signerom

Za "plave" kartice metode bi trebale generirati podatke binarno identične onima koje daje AKDSH Signer. Međutim,
rezultati će se ipak binarno razlikovati ako se biblioteka izgradi za .NET umjesto za .NET Framework, jer potonji
emitira u DER zapis AlgorithmIdentifier tip s uključenim jednim NULL parametrom što odgovara načinu na koji radi AKDSH
Signer, dok .NET izostavlja taj parametar. Te razlike proizlaze iz RFC 3279, RFC 4055 i RFC 5652 (CMS) primjena i van su
opsega ove dokumentacije. Obje su situacije posve legalne za DER zapise.

Želi li se dobiti binarno identičan rezultat i za HZZO-ov soft cerifikat, kroz `Pkcs7Options` objekt podesiti SHA-1 kao
algoritam digitalnog sažetka i za RSA certifikate koji nisu AKDSHCard CSP implementacije, v.
`Pkcs7Options.DigestAlgorithms.RsaKsp`.

## Čitanje ASN.1 sintakse

Za pregled sadržaja DER zapisa binarni se podatak može pretvoriti u heskadecimalni zapis kroz `Convert.ToHexString` i 
kopirati u neki online čitač, v. npr. https://lapo.it/asn1js.

---

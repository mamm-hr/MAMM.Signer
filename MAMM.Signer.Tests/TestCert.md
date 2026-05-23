# Testni certifikati

Testovi koriste testne certifikate izdane za CEZIH, kao i nekoliko generiranih za potrebe testiranja, a koji svojstvima
prate certifikate izdane za CEZIH. Generirani certifikati su priloženi u projektu kao .pfx datoteke, a izrađeni su na
ovdje dokumentirani način.

Certifikati izdani za CEZIH koriste se isključivo eksplorativno, odnosno da se usporedi proizvod od programa AKDSH
Signer s proizvodom ovog programa, stoga je njihovo korištenje opcionalno. Za testiranje biblioteke koriste se
generirani certifikati.

Da bi testovi koji koriste certifikate izdane za CEZIH radili, potrebno je konfigurirati njihove parametre u
[.runsettings](../.runsettings) sukladno karticama raspoloživima na računalu na kojem se testira. Ne konfiguriraju li
se, odn. ne bude li moguće u korisnikovom spremištu certifikata pronaći konfigurirane certifikate, testovi će završiti
bez ishoda.

Generirane certifikate priložene kao .pfx datoteke ne treba uvažati u spremište certifikata, to se napravi automatski
tijekom izvođenja testova i potom se certifikati automatski i obrišu. Zbog toga se testovi moraju izvoditi u slijedu, ne
smiju se izvoditi paralelno (v. [MSTestSettings.cs](MSTestSettings.cs)). 

Svi se certifikati konfiguriraju u datoteci [.runsettings](../.runsettings) kroz parametre čiji nazivi započinju s
`TestCert.n`, gdje je `n` indeks certifikata definiran konstantom `RunSettings.TESTCERT_*` u
[RunSettings.cs](RunSettings.cs) prema slijedećoj tablici:

| `n` | Opis |
|-----|------|
| TESTCERT_SOFT        | Testni soft certifikat izdan od HZZO-a. |
| TESTCERT_BLUE        | Certifikat na plavoj kartici izdanoj od HZZO-a. |
| TESTCERT_WHITE       | Certifikat na bijeloj kartici izdanoj od HZZO-a. |
| TESTCERT_ROOT        | Korijenski CA certifikat generiran za testiranje. |
| TESTCERT_CA          | Subordinirani CA certifikat generiran za testiranje. |
| TESTCERT_RSA         | RSA certifikat sličan soft certifikatu i certifikatu na plavoj kartici generiran za testiranje. |
| TESTCERT_ECDSA_IDENT | ECDSA certifikat sličan identifikacijskom certifikatu na bijeloj kartici generiran za testiranje. |
| TESTCERT_ECDSA_SIGN  | ECDSA certifikat sličan potpisnom certifikatu na bijeloj kartici generiran za testiranje. |

Značenje pojedinih [.runsettings](../.runsettings) parametara koji opisuju certifikate je kako slijedi:

| Parametar | Opis |
|-----------|------|
| `TestCert.n.Thumb`   | Digitalni otisak /_thumbprint_/ certifikata po kojem se identificira. |
| `TestCert.n.ShowsUI` | Ovo mora biti `true` ako je se pri korištenju privatnog ključa prikazuje korisničko sučelje (npr. za unos PIN-a), inače treba biti `false`. Služi tome da se konfiguracijom može isključiti testove koji bi mogli prikazati korisničko sučelje tijekom izvođenja. |
| `TestCert.n.Cezih`   | Ovo mora biti `true` za certifikate izdane za CEZIH kako ih testovi ne bi brisali iz spremišta certifikata po završetku, a `false` za generirane certifikate priložene projektu kao .pfx datoteke kako bi bili uvezeni u spremišta pri izvođenju testova. |

Generirani certifikati ne uključuju podatak o točkama raspodjele listi opozvanih certifikata /_CDP, CLR Distribution
Points_/, a niti podatak o pristupu informacijama o ustanovi koja je izdala certifikat /_AIA, Authority Information
Access_/, zbog čega provjera opoziva certifikata nije moguća, pa tako niti provjera lanca povjerenja koja uključuje te
provjere nije moguća prilikom ovjere potpisa tim certifikatima. Ove provjere implementirane su u razredu
`Tests_CmsMessage_Verify` koji je onda utoliko parcijalan.

Kako nije moguće vršiti provjeru lanca povjerenja, testovi ne zahtjevaju uvažanje korijenskog, niti subordiniranog CA
certifikata. Ipak, oni se mogu uvesti izvođenje testova u razredu `TestCert_Setup`.

## TESTCERT_ROOT

Korijenski certifikat institucije za izdavanje certifikata (root CA certifikat) naziva TESMAMMCA koji imitira
TESTAKDCA korijenski certifikat AKD-a za izdavanje testnih kartica.

    $testmammca = New-SelfSignedCertificate `
        -Type Custom `
        -Subject 'CN=TESTMAMMCA Root,OID.2.5.4.97=VATHR-52599776564,O=MAMM d.o.o.,C=HR' `
        -KeyAlgorithm 'RSA' `
        -KeyLength 4096 `
        -HashAlgorithm sha256 `
        -KeyExportPolicy Exportable `
        -KeyUsage CertSign,CRLSign `
        -TextExtension @(
            '2.5.29.19={critical}{text}CA=true'
        ) `
        -CertStoreLocation 'Cert:\CurrentUser\My' `
        -NotBefore (Get-Date).AddMinutes(-10) `
        -NotAfter ([datetime]'2038-01-19T04:14:07Z') `
        -FriendlyName 'TESTMAMMCA Root'

Certifikat je priložen u datoteci [TestCert_Root.pfx](TestCert_Root.pfx) za zaporkom "testcert".

## TESTCERT_CA

Subordinirani certifikat institucije za izdavanje certifikata (intermediate CA certifikat) naziva TESTMAMMSIGNER koji
imitira TESTCERTILIA subordinirani certifikat AKD-a za izdavanje testnih kartica.

    $testmammsigner = New-SelfSignedCertificate `
        -Type Custom `
        -Subject 'CN=TESTMAMMSIGNER,OID.2.5.4.97=VATHR-52599776564,O=MAMM d.o.o.,C=HR' `
        -Signer $testmammca `
        -KeyAlgorithm ECDSA_nistP384 `
        -CurveExport CurveName `
        -HashAlgorithm sha256 `
        -KeyExportPolicy Exportable `
        -KeyUsage CertSign,CRLSign `
        -TextExtension @(
            '2.5.29.19={critical}{text}CA=true'
        ) `
        -CertStoreLocation 'Cert:\CurrentUser\My' `
        -NotBefore (Get-Date).AddMinutes(-10) `
        -NotAfter (Get-Date).AddYears(15) `
        -FriendlyName 'TESTMAMMSIGNER'

Certifikat je priložen u datoteci [TestCert_CA.pfx](TestCert_CA.pfx) za zaporkom "testcert".

## TESTCERT_RSA

Certifikat potpisan s TESTCERT_CA naziva "HLAPIĆ, FRANC, 990001591" koji imitira certifikat na "plavoj" kartici, odnosno
soft certifikat izdan od CEZIH-a.

    $oids = [System.Security.Cryptography.OidCollection]::new()
    $null = $oids.Add([System.Security.Cryptography.Oid]::new('1.3.6.1.5.5.7.3.2'))      # Client Authentication
    $null = $oids.Add([System.Security.Cryptography.Oid]::new('1.3.6.1.5.5.7.3.4'))      # Secure Email
    $null = $oids.Add([System.Security.Cryptography.Oid]::new('1.3.6.1.4.1.311.20.2.2')) # Smart Card Logon

    $eku = [System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension]::new($oids, $false)

    $francsoft = New-SelfSignedCertificate `
        -Type Custom `
        -Subject 'OID.0.9.2342.19200300.100.1.1=990001591,OU=cezih,O=cezih,C=HR' `
        -Signer $testmammca `
        -KeyAlgorithm RSA `
        -KeyLength 1024 `
        -HashAlgorithm sha1 `
        -KeyExportPolicy Exportable `
        -KeyUsage DigitalSignature,NonRepudiation,KeyEncipherment `
        -TextExtension @(
            '2.5.29.19={text}CA=false',
            '2.5.29.17={text}email=990001591@cezih.hr&upn=990001591@cezih.hr'
        ) `
        -Extension @($eku) `
        -CertStoreLocation 'Cert:\CurrentUser\My' `
        -NotBefore (Get-Date).AddMinutes(-10) `
        -NotAfter ([datetime]'2026-06-29T22:00:00Z') `
        -FriendlyName 'HLAPIĆ, FRANC, 990001591'

Certifikat je priložen u datoteci [TestCert_RSA.pfx](TestCert_RSA.pfx) za zaporkom "testcert".

## TESTCERT_ECDSA_IDENT

Certifikat potpisan s TESTCERT_CA naziva "Franc Hlapić (IdentificationTest)" koji imitira identifikacijski certifikat na
"bijeloj" iskaznici izdanoj od AKD-a. 

    $franceccident = New-SelfSignedCertificate `
        -Type Custom `
        -Subject 'CN=Franc Hlapić,serialNumber=PNOHR-99000159197,G=Franc,SN=Hlapić,OU=IdentificationTest,C=HR' `
        -Signer $testmammsigner `
        -KeyAlgorithm ECDSA_nistP384 `
        -CurveExport CurveName `
        -HashAlgorithm sha384 `
        -KeyExportPolicy Exportable `
        -KeyUsage DigitalSignature `
        -TextExtension @(
            '2.5.29.19={critical}{text}CA=false',
            '2.5.29.37={text}1.3.6.1.5.5.7.3.2'
        ) `
        -CertStoreLocation 'Cert:\CurrentUser\My' `
        -NotBefore (Get-Date).AddMinutes(-10) `
        -NotAfter (Get-Date).AddYears(3) `
        -FriendlyName 'Franc Hlapić (IdentificationTest)'

Certifikat je priložen u datoteci [TestCert_ECDSA_Ident.pfx](TestCert_ECDSA_Ident.pfx) za zaporkom "testcert".

## TESTCERT_ECDSA_SIGN

Certifikat potpisan s TESTCERT_CA naziva "Franc Hlapić (SignatureTest)" koji imitira potpisni certifikat na "bijeloj"
iskaznici izdanoj od AKD-a. Ne sadrži kvalificirane izjave o certifikatu /Qualified Certificate Statements/
(1.3.6.1.5.5.7.1.3, qcStatements), za njih bi certifikat trebalo kreirati naredbom `certreq`.

    $franceccsign = New-SelfSignedCertificate `
        -Type Custom `
        -Subject 'CN=Franc Hlapić,serialNumber=PNOHR-99000159197,G=Franc,SN=Hlapić,OU=IdentificationTest,C=HR' `
        -Signer $testmammsigner `
        -KeyAlgorithm ECDSA_nistP384 `
        -CurveExport CurveName `
        -HashAlgorithm sha384 `
        -KeyExportPolicy Exportable `
        -KeyUsage NonRepudiation `
        -TextExtension @(
            '2.5.29.19={critical}{text}CA=false'
        ) `
        -CertStoreLocation 'Cert:\CurrentUser\My' `
        -NotBefore (Get-Date).AddMinutes(-10) `
        -NotAfter (Get-Date).AddYears(3) `
        -FriendlyName 'Franc Hlapić (SignatureTest)'

Certifikat je priložen u datoteci [TestCert_ECDSA_Sign.pfx](TestCert_ECDSA_Sign.pfx) za zaporkom "testcert".

## Premještanje certifikata

Generirani korijenski i subordinirani certifikat se eksportiraju iz osobnog spremišta bez privatnih ključeva i
razmještaju u spremište pouzdanih korijenskih ustanova, odnosno međuustanova za izdavanje certifikata. 

    Export-Certificate -Cert $testmammca -FilePath .\TESTMAMMCA.cer
    Import-Certificate -FilePath .\TESTMAMMCA.cer -CertStoreLocation 'Cert:\CurrentUser\Root'

    Export-Certificate -Cert $testmammsigner -FilePath .\TESTMAMMSIGNER.cer
    Import-Certificate -FilePath .\TESTMAMMSIGNER.cer -CertStoreLocation 'Cert:\CurrentUser\CA'

Iz osobnog spremišta ih ne treba brisati da se ne izgubi privatni ključ, ali brisali bi se ovako:

    $thumbprint = $testmammca.Thumbprint
    Remove-Item -Path "Cert:\CurrentUser\My\$thumbprint"
    $thumbprint = $testmammsigner.Thumbprint
    Remove-Item -Path "Cert:\CurrentUser\My\$thumbprint"

---

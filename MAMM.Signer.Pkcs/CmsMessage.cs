using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace MAMM.Signer.Pkcs;

/// <summary>
/// Objekt za izradu i čitanje poruka u CMS sintaksi.
/// </summary>
///
/// <remarks>
/// <para>
///     Sučelje ovog razreda dizajnirano je zamišljanjem dviju strana koje razmjenjuju poruke. Pošiljatelj konstruira
///     objekt sadržajem poruke, poruku potpiše i kuvertira je adresiranjem na primatelja, potom očita tekst poruke i
///     dostavi ga primatelju. Primatelj konstruira objekt tekstom primljene poruke, otvori kuvertu, verificira potpis
///     pošiljatelja i pročita sadržaj poruke. Pošiljatelj poruku ne mora i potpisati i kuvertirati, već je može i samo
///     potpisati ili samo kuvertirati bez potpisa. Primatelj može objekt konstruirati samo potpisanom porukom, samo
///     kuvertiranom porukom ili kuvertiranom potpisanom porukom.</para>
/// <para>
///     Tekst poruke koja se priprema za slanje inicijalno je identičan sadržaju poruke s kojim je objekt
///     inicijaliziran. Potpisivanjem, odnosno kuvertiranjem poruke, sadržaj poruke se zapiše u CMS sintaksi, sukladno
///     specifikacijama <see href="https://www.rfc-editor.org/rfc/rfc2315">RFC 2315</see> (PKCS #7), tj. <see
///     href="https://www.rfc-editor.org/rfc/rfc5652">RFC 5652</see> (CMS), a korištenjem .NET implementacije kroz
///     objekte <see cref="SignedCms"/> i <see cref="EnvelopedCms"/>, rezultirajući ovim tekstom poruke:</para>
/// <list type="bullet">
/// <item>
///     tekst potpisane poruke je "signed-data content type", odnosno SignedData tip,</item>
/// <item>
///     tekst kuvertirana poruke je "enveloped-data content type", odnosno EnvelopedData tip,</item>
/// <item>
///     tekst poruke koja je potpisana i potom kuvertirana je EnvelopedData(SignedData), dakle EnvelopeData tip koji
///     sadrži SignedData tip,</item>
/// <item>
///     ovaj objekt <b>ne generira</b>, niti korištena .NET implementacija podržava "signed-and-enveloped-data content
///     type" tj. SignedAndEnvelopedData tip i potpisivanjem, pa kuvertiranje poruke ne prepiše sadržaj poruke u tekst
///     poruke tog sintaktičkog zapisa.</item>
/// </list>
/// <para>
///     Tekst poruke čita se funkcijom <see cref="Encode"/>. Po konstruiranju objekta sadržajem poruke koju se šalje,
///     funkcija <see cref="Encode"/> nije dostupna jer nije poznato kojeg tipa bi trebao biti tekst poruke (SignedData
///     ili EnvelopedData). Poruka se potpiše metodom <see cref="Sign(X509Certificate2, DateTimeOffset)"/> po čemu <see
///     cref="Encode()"/> vrati SignedData tip. Poruka se kuvertira metodom <see cref="Envelope(X509Certificate2,
///     Oid?)"/> po čemu <see cref="Encode()"/> vrati EnvelopedData tip ako poruka nije prethodno potpisana, a
///     EnvelopedData(SignedData) ako je prethodno potpisana. Ako se sadržaj poruke potpisuje i kuvertira, prvo se mora
///     potpisati, a onda kuvertirati tako da se producira tekst poruke tipa EnvelopedData(SignedData).</para>
/// <para>
///     Funkciju <see cref="Encode()"/> ne treba koristiti nakon što se objekt inicijalizira tekstom primljene poruke.
///     Ipak, funkcija po inicijalizaciji tekstom primljene poruke vrati kako slijedi: funkcija nije dostupna kad je
///     objekt konstruiran iz EnvelopedData teksta; po inicijalizaciji SignedData tipom teksta ili po otvaranju
///     kuvertirane poruke vrati SignedData tip, odnosno sadržaj poruke kad se otvori kuvertirana nepotpisana
///     poruka.</para>
/// <para>
///     Sadržaj poruke čita se funkcijom <see cref="Read"/>. Inicijalizirati objekt primljenom porukom dopušteno je
///     isključivo tekstom SignedData ili EnvelopedData tipa. Ako je inicijaliziran tekstom EnvelopedData tipa, funkcija
///     <see cref="Read"/> nije odmah dostupna, nego tek pošto se kuverta otvori pozivom metode <see
///     cref="OpenEnvelope(X509Certificate2?)"/>.</para>
/// <para>
///     Kad se objekt inicijalizira sadržajem poruke, funkcija <see cref="Read"/> vrati taj isti sadržaj.</para>
/// </remarks>
public class CmsMessage
{
    /// <summary>
    /// Stanje poruke.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Ovo je diskriminator koji utvrđuje koja se vrsta objekta referencira članskom varijablom <see
    ///     cref="m_cms"/>. Objekt može biti <see cref="SignedCms"/> ili <see cref="EnvelopedCms"/>, a uvijek je u
    ///     dekodiranom stanju.</para>
    /// </remarks>
    ///
    protected enum MessageState
    {
        /// <summary>
        /// Poruka je nepotpisana i nešifrirana, a čuva se u <see cref="m_cms"/> kao referenca na objekt tipa <see
        /// cref="SignedCms"/> s još nepotpisanim sadržajem. Dopušteni prijelazi u <see cref="Signed"/> pozivom metode
        /// <see cref="Sign(X509Certificate2, DateTimeOffset)"/> ili <see cref="Enveloped"/> pozivom metode <see
        /// cref="Envelope(X509Certificate2, Oid?)"/>.
        /// </summary>
        Plain,

        /// <summary>
        /// Poruka je potpisana, tj. tipa SignedData, a čuva se u <see cref="m_cms"/> kao referenca na objekt tipa <see
        /// cref="SignedCms"/> s potpisanim sadržajem. Dopušteni prijelaz u <see cref="Enveloped"/> pozivom metode <see
        /// cref="Envelope(X509Certificate2, Oid?)"/>.
        /// </summary>
        Signed,

        /// <summary>
        /// Poruka je stavljena u kuvertu, tj. EnvelopedData ili EnvelopedData(SignedData) nakon šifriranja sadržaja,
        /// ali prije kodiranja, a čuve se u <see cref="m_cms"/> kao referenca na objekt tipa <see cref="EnvelopedCms"/>
        /// s dostupnim sadržajem poruke. Završno stanje, nema prijelaza.
        /// </summary>
        Enveloped,

        /// <summary>
        /// Poruka je u primljenoj kuverti, tj. EnvelopedData ili EnvelopedData(SignedData) nakon dekodiranja, ali prije
        /// dešifriranja, a čuva se u <see cref="m_cms"/> kao referenca na objekt tipa <see cref="EnvelopedCms"/> s
        /// nedostupnim sadržajem poruke. Prelazi u <see cref="Signed"/> ili <see cref="Plain"/> pozivom metode <see
        /// cref="OpenEnvelope(X509Certificate2?)"/>.
        /// </summary>
        ReceivedEnvelope,
    }

    /// <summary>
    /// Popis identiteta primatelja poruke.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Sadržaj popisa inicijalizira se konstruktorom, a bude prazan ako objekt nije inicijaliziran kuvertiranom
    ///     porukom. Popis se ažurira kuvertiranjem poruke. Sadrži isključivo primatelje koji su u EnvelopedData
    ///     identificirani izdavačem i serijskim brojem certifikata, dok se primatelji koji su identificirani digitalnim
    ///     sažetkom javnog ključa u ovom potpisu ne nalaze, ignorirani su.</para>
    /// </remarks>
    public IReadOnlyList<X509IssuerSerial> Recipients { get; protected set; }

    /// <summary>
    /// Popis identiteta potpisnika poruke.
    /// </summary>
    ///
    /// <exception cref="InvalidMessageStateException">
    ///     Potpisnci možda postoje, ali ih nije moguće utvrditi jer je poruka još u kuvert.</exception>
    ///
    /// <remarks>
    /// <para>
    ///     Sadržaj popisa inicijalizira se konstrutorom, a bude prazan ako objekt nije inicijaliziran potpisanom
    ///     porukom. Kad je inicijaliziran kuvertiranom porukom, potpisnici možda postoje, ali nisu poznati dok se
    ///     kuverta ne otvori. Popis se ažurira otvaranjem kuvertirane poruke, odnosno potpisivanjem poruke. Sadrži
    ///     isključivo potpisnike koji su u SignedData identificirani izdavačem i serijskim brojem certifikata, dok se
    ///     potpisnici koji su identificirani digitalnim sažetkom javnog ključa u ovom potpisu ne nalaze, ignorirani
    ///     su.</para>
    /// </remarks>
    public IReadOnlyList<X509IssuerSerial> Signers
        => m_signers ?? throw new InvalidMessageStateException( MessageState.Signed.ToString(), m_messageState.ToString() );
    protected IReadOnlyList<X509IssuerSerial>? m_signers = null;

    /// <summary>
    /// Inicijalizira objekt sadržajem poruke koja se šalje ili tekstom poruke koja je primljena kodirana u CMS
    /// sintaksi.
    /// </summary>
    ///
    /// <param name="data">
    ///     Sadržaj ili tekst poruke.
    ///     </param>
    ///
    /// <param name="isReceived">
    ///     Ako je <see langword="true"/>, indicira da je <paramref name="data"/> tekst primljene poruke, dakle valjani
    ///     CMS tekst. Ako je <see langword="false"/>, indicira da je <paramref name="data"/> sadržaj poruke koja se
    ///     šalje, dakle čisti (nepotpisani i nešifrirani) tekst.
    ///     </param>
    ///
    /// <param name="options">
    ///     Dodatne opcije za usmjeravanje rads metoda objekta, vidi <see cref="Pkcs7Options"/>.</param>
    ///
    /// <exception cref="ArgumentNullException">
    ///     Agument <paramref name="data"/> je <see langword="null"/>.
    ///     </exception>
    ///
    /// <exception cref="CryptographicException">
    ///     Sadržaj argumenta <paramref name="data"/> ne može se dekodirati kao valjana CMS struktura.
    ///     </exception>
    ///
    /// <exception cref="UnsupportedCmsContentTypeException">
    ///     Sadržaj argumenta <paramref name="data"/> je valjana CMS struktura, ali nije jednog od podržanih tipova
    ///     EnvelopedData, odnosno SignedData.
    ///     </exception>
    ///
    internal CmsMessage(
          byte[] data
        , bool isReceived
        , Pkcs7Options? options = null
        )
    {
        if(data is null)
            throw new ArgumentNullException( nameof( data ) );

        m_options = options ?? new();

        // Utvrdi vrstu podatka i iz nje stanje sadržaja poruke, te identitiet primatelja i potpisnika poruke.
        if(isReceived)
        {
            // Pozivatelj idicira da se radi o tekstu primljene poruke, dakle tekstu u CMS sintaksi.

            // Utvrdi je li u pitanju kuvertirana poruka (EnvelopedData) ili potpisana poruka (SignedData) ili
            // nepodržani tip podatka.
            var contentTypeOid = Pkcs7.GetContentTypeOid( data );
            if(Pkcs7.Oids.EnvelopedData.Value == contentTypeOid.Value)
            {
                // Ovo je kuvertirana poruka. Očekuje da se u njemu nalazi potpisana poruke ili nepotpisani sadražj, ali
                // taj informacije je trenutno nedostupna, jer kuverta nije otvorena (poruke je šifrirana).
                var cms = new EnvelopedCms();
                cms.Decode( data );
                m_cms = cms;

                // Dostupan je identitet primatelja poruke, o potpisnicima nije ništa moguće reći.
                this.Recipients = RetrieveRecipientIdentities( cms.RecipientInfos, m_options );
                m_signers = null;

                m_messageState = MessageState.ReceivedEnvelope;
            }
            else if(Pkcs7.Oids.SignedData.Value == contentTypeOid.Value)
            {
                // Ovo je potpisana poruka.
                var cms = new SignedCms();
                cms.Decode( data );
                m_cms = cms;

                // Dostupan je identitet potpisnika poruke, dok primatelja nema jer poruka nije kuvertirana.
                this.Recipients = [];
                m_signers = RetrieveSignerIdentities( cms.SignerInfos, m_options );

                m_messageState = MessageState.Signed;
            }
            else
                throw new UnsupportedCmsContentTypeException( contentTypeOid.Value );
        }
        else
        {
            // Pozivatelj indicira da se radi o sadržaju poruke koju se šalje.

            // Tekst poruke za sada je identičan sadržaju poruke.
            m_messageState = MessageState.Plain;
            m_cms = new SignedCms(
                  SubjectIdentifierType.IssuerAndSerialNumber
                , new ContentInfo( data )
                , detached: false
                );

            // Poruka nema niti primatelja, niti potpisnika.
            this.Recipients = [];
            m_signers = [];
        }
    }

    /// <summary>
    /// Kodira trenutni tekst poruke, odgovarajuće stanju sadržaja.
    /// </summary>
    ///
    /// <exception cref="InvalidMessageStateException">
    ///     Objekt je inicijaliziran sadržajem poruke koji nije niti potpisan, niti kuvertiran i ne može se kodirati u
    ///     SignedData ili EnvelopedData tip, pošto tip još nije određen. Alternativno, tekst poruke je nedostupan jer
    ///     je poruka inicijalizirana kuvertiranom porukom, a <see cref="EnvelopedCms"/> objekt ne podržava ponovno
    ///     kodiranje dekodirane poruke, tj. ne producira valjan CMS zapis. Potrebno je kuvertu otvoriti (poruku
    ///     dešifrirati), pa je potom staviti u novu kuvertu (opet kuvertirati).</exception>
    ///
    public byte[] Encode()
    {
        switch(m_messageState)
        {
            case MessageState.Plain:
                Debug.Assert( m_cms is SignedCms );
                throw new InvalidMessageStateException( MessageState.Signed.ToString(), m_messageState.ToString() );

            case MessageState.Signed:
                Debug.Assert( m_cms is SignedCms );
                return ((SignedCms)m_cms).Encode();

            case MessageState.Enveloped:
                // Zadnja operacija nad objektom bila je Encrypt, pa se sada smije zvati Encode.
                Debug.Assert( m_cms is EnvelopedCms );
                return ((EnvelopedCms)m_cms).Encode();

            case MessageState.ReceivedEnvelope:
                // EnvelopedCms objekt je upravo dekodiran (Decode je pozvan u kontruktoru). Objekt ne podržava Decode
                // -> Encode round-trip, tj. Encode se smije pozivati samo nakon Encrypt.
                throw new InvalidMessageStateException( MessageState.Enveloped.ToString(), m_messageState.ToString() );
        }
        Debug.Assert( false );
        return [];
    }

    /// <summary>
    /// Kuvertira poruku, tj. šifrira je za primatelja.
    /// </summary>
    ///
    /// <param name="certificate">
    ///     Primateljev certifikat koji ne treba sadržavati privatni ključ, već samo javni.</param>
    ///
    /// <param name="algorithm">
    ///     Algoritam za enkripciju sadržaja. Ne navede li se, .NET Framework od v. 4.8 i .NET od v. 4.6.0 NuGet paketa
    ///     koriste AES-256, a ranije DES3-EDE.</param>
    ///
    /// <exception cref="InvalidMessageStateException">
    ///     Ne može se poruka kuvertirati dva puta.</exception>
    ///
    /// <remarks>
    /// <para>
    ///     Da pošalje potpisanu poruku, potpisati treba pozivom metode <see cref="Sign(X509Certificate2,
    ///     DateTimeOffset)"/> prije nego se pozove ova metoda.</para>
    /// <para>
    ///     Nakon što je ova metoda pozvana, poruka je kuvertirana i jedino se metodom <see cref="OpenEnvelope"/> ona
    ///     može manipulirati.</para>
    /// <para>
    ///     Tekst poruke očitan metodom <see cref="Encode"/> odmah po kuvertiranju prikladan je za zapisivanje u
    ///     datoteku .p7m ekstenizije prema RFC 2311. Za MIME tip koristiti treba "application/pkcs7-mime", a za S/MIME
    ///     tip "enveloped-data":
    ///     <code>
    ///         Content-Type: application/pkcs7-mime; smime-type=enveloped-data; name="smime.p7m"
    ///         Content-Transfer-Encoding: base64 Content-Disposition: attachment; filename="smime.p7m"
    ///     </code>
    ///     </para>
    /// </remarks>
    ///
    public void Envelope(
          X509Certificate2 certificate
        , Oid? algorithm = null
        )
    {
        // Kuvertirati može nepotpisanu ili potpisanu poruka.
        if(MessageState.Signed != m_messageState && MessageState.Plain != m_messageState)
            throw new InvalidMessageStateException( expected: MessageState.Signed.ToString(), actual: m_messageState.ToString() );
        // Primatelj mora biti specificiran.
        if(certificate is null) throw new ArgumentNullException( nameof( certificate ) );

        // Trenutni tekst poruke (sadržaj poruke ili SignedData tip).
        var data = MessageState.Plain == m_messageState ? this.Read() : this.Encode();

        // Adresira na specificiranog primatelja.
        var recipient = new CmsRecipient(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
        var envelopedCms
            = algorithm is null
            ? new EnvelopedCms(new ContentInfo(data))
            : new EnvelopedCms(new ContentInfo(data), new AlgorithmIdentifier(algorithm));

        // Generira ključ za specificirani (ili prešutni) simetrični algoritam, šifrira trenutni test poruke tim
        // ključem, a taj ključ javnim ključem primatelja prevodeći poruku u EnvelopedData ili EnvelopedData(SignedData)
        // tip.
        envelopedCms.Encrypt( recipient );

        // Ažurira tekst poruke, identitet primatelja na upravo specificiranog i stanje sadržaja na otvorenu kuvertu
        // (poruka je šifrirana, ali i nešifrirani tekst je dostupan u objektu).
        m_cms = envelopedCms;
        this.Recipients = [new X509IssuerSerial() { IssuerName = certificate.Issuer, SerialNumber = certificate.SerialNumber }];
        m_messageState = MessageState.Enveloped;
    }

    /// <summary>
    /// Otvori kuvertiranu poruku, tj. dešifrira je.
    /// </summary>
    ///
    /// <param name="certificate">
    ///     Certifikat koji mora odgovarati identitetu jednog od primatelja ili <see langword="null"/> da se poruka
    ///     dešifrira raspoloživim certifikatom bilo kojeg od primatelja. Identiteti primatelja poruke (iako ne nužno
    ///     svi) dostupni su kroz svojstvo <see cref="Recipients"/>.</param>
    ///
    /// <exception cref="InvalidMessageStateException">
    ///     Otvarati se može samo još neotvorenu kuvertu.</exception>
    ///
    /// <remarks>
    /// <para>
    ///     Ako <paramref name="certificate"/> nije naveden, onda u dostupnim spremištima mora postojati certifikat s
    ///     privatnim ključem barem jednog od primatelja da bi metoda uspjela.</para>
    /// <para>
    ///     Ako <paramref name="certificate"/> jest naveden, sadržaj dešifrira certifikatom primatelja identificiranog
    ///     navedenim certifikatom. Certifikat primatelja traži i u dostupnim spremištima i može, ali ne mora koristiti
    ///     ondje navedeni certifikat ako ga pronađe (ovisi o implementaciji u .NET kosturu); to je, međutim,
    ///     irelevantno jer radi se o istom certifikatu. Garantirano koristi ovdje navedeni certifikat ako certifikata
    ///     nema u dostupnim spremištima.</para>
    /// <para>
    ///     Na Windowsima i u .NET implementaciji dostupna su spremišta osobnih certifikata računala (My, LocalMachine)
    ///     i trenutnog korisnika (My, CurrentUser), a vjerojatn je ista ponašanje i u .NET Framework
    ///     implementaciji.</para>
    /// <para>
    ///     Ne zada li se parametar <paramref name="certificate"/>, tijekom izvođenja ove metode može se pojaviti
    ///     korisničko sučelje za unos PIN-a prvog od primatelja za kojeg certifikat pronađe u dostupnim spremištima.
    ///     Ako sučelje za unos PIN-a neće identificirati certifikat, korisnik ne mora znati za koji se certifikat traži
    ///     PIN, tj. koji primatelj treba upisati PIN. Stoga je primatelja preporučljivo specificirati, osim kada za
    ///     korisnika nema dvojbe.</para>
    /// </remarks>
    ///
    public void OpenEnvelope(
          X509Certificate2? certificate = null
        )
    {
        // Otvarati se može samo zatvorenu kuvertu (još nedešifriranu poruku).
        if(MessageState.ReceivedEnvelope != m_messageState)
            throw new InvalidMessageStateException( expected: MessageState.ReceivedEnvelope.ToString(), actual: m_messageState.ToString() );

        // Poruka.
        var envelopedCms = m_cms as EnvelopedCms;
        Debug.Assert( envelopedCms is not null );

        // Dešifrira poruku.
        if(certificate is not null)
        {
            // Dešifrira privatnim ključem specificiranog primatelja.
            if(!TryFindRecipient(
                  envelopedCms!.RecipientInfos
                , new X509IssuerSerial() { IssuerName = certificate.Issuer, SerialNumber = certificate.SerialNumber }
                , out var recipient
                ))
                throw new UnknownRecipientException( certificate.Issuer, certificate.SerialNumber );
            Debug.Assert( recipient is not null );
            envelopedCms!.Decrypt( recipient!, new X509Certificate2Collection(certificate) );
        }
        else envelopedCms!.Decrypt(); // Dešifrira privatnim ključem bilo kojeg od primatelja.

        // Ažurira tekst poruke i identitete potpisnika (jer su upravo otkriveni). U kuverti mora biti ili potpisana
        // poruka ili je u pitanju samo sadržaj poruke bez potpisa.
        Oid contentTypeOid = Pkcs7.GetContentTypeOid( envelopedCms!.ContentInfo.Content );
        if(Pkcs7.Oids.SignedData.Value == contentTypeOid.Value)
        {
            // U kuverti je potpisana poruka.
            var signedCms = new SignedCms();
            signedCms.Decode( envelopedCms.ContentInfo.Content );
            m_cms = signedCms;

            // Dostupan je identitet potpisnika.
            m_signers = RetrieveSignerIdentities( signedCms.SignerInfos, m_options );

            m_messageState = MessageState.Signed;
        }
        else if(Pkcs7.Oids.Data.Value == contentTypeOid.Value)
        {
            // Sadržana je nepotpisana poruka.
            var signedCms = new SignedCms(new ContentInfo(envelopedCms.ContentInfo.Content));
            m_cms = signedCms;

            // Potpisinka nema.
            m_signers = [];

            m_messageState = MessageState.Plain;
        }
        else
            throw new UnsupportedCmsContentTypeException( contentTypeOid.Value );
    }

    /// <summary>
    /// Pročita sadržaj poruke.
    /// </summary>
    /// <exception cref="InvalidMessageStateException">
    ///     Kuverta je još uvijek zatvorena, tj. poruka još nije dešifrirana.</exception>
    /// <returns>
    /// <para>
    ///     Vrati sadržaj poruke.</para>
    /// </returns>
    public byte[] Read()
    {
        switch(m_messageState)
        {
            case MessageState.Plain:
                Debug.Assert( m_cms is SignedCms );
                return ((SignedCms)m_cms).ContentInfo.Content;

            case MessageState.Signed:
                Debug.Assert( m_cms is SignedCms );
                return ((SignedCms)m_cms).ContentInfo.Content;

            case MessageState.ReceivedEnvelope:
                // Poruka nije dešifrirana.
                throw new InvalidMessageStateException( MessageState.Enveloped.ToString(), m_messageState.ToString() );

            case MessageState.Enveloped:
                Debug.Assert( m_cms is EnvelopedCms );
                return ((EnvelopedCms)m_cms).ContentInfo.Content;
        }
        Debug.Assert( false );
        return [];
    }

    /// <summary>
    /// Potpiše poruku.
    /// </summary>
    ///
    /// <param name="certificate">
    ///     Certifikat potpisnika koji mora sadržavati privatni ključ.</param>
    ///
    /// <param name="signingTime">
    ///     Vrijeme potpisa koje se dodaje kao potpisani atribut CMS zapisa.</param>
    ///
    /// <exception cref="InvalidMessageStateException">
    ///     Ne može se dva puta potpisati, niti potpisivati već kuvertiranu poruku.</exception>
    ///
    /// <remarks>
    /// <para>
    ///     U potpisanu poruku može se uz potpisni certifikat uključiti i dio ili sve certifikate iz njegovog lanca
    ///     povjerenja. Ova metoda radi sukladno prešutnom izboru .NET-a, tj. uključi sve certifikate osim korijenskog.
    ///     Korijenski certifikat bi već trebao postojati kao certifikat od povjerenja na računalu koje vrši validaciju
    ///     certifikata.</para>
    /// <para>
    ///     Algoritam digitalnog sažetka odredi se svojstvom <see cref="Pkcs7Options.DefaultDigestAlgorithms"/> pri
    ///     konstrukciji zadanih opcija, a inače implementacijom prešutno zadani algoritam (v. <see
    ///     cref="Pkcs7Options.DigestAlgorithms"/> za detalje).</para>
    /// <para>
    ///     Potisne prikaz korisničkih sučelja, poput dijaloškog okvira za unos PIN-a, sukladno svojstvu <see
    ///     cref="Pkcs7Options.SilentUi"/>, ali u slučaju da je prikaz bio nužan (npr. PIN nije već upisan u
    ///     međuspremnik implementirajućeg kriptografkog modula), operacija će završiti iznimkom <see
    ///     cref="CryptographicException"/>. Sučelje može biti potisnuto i kontekstom izvršavanja operacije, npr.
    ///     procesi Windowsovih servisa obično nemaju asociranu radnu površinu i ne mogu prikazivati korisnička
    ///     sučelja.</para>
    /// <para>
    ///     Tekst poruke očitan metodom <see cref="Read"/> odmah po potpisivanjeu prikladan je za zapisivanje u datoteku
    ///     .p7m ekstenzije prema RFC 2311, iako se često zapisuje u datoteke .p7s ekstenzija (te datoteke bi zapravo
    ///     trebale sadržavati samo potpis, bez potpisanog sadržaja). Za MIME tip koristiti treba
    ///     "application/pkcs7-mime", a za S/MIME tip "signed-data":
    ///     <code>
    ///         Content-Type: application/pkcs7-mime; smime-type=signed-data; name="smime.p7m"
    ///         Content-Transfer-Encoding: base64 Content-Disposition: attachment; filename="smime.p7m"
    ///     </code>
    ///     </para>
    /// </remarks>
    ///
    public void Sign(
          X509Certificate2 certificate
        , DateTimeOffset signingTime
        )
    {
        // Potpisati može samo još nepotpisanu i nekuvertiranu poruku.
        if(MessageState.Plain != m_messageState)
            throw new InvalidMessageStateException( expected: MessageState.Plain.ToString(), actual: m_messageState.ToString() );
        // Potpisnik mora biti specificiran.
        if(certificate is null) throw new ArgumentNullException( nameof( certificate ) );

        // Pripremi potpis s vremenom potpisa i algoritmom za izradu digitalnog sažetka.
        var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, certificate);
        signer.SignedAttributes.Add( new Pkcs9SigningTime( signingTime.UtcDateTime ) );
        SetProperDigestAlgorithm( signer, certificate, m_options.DefaultDigestAlgorithms );

        // Potpiše.
        var signedCms = m_cms as SignedCms;
        Debug.Assert( signedCms is not null );
        signedCms!.ComputeSignature( signer, silent: m_options.SilentUi );

        // Ažurira stanje sadržaja i upravo specificirani identitet potpisnika.
        m_signers = RetrieveSignerIdentities( signedCms.SignerInfos, m_options );
        m_messageState = MessageState.Signed;
    }

    /// <summary>
    /// Potpiše poruku uz trenutno sistemsko UTC vrijeme kao vrijeme potpisa.
    /// </summary>
    ///
    /// <param name="certificate">
    ///     Certifikat potpisnika koji mora sadržavati privatni ključ.</param>
    ///
    /// <remarks>
    /// <para>
    ///     V. <see cref="Sign(X509Certificate2)"/> za više detalja.</para>
    /// </remarks>
    ///
    public void Sign(
          X509Certificate2 certificate
        )
        => Sign( certificate, DateTimeOffset.UtcNow );

    /// <summary>
    /// Ovjeri potpise poruke i opcionalno provjeri valjanost certifikata u lancu povjerenja potpisnih certifikata.
    /// </summary>
    ///
    /// <exception cref="InvalidMessageStateException">
    ///     Verificirati se može samo potpisana poruka, a kuvertiranu je potrebno prvo otvoriti.</exception>
    ///
    /// <remarks>
    /// <para>
    ///     Metoda ovjeri potpise svih potpisinika i supotpisnika, na sadržaju i potpisanim atributima. Opcionalno
    ///     provjeri i valjanost certifikata u lancu povjerenja (kod potpisivanja se u SignedData podatak uključuje
    ///     certifikat kojim je izvršeno potpisivanje, a opcionalno se mogu uključiti i dio ili svi certifikati iz
    ///     njegovog lanca povjerenja). Valjanost lanca povjerenja provjeri sukladno stanju svojstva <see
    ///     cref="Pkcs7Options.TrustCertificates"/> pri konsktrukciji zadanih opcija.</para>
    /// <para>
    ///     Za postupak ovjere potpisa mora se certifikat potpisnika nalaziti u dostupnim spremišitima, a korijenski
    ///     certifikat biti u spremištu povjerljivih certifikata korijenskih ustanova.</para>
    /// </remarks>
    ///
    public void Verify()
    {
        if(MessageState.Signed != m_messageState)
            throw new InvalidMessageStateException( expected: MessageState.Signed.ToString(), actual: m_messageState.ToString() );

        var cms = m_cms as SignedCms;
        Debug.Assert( cms is not null );
        cms!.CheckSignature( verifySignatureOnly: m_options.TrustCertificates );
    }

    /// <summary>
    /// Objekt za Pkcs #7 operaciju sukladan diskriminatoru <see cref="m_messageState"/>: ili referenca na <see
    /// cref="SignedCms"/> objekt ili referenca na <see cref="EnvelopedCms"/> objekt. Objekt je uvijek u dekodiranom
    /// stanju.
    /// </summary>
    protected object m_cms;

    /// <summary>
    /// Stanje sadržaja poruke, je li u pitanju poruka koja se šalje ili poruka koja je primljena i je li primljena
    /// poruka još šifrirana ili već dešifrirana.
    /// </summary>
    protected MessageState m_messageState;

    /// <summary>
    /// Dodatne opcije za usmjeravanje rada metoda objekta, vidi <see cref="Pkcs7Options"/>.</param>
    /// </summary>
    protected Pkcs7Options m_options;

    /// <summary>
    /// Vrati opisnike identiteta primatelja.
    /// </summary>
    ///
    /// <param name="recipients">
    ///     Kolekcija primatelja.</param>
    ///
    /// <param name="options">
    ///     Opcije koje upravljaju radom ove metode, ne koristi se trenutno.</param>
    ///
    /// <returns>
    ///     Vrati opisnike identiteta svih primatelja koji su identificirani izdavačem i serijskim brojem
    ///     cerifikata ili praznu listu ako je kolekcija potpisnika prazna.</returns>
    ///
    /// <exception cref="NoIdentifiableRecipientsException">
    ///     Niti jedan od primatelja poruke nije identifican preko izdavača i serijskog broja certifikata.</exception>
    ///
    private static List<X509IssuerSerial> RetrieveRecipientIdentities(
          RecipientInfoCollection recipients
        , Pkcs7Options options
        )
    {
        List<X509IssuerSerial> retVal = [];
        if(0 == recipients.Count)
            return retVal;
        for(var i = 0; i < recipients.Count; i++)
        {
            var recipient = recipients[i].RecipientIdentifier;
            if(SubjectIdentifierType.IssuerAndSerialNumber != recipient.Type)
                continue;
            if(recipient.Value is null)
                continue;
            Debug.Assert( recipient.Value is X509IssuerSerial );
            retVal.Add( (X509IssuerSerial)recipient.Value );
        }
        if(0 == retVal.Count)
            throw new NoIdentifiableRecipientsException( recipients.Count );
        return retVal;
    }

    /// <summary>
    /// Vrati opisnike identiteta potpisnika.
    /// </summary>
    ///
    /// <param name="signers">
    ///     Kolekcija potpisnika.</param>
    ///
    /// <param name="options">
    ///     Opcije koje upravljaju radom ove metode, trenutno se ne koristi.</param>
    ///
    /// <returns>
    ///     Vrati opisnike identiteta svih potpisnika koji su identificirani izdavačem i serijskim brojem
    ///     cerifikata.</returns>
    ///
    /// <exception cref="DegenerateSignatureException">
    ///     Kolekcija ne sadrži niti jednog potpisnika, izgledno se radi o degeneriranom SignedData tipu za prijenos
    ///     certifikata i drugog sigurnosnog materijala.</exception>
    ///
    /// <exception cref="NoIdentifiableSignersException">
    ///     Niti jedan od potpisnika poruke nije identifican preko izdavača i serijskog broja certifikata.</exception>
    ///
    private static List<X509IssuerSerial> RetrieveSignerIdentities(
          SignerInfoCollection signers
        , Pkcs7Options options
        )
    {
        if(0 == signers.Count)
            throw new DegenerateSignatureException();
        List<X509IssuerSerial> retVal = [];
        for(var i = 0; i < signers.Count; i++)
        {
            var signer = signers[i].SignerIdentifier;
            if(SubjectIdentifierType.IssuerAndSerialNumber != signer.Type)
                continue;
            if(signer.Value is null)
                continue;
            Debug.Assert( signer.Value is X509IssuerSerial );
            retVal.Add( (X509IssuerSerial)signer.Value );
        }
        if(0 == retVal.Count)
            throw new NoIdentifiableSignersException( signers.Count );
        return retVal;
    }

    /// <summary>
    /// Izabire prikladni algoritam digitalnog sažetka pri potpisivanju, tj. izradi SignedData tipa.
    /// </summary>
    ///
    /// <param name="signer">
    ///     Objekt za potpisivanje kojem pridruži algoritam digitalnog sažetka.</param>
    ///
    /// <param name="cert">
    ///     Certifikat kojim će se potpisati sadržaj. Algoritam digitalnog sažetka izabere prema vrsti ključa
    ///     certifikata.</param>
    ///
    /// <param name="defaults">
    ///     Algoritmi digitalnog sažetka koje treba koristiti za različite vrste ključeva (v. <see
    ///     cref="Pkcs7Options"/>).</param>
    ///
    private static void SetProperDigestAlgorithm(
          CmsSigner signer
        , X509Certificate2 cert
        , Pkcs7Options.DigestAlgorithms defaults
        )
    {
        switch(CryptoHelpers.GetProviderType( cert ))
        {
            case CryptoProviderType.RsaAkdshCsp:
                signer.DigestAlgorithm = Oid.FromFriendlyName( "sha1", OidGroup.HashAlgorithm );
                break;
            case CryptoProviderType.RsaCsp:
                if(defaults.RsaCsp is not null)
                    signer.DigestAlgorithm = defaults.RsaCsp;
                break;
            case CryptoProviderType.RsaKsp:
                if(defaults.RsaKsp is not null)
                    signer.DigestAlgorithm = defaults.RsaKsp;
                break;
            case CryptoProviderType.Ecdsa256:
                signer.DigestAlgorithm = defaults.Ecdsa256 ?? Oid.FromFriendlyName( "sha256", OidGroup.HashAlgorithm );
                break;
            case CryptoProviderType.Ecdsa384:
                signer.DigestAlgorithm = defaults.Ecdsa384 ?? Oid.FromFriendlyName( "sha384", OidGroup.HashAlgorithm );
                break;
            case CryptoProviderType.Ecdsa521:
                signer.DigestAlgorithm = defaults.Ecdsa521 ?? Oid.FromFriendlyName( "sha512", OidGroup.HashAlgorithm );
                break;
        }
    }

    /// <summary>
    /// Pronađe primatelja po izdavaču i serijskom broju certifikata.
    /// </summary>
    ///
    /// <param name="recipients">
    ///     Kolekcija primatelja.</param>
    ///
    /// <param name="ident">
    ///     Izdavač i serijski broj certifikata primatelja.</param>
    ///
    /// <param name="recipient">
    ///     Pronađeni primatelj ili <see langword="null"/> ako primatelja ne nađe.</param>
    ///
    /// <returns>
    ///     Vrati <see langword="true"/> kad primatelja nađe, inače <see langword="false"/>.</returns>
    ///
    private static bool TryFindRecipient(
          RecipientInfoCollection recipients
        , X509IssuerSerial ident
        , out RecipientInfo? recipient
        )
    {
        foreach(var candidate in recipients)
        {
            if(SubjectIdentifierType.IssuerAndSerialNumber != candidate.RecipientIdentifier.Type)
                continue;
            if(candidate.RecipientIdentifier.Value is null)
                continue;
            Debug.Assert( candidate.RecipientIdentifier.Value is X509IssuerSerial );
            var candidateIdent = (X509IssuerSerial)candidate.RecipientIdentifier.Value;
            if(ident.IssuerName != candidateIdent.IssuerName)
                continue;
            if(ident.SerialNumber != candidateIdent.SerialNumber)
                continue;
            recipient = candidate;
            return true;
        }
        recipient = null;
        return false;
    }

    /// <summary>
    /// Pronađe potpisnika po izdavaču i serijskom broju certifikata.
    /// </summary>
    ///
    /// <param name="signers">
    ///     Kolekcija potpisnika.</param>
    ///
    /// <param name="ident">
    ///     Izdavač i serijski broj certifikata potpisnika.</param>
    ///
    /// <param name="signer">
    ///     Pronađeni potpisnik ili <see langword="null"/> ako potpisnika ne nađe.</param>
    ///
    /// <returns>
    ///     Vrati <see langword="true"/> kad potpisnika nađe, inače <see langword="false"/>.</returns>
    ///
    private static bool TryFindSigner(
          SignerInfoCollection signers
        , X509IssuerSerial ident
        , out SignerInfo? signer
        )
    {
        foreach(var candidate in signers)
        {
            if(SubjectIdentifierType.IssuerAndSerialNumber != candidate.SignerIdentifier.Type)
                continue;
            if(candidate.SignerIdentifier.Value is null)
                continue;
            Debug.Assert( candidate.SignerIdentifier.Value is X509IssuerSerial );
            var candidateIdent = (X509IssuerSerial)candidate.SignerIdentifier.Value;
            if(ident.IssuerName != candidateIdent.IssuerName)
                continue;
            if(ident.SerialNumber != candidateIdent.SerialNumber)
                continue;
            signer = candidate;
            return true;
        }
        signer = null;
        return false;
    }
}

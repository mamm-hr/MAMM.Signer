using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Shared;

/// <summary>
/// Definicija potrebnih operacija s certifikatima.
/// </summary>
///
public interface ICertificateManager
{
    /// <summary>
    /// Učita certifikate iz specificirane lokacije u pripremi skupa certifikata nad kojim rade ostale operacije.
    /// </summary>
    ///
    /// <param name="location">
    ///     Spremište iz kojeg se bira certifikat.</param>
    ///
    /// <param name="includeCsp">
    ///     Ako je ovo istina, a <paramref name="location"/> je <see cref="CertificateLocation.SmartCardReaders"/>, onda
    ///     se certifikate na karticama traži i kroz staro CSP sučelje.</param>
    ///
    public void LoadCertificates(
          CertificateLocation location
        , bool includeCsp
        );

    /// <summary>
    /// Nađe certifikat po digitalnom otisku u skupu certifikata pripremljenom metodom <see
    /// cref="LoadCertificates(CertificateLocation, bool)"/> .
    /// </summary>
    ///
    /// <param name="thumbprint">
    ///     Digitalni otisak certifikata kojeg se nalazi u specificiranom spremištu. Ako nije dano, prikaže korisničko
    ///     sučelje za izbor certifikata. Ako je dano, a certifikat ne nađe, ponaša se kao da nije dano.</param>
    ///
    /// <param name="validOnly">
    ///     Ako je <see langword="true"/> vraćeni certifikat mora biti valjan (neistakeo).</param>
    ///
    /// <returns>
    ///     Vrati referencu na uspješno nađeni certifikat ili <see langword="null"/> ako certifikat nije
    ///     nađen.</returns>
    ///
    X509Certificate2? FindCertificate(
          string thumbprint
        , bool validOnly = false
        );

    /// <summary>
    /// Izabere certifikat za kriptografsku operaciju iz skupa certifikata pripremljenog metodom <see
    /// cref="LoadCertificates(CertificateLocation, bool)"/> .
    /// </summary>
    ///
    /// <param name="purpose">
    ///     Koristi se samo ako se prikazuje korisničko sučelje i osigurava da se korisniku ponude samo oni certifikati
    ///     iz zadanog spremišta koji imaju ovim parametrom navedenu svrhu.</param>
    ///
    /// <param name="validOnly">
    ///     Ako je <see langword="true"/> vraćeni certifikat mora biti valjan (neistakeo).</param>
    ///
    /// <param name="title">
    ///     Naslov dijaloškog okvira za izbor certifikata.</param>
    ///
    /// <param name="message">
    ///     Poruka korisniku na dijaloškom okviru za izbor certifikata.</param>
    ///
    /// <returns>
    ///     Vrati referencu na izabrani certifikat ili <see langword="null"/> ako certifikat nije izabran od
    ///     korisnika.</returns>
    X509Certificate2? SelectCertificate(
          CertificatePurpose purpose
        , bool validOnly
        , string? title
        , string? message
        );

    /// <summary>
    /// Vrati neslužbeni naziv certifikata ako je upisan, inače predmet certifikata.
    /// </summary>
    ///
    /// <param name="cert">
    ///     Certifikat u pitanju.</param>
    ///
    /// <returns>
    ///     Vraća string koji sadrži neslužbeni ili jednostavni naziv certifikata.</returns>
    string GetFriendlyOrSubjectName(
          X509Certificate2 cert
        );
}

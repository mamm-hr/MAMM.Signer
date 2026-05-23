using MAMM.Signer.Shared;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_ICertificates = "51B87EA2-D7B0-4335-887B-EB99E6AC5179"; // Usklađivati s IDL datotekom.
}

[ComVisible(true)]
[Guid(InteropGuids.IID_ICertificates)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface ICertificates
{
    /// <summary>
    /// Učita certifikate iz specificirane lokacije u pripremi skupa certifikata nad kojim rade ostale operacije.
    /// </summary>
    ///
    /// <param name="location">
    ///     Spremište iz kojeg se bira certifikat.</param>
    ///
    /// <param name="includeCsp">
    ///     Ako je ovo istina, a <paramref name="location"/> je <see cref="CoCertificateLocation.SmartCardReaders"/>,
    ///     onda se certifikate na karticama traži i kroz staro CSP sučelje.</param>
    ///
    [DispId( 1 )]
    void LoadCertificates(
          CoCertificateLocation location
        , bool includeCsp
        );

    /// <summary>
    /// Nađe certifikat po digitalnom otisku u skupu certifikata pripremljenom metodom <see
    /// cref="LoadCertificates(CoCertificateLocation, bool)"/> .
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
    [DispId( 2 )]
    ICertificate? FindCertificate(
          string thumbprint
        , bool validOnly
        );

    /// <summary>
    /// Izabere certifikat za kriptografsku operaciju.
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
    ///     Vrati referencu na uspješno selektirani certifikat ili <see langword="null"/> ako certifikat nije nađen niti
    ///     izabran od korisnika.</returns>
    [DispId( 3 )]
    ICertificate? SelectCertificate(
          CoCertificatePurpose purpose
        , bool validOnly
        , string title
        , string message
        );
}

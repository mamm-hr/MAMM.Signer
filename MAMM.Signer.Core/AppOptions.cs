using MAMM.Signer.Shared;
using System.Security.Cryptography;

namespace MAMM.Signer.Core;

/// <summary>
/// Izvršne opcije programa.
/// </summary>
///
/// <remarks>
/// <para>
///     Za precizniji opis vidi README.md projekta CLI programa.</para>
/// </remarks>
///
public class AppOptions
{
    /// <summary>
    /// U popise za izbor certifikata uključi i nevaljale (npr. istekle) certifikate.
    /// </summary>
    public bool AllowInvalid { get; set; } = false;

    /// <summary>
    /// Izlaznu datoteku šifrira.
    /// </summary>
    public bool Encrypt { get; set; } = false;

    /// <summary>
    /// Algoritam simetričnog ključa za šifriranje izlazne, odnosno dešifriranje ulazne datoteke.
    /// </summary>
    public Oid? EncryptAlg { get; set; } = null;

    /// <summary>
    /// Digitalni otisak certifikata primatelja.
    /// </summary>
    public string? EncryptCert { get; set; } = null;

    /// <summary>
    /// Lokacija certifikata primatelja.
    /// </summary>
    public CertificateLocation EncryptLoc { get; set; } = CertificateLocation.CurrentUser;

    /// <summary>
    /// Ekstenzija (s točkom) koju program dodaje na puno ime (uključujući i ekstenziju) ulazne datoteke.
    /// </summary>
    public string Ext { get; set; } = ".p7m";

    /// <summary>
    /// U popise za izbor certifikata uključi sve certifikate, a ne samo one koje odgovaraju svrsi za koju se izabire
    /// certifikat.
    /// </summary>
    public bool IgnorePurpose { get; set; } = false;

    /// <summary>
    /// Uključi u pregled ili traženje certifikata na pametnim karticama i one dostupne samo kroz CSP.
    /// </summary>
    public bool IncludeCsp { get; set; } = false;

    /// <summary>
    /// Ulazne se datoteke specificiraju datotekom s popisom ulaznih datoteka.
    /// </summary>
    public bool SpecList { get; set; } = false;

    /// <summary>
    /// Direktorij u koji zapiše izlazne datoteke.
    /// </summary>
    public string? OutDir { get; set; } = null;

    /// <summary>
    /// Ulazni dokument potpiše.
    /// </summary>
    public bool Sign { get; set; } = false;

    /// <summary>
    /// Digitalni otisak potpisnog certifikata.
    /// </summary>
    public string? SignCert { get; set; } = null;

    /// <summary>
    /// Lokacija potpisnog certifikata.
    /// </summary>
    public CertificateLocation SignLoc { get; set; } = CertificateLocation.CurrentUser;

    /// <summary>
    /// Datum i vrijeme potpisivanja.
    /// </summary>
    public DateTimeOffset? SignTime { get; set; } = null;

    /// <summary>
    /// Ne prikaže sistemski dijaloški okvir za izbor certifikata.
    /// </summary>
    public bool SilentUi { get; set; } = false;

    /// <summary>
    ///  Otvara kuvertu (dešifrira) i ovjerava potpis na dokumentu u ulaznoj datoteci, odnosno vrši otvaranje kuverte i
    ///  ovjeru potpisa na izlaznoj datoteci kad se kombinira s <see cref="Encrypt"/> ili <see cref="Sign"/>.
    /// </summary>
    public bool Verify { get; set; } = false;
}

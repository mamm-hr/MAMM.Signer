using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Core;

/// <summary>
/// Ishod obrade svih ulaznih daoteka.
/// </summary>
public class AppResult
{
    /// <summary>
    /// Certifikat primatelja koji je zadan od korisnika ili <see langword="null"/> ako korisnik nije tražio šifriranje.
    /// </summary>
    public X509Certificate2? EncryptCert { get; set; } = null;

    /// <summary>
    /// Objekt iznimke zbog koje je obrada ulaznih datoteka prekinuta ili <see langword="null"/> ako nije prekinuta.
    /// </summary>
    public Exception? Exception { get; set; } = null;

    /// <summary>
    /// Opisnik ishoda obrade ulazne datoteke na kojoj je obrada ulaznih datoteka prekinuta ili <see langword="null"/>
    /// ako nije prekinuta.
    /// </summary>
    public OpResult? FailedOp { get; set; } = null;

    /// <summary>
    /// Opisnik ishoda obrade pojedinačne ulazne datoteke.
    /// </summary>
    ///
    /// <param name="inputFile">
    ///     Opisnik ulazne datoteke čiji je ovaj objekt ishod obrade.</param>
    public class OpResult(FileInfo inputFile)
    {
        /// <summary>
        /// Istbina ako je datoteka prije ovjere trebala biti dešifrirana.
        /// </summary>
        public bool DataDecrypted { get; set; } = false;

        /// <summary>
        /// <summary>
        /// Istina ako je obrada datoteke proizvela potpisani/šifrirani podatak. Pri ovjeri ulaznih datoteka ovo je
        /// uvijek laž.
        /// </summary>
        public bool DataProduced { get; set; } = false;

        /// <summary>
        /// Istina ako je pri obradi datoteke obavljena ovjera.
        /// </summary>
        public bool DataVerified { get; set; } = false;

        /// Certifikat primatelja koji je korišten za dešifriranje ili <see langword="null"/> ako ulazna datoteka,
        /// odnosno proizvedeni podatak nije ovjeravan (i dešifriran) u ovoj operaciji, odnosno ako je dešifriran bez
        /// zadavanja primatelja, pa je izabran iz korisnikovog spremišta osobnih certifikata prvi pronađeni primatelj.
        /// </summary>
        public X509Certificate2? DecryptCert { get; set; } = null;


        /// Certifikat primatelja koji je korišten za šifriranje ili <see langword="null"/> ako datoteka nije šifrirana
        /// u ovoj operaciji.
        /// </summary>
        public X509Certificate2? EncryptCert { get; set; } = null;

        /// <summary>
        /// Opisnik ulazne datoteke.
        /// </summary>
        public FileInfo InputFile { get; } = inputFile;

        /// <summary>
        /// Opisnik izlazne datoteke ili <see langword="null"/> ako izlazna datoteka nije proizvedena.
        /// </summary>
        public FileInfo? OutputFile { get; set; } = null;

        /// <summary>
        /// Potpisni certifikat koji je korišten ili <see langword="null"/> ako datoteka nije potpisana.
        /// </summary>
        public X509Certificate2? SignCert { get; set; } = null;

        /// <summary>
        /// Lokalni (ne UTC) datum i vrijeme potpisivanja ili <see langword="null"/> ako datoteke nisu potpisivane.
        /// </summary>
        public DateTime? SignDateTime { get; set; } = null;
    }

    /// <summary>
    /// Popis ishoda obrada ulaznih datoteka redoslijedom kojim su obrađivane.
    /// </summary>
    public readonly List<OpResult> OpResults = [];

    /// <summary>
    /// Potpisni certifikat koji je zadan od korisnika ili <see langword="null"/> ako korisnik nije tražio potpisivanje.
    /// </summary>
    public X509Certificate2? SignCert { get; set; } = null;
}

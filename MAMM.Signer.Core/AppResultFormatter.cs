using MAMM.Signer.Shared;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MAMM.Signer.Core;

/// <summary>
/// Formatira ishod rada programa za izvještavanje korisnika.
/// </summary>
///
/// <param name="certManager">
///     Sučelje komponente za pomoćne procedure koje se koriste pri formatiranju.</param>
///
public class AppResultFormatter(
      ICertificateManager certManager
    )
{
    /// <summary>
    /// Provjerava je li izvođenje operacija prekinuto pogreškom.
    /// </summary>
    ///
    /// <param name="result">
    ///     Objekt ishoda izvođenja.</param>
    ///
    /// <param name="message">
    ///     Opis pogreške kojom je prekinuto izvođenje ili prazni niz znakova.</param>
    ///
    /// <returns>
    ///     Vrati <see langword="true"/> ako je izvođenje prekinuto, inače <see langword="false"/>.</returns>
    ///
    public virtual bool FormatError(
          AppResult result
        , out string message
        )
    {
        message = "";
        if(result.Exception is null)
            return false;
        string description
            = result.Exception is SignerException signerEx
            ? string.Format(signerEx.Message, [.. signerEx.Data.Values.Cast<object>()])
            : result.Exception.Message
            ;
        if(result.FailedOp is null)
            message = string.Format( Resources.ReportError, description );
        else
            message = string.Format( Resources.ReportOpError, result.FailedOp.InputFile.FullName, description );
        return true;

    }

    /// <summary>
    /// Formatira početni tekst izvještaja koji opisuje generalni ishod izvršavanja.
    /// </summary>
    ///
    /// <param name="buffer">
    ///     Spremnik u koji se dodaje generirani teks.</param>
    ///
    /// <param name="result">
    ///     Objekt ishoda izvođenja.</param>
    ///
    /// <returns>
    ///     Vrati <see langword="true"/> ako je tekst generiran, a <see langword="false"/> ako ništa nije dopisano u
    ///     <paramref name="buffer"/>.</returns>
    ///
    public virtual bool FormatPreamble(
          StringBuilder buffer
        , AppResult result
        )
    {
        if(FormatError( result, out var errorMessage ))
            buffer.AppendLine( errorMessage );
        else if(0 < result.OpResults.Count)
        {
            buffer.AppendFormat( Resources.ReportOpPreamble, result.OpResults.Count( r => r.OutputFile is not null ) );
            buffer.AppendLine();
        }
        else
            return false;
        return true;
    }

    /// <summary>
    /// Formatira stavku izvještaja odnosnu na ishod operacije specificirane njenim indeksom u kolekciji ishoda
    /// operacija.
    /// </summary>
    ///
    /// <param name="buffer">
    ///     Spremnik u koji se dodaje generirani teks.</param>
    ///
    /// <param name="result">
    ///     Objekt ishoda izvođenja.</param>
    ///
    /// <param name="opIndex">
    ///     Indeks operacije za koju se formira izvještaj.</param>
    ///
    /// <returns>
    ///     Vrati <see langword="true"/> ako je tekst generiran, a <see langword="false"/> ako ne postoji unos pod tim
    ///     indeksom.</returns>
    ///
    public virtual bool Format(
          StringBuilder buffer
        , AppResult result
        , int opIndex
        )
    {
        if(opIndex < 0 || result.OpResults.Count <= opIndex)
            return false;
        this.Format( buffer, result.OpResults[opIndex] );
        return true;
    }

    /// <summary>
    /// Formatira stavku izvještaja odnosnu na ishod operacije.
    /// </summary>
    ///
    /// <param name="buffer">
    ///     Spremnik u koji se dodaje generirani teks.</param>
    ///
    /// <param name="r">
    ///     Objekt ishoda operacije.</param>
    ///
    public virtual void Format(
          StringBuilder buffer
        , AppResult.OpResult r
        )
    {
        buffer.AppendLine( r.InputFile.FullName );
        buffer.AppendLine();

        if(r.DataVerified)
        {
            buffer.AppendLine( r.DataDecrypted ? Resources.ReportVerifiedAndDecrypted : Resources.ReportVerified );
            buffer.AppendLine();
        }

        static void ReportCert(
              StringBuilder buffer
            , ICertificateManager certManager
            , string formatValid
            , string formatInvalid
            , X509Certificate2 cert
            , DateTime? signDateTime
            )
        {
            buffer.AppendFormat(
                  cert.Verify() ? formatValid : formatInvalid
                , certManager.GetFriendlyOrSubjectName( cert )
                , cert.SubjectName.Format( multiLine: false )
                , signDateTime
                );
            buffer.AppendLine();
            buffer.AppendLine();
        }

        if(r.SignCert is not null)
            ReportCert( buffer, certManager, Resources.ReportValidSignCert, Resources.ReportInvalidSignCert, r.SignCert, r.SignDateTime );

        if(r.EncryptCert is not null)
            ReportCert( buffer, certManager, Resources.ReportValidEncryptCert, Resources.ReportInvalidEncryptCert, r.EncryptCert, null );

        if(r.DecryptCert is not null)
            ReportCert( buffer, certManager, Resources.ReportValidDecryptCert, Resources.ReportInvalidDecryptCert, r.DecryptCert, null );

        if(!r.DataProduced && !r.DataVerified)
        {
            buffer.AppendLine( Resources.ReportFileNotProcessed );
            buffer.AppendLine();
        }
    }
}

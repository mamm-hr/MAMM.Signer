using MAMM.Signer.Shared;
using MAMM.Signer.Core;
using MAMM.Signer.Pkcs;
using Microsoft.Extensions.Configuration;

namespace MAMM.Signer.Cli;

/// <summary>
/// Aplikacijski objekt.
/// </summary>
///
/// <param name="args">
///     Programski argumenti skinuti s naredbenog retka prije nego je predan Host infrastrukturi na obradu.</param>
///
/// <param name="config">
///     Konfiguracijske postavke programa, očekivano zadane preko naredbenog retka. Specificirati se trebaju svojstva
///     <see cref="Core.AppOptions"/> objekta izvršnih opcija programa, a mogu svojstva <see cref="Pkcs7Options"/>
///     objekta koji usmjerava izvođenje PKCS #7 operacija. Za zadavanje preko naredbenog retka koristiti sintaksu koju
///     očekuje .NET Core Comman-line Configuration Provider (vidi i <see cref="AppArguments"/> za više detalja oko
///     sintakse).</param>
///
/// <param name="certManager">
///     Implementacija sučelja s operacijama nad certifikatima.</param>
///
/// <param name="appResultWriter">
///     Objekt za izvještavanje o ishodu rada.</param>
///
///
internal sealed class App(
      AppArguments args
    , IConfiguration config
    , ICertificateManager certManager
    , AppResultWriter appResultWriter
    )
{
    /// <summary>
    /// Izvršne opcije programa.
    /// </summary>
    public AppOptions? AppOptions { get => m_appOptions; }

    /// <summary>
    /// Implementira funkcionalnost programa.
    /// </summary>
    ///
    /// <returns>
    ///     Vrati opisnik ishoda rada aplikacije.</returns>
    public async Task<AppResult> RunAsync(
          CancellationToken cancellationToken = default
        )
    {
        // Ako nema programskih opcija, nikakva se radnja ne vrši.
        if(m_appOptions is null)
            return new();

        // Obradi datoteke zadane programskim opcijama programa.
        AppResult appResult = await AppOperations.RunOperationsAsync(
              m_appOptions.SpecList
                ? AppOperations.EnumerateInputFilesFromList( args.InputSpec )
                : AppOperations.EnumerateInputFilesFromSpec( args.InputSpec )
            , m_appOptions
            , m_pkcs7Options
            , certManager
            , cancellationToken
            );

        // Izvještava ishod rada.
        appResultWriter.Write( appResult );

        // Vraća ishod rada.
        return appResult;
    }

    /// <summary>
    /// Izvršne opcije programa.
    /// </summary>
    private readonly AppOptions? m_appOptions = config.Get<AppOptions>();

    /// <summary>
    /// Konfiguracijske opcije programa.
    /// </summary>
    private readonly Pkcs7Options? m_pkcs7Options = config.Get<Pkcs7Options>();

}

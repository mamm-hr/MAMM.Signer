using MAMM.Signer.Core;
using System.Diagnostics.CodeAnalysis;

namespace MAMM.Signer.Cli;

/// <summary>
/// Argumenti programa očitani s naredbednog retka.
/// </summary>
///
/// <remarks>
/// <para>
///     Naredbeni redak iza naziva programa sadrži programske argumente, a potom programske opcije. Ovaj razred čita
///     programske argumente, dok opcije čita .NET Core Comand-line Configuration Provider. Na primjer, u naredbenom
///     retku <c>MAMM.Signer.exe 114_Rn_9000102400001.P26 /sign /DefaultDigestAlgorithm:RsaKsp:FriendlyName sha1</c>
///     prvi argument "114_Rn_9000102400001.P26" čita ovaj razred, dok ostale očitava konfiguracijska infrastruktura
///     .NET-a.</para>
/// <para>
///     Prvi argument mora biti apsolutna ili relativna staza do specifikacije datoteka koje se programom
///     obrađuju.</para>
/// <para>
///     Programske opcije moraju imati sintaktički oblik definiran .NET Core dokumentacijom za konfiguraciju naredbenim
///     retkom tj. biti key/value parovi gdje ključ započinje kosom crtom (/), minusom (-) ili dvostrukim minusom (--),
///     ali format sa znakom jednakosti kao separatorom ključa od vrijednosti eksplicitno nije dopušten. Dopušteni su,
///     međutim, prekidači, tj. ključevi bez prateće vrijednosti koje će ovaj razred pretvoriti u key/value parove s
///     vrijednošću <see langword="true"/>.</para>
/// </remarks>
///
internal sealed class AppArguments
{
    /// <summary>
    /// Specifikacija ulaznih datoteka navedena kao prvi argument.
    /// </summary>
    public required FileInfo InputSpec;

    /// <summary>
    /// Kreira ovaj objekt iz argumenata naredbenog retka kako su predani ulaznoj točki programa.
    /// </summary>
    ///
    /// <param name="args">
    ///     Argumenti kako su dani na naredbenom retku.</param>
    ///
    /// <param name="options">
    ///     Programske opcije koje prate uzete programske argumente. Ovaj se niz može predati Application Builderu za
    ///     čitanje konfiguracije kroz NET. infrastrukturu.</param>
    ///
    /// <returns>
    ///     Vraća <see cref="AppArguments"/> objekt s uzetim programskim argumentima u svojstvima vraćenog
    ///     razreda.</returns>
    ///
    /// <exception cref="InputSpecArgumentMissingException">
    ///     Nije naveden prvi argument naredbenog retka: specifikacija ulaznih datoteka.</exception>
    ///
    /// <exception cref="ParameterMissingException">
    ///     U listi opcija navedena je vrijednost bez prethodećeg parametra.</exception>
    ///
    public static AppArguments TakeArguments(
          string[] args
        , out string[] options
        )
    {
        ArgumentNullException.ThrowIfNull( args );

        int argIter = 0;

        // Čita programske argumente do prvog parametra.

        List<string> argumentsList = [];
        for(; argIter < args.Length && !ParseParam( args[argIter], out _, out string argument ); argIter++)
            argumentsList.Add( argument );

        // U ovoj verziji, program ima samo jedan argument, obaveznu specifikaciju ulaznih datoteka.

        if(argumentsList.Count < 1)
            throw new InputSpecArgumentMissingException();
        var inputSpec = new FileInfo( argumentsList[0] );
        if(1 < argumentsList.Count)
            throw new TooManyArgumentsException( expected: 1, actual: argumentsList.Count );

        // Ostale argumente vraća natrag kao programske opcije, ali dopušta da prekidače tako što će parametru kojeg ne
        // slijedi njegova vrijednost dodati vrijednost istine.

        List<string> optionsList = [];
        string defaultSwitchValue = true.ToString();
        while(argIter < args.Length)
        {
            // Argumenti moraju biti u formatu {parametar vrijednost}... gdje parametar započinje prefiksom parametra (/
            // ili --). Dopušta prekidače, tj. parametre bez vrijednosti tako što za njih umetne praznu vrijednost.
            if(ParseParam( args[argIter++], out string? prefix, out string name ))
            {
                // Ovo je parametar koji mora biti na parnoj poziciji. Ako je na neparnoj, to znači da je prethodni
                // parametar (onaj na parnoj poziciji) bio prekidač, pa se umeče jedna prazna vrijednost da se ovaj
                // parametar dovede na parnu poziciju.
                if(1 == optionsList.Count % 2)
                    optionsList.Add( defaultSwitchValue );
                optionsList.Add( prefix + name );
            }
            else if(1 == optionsList.Count % 2)
            {
                // Ovo je vrijednost, ispravno na neparnoj poziciji.
                optionsList.Add( name );
            }
            else
            {
                // Vrijednosti na parnoj poziciji fali prethodeći parametar.
                throw new ParameterMissingException( name );
            }
        }

        // Ima neparan broj preostalih argumenata, onda je zadnji bio prekidač, pa dodaje praznu vrijednost.
        if(1 == optionsList.Count % 2)
            optionsList.Add( defaultSwitchValue );

        // Povratne vrijednosti.

        options = [.. optionsList];
        return new AppArguments
        {
            InputSpec = inputSpec,
        };
    }

    /// <summary>
    /// Prešutni konstruktor.
    /// </summary>
    private AppArguments() { }

    /// <summary>
    /// Vraća istinu ako argument započinje sa znakom koji označava da se radi o imenovanom parametru.
    /// </summary>
    ///
    /// <param name="arg">
    ///     Argument s naredbenog retka.</param>
    ///
    /// <param name="prefix">
    ///     Prefiks kojim je argument označen kao parametar - kosa crta (/), minus (-) ili dvostruki minus (--) ili <see
    ///     langword="null"/> ako argument nije parametar.</param>
    ///
    /// <param name="name">
    ///     Ostatak teksta argumenta iza prefiksa ili cijeli tekst argumenta ako argument nije parametar.</param>
    ///
    /// <returns>
    ///     Vraća <see langword="true"/> ako argument započinje znakom kose crte (/), minusa (-) ili dvostrukog minusa
    ///     (--), inače vraća <see langword="false"/>.</returns>
    ///
    private static bool ParseParam(
          string arg
        , [NotNullWhen(true)] out string? prefix
        , out string name
        )
    {
        int prefixLen;
        if(0 < arg.Length)
        {
            if('/' == arg[0])
                prefixLen = 1;
            else if('-' == arg[0])
            {
                if(1 < arg.Length)
                {
                    if('-' == arg[1])
                        prefixLen = 2;
                    else
                        prefixLen = 1;
                }
                else prefixLen = 1;
            }
            else prefixLen = 0;
        }
        else prefixLen = 0;
        if( 0 == prefixLen )
        {
            prefix = null;
            name = arg;
        }
        else
        {
            prefix = arg[..prefixLen];
            name = arg[prefixLen..];
            if(0 == name.Length)
                throw new NamelessParameterException( arg );
        }
        return 0 != prefixLen;
    }
}

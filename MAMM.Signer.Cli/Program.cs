using MAMM.Signer.Cli;
using MAMM.Signer.Core;
using MAMM.Signer.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Ulazna točka programa.
try
{
    // Očitava programske argumente koji se ne predaju preko hosting infrastrukture i uz njih vraća ostatak naredbenog
    // retka koji čini programske opcije za čitanje kroz hosting infrastrukutru.
    var appArguments = AppArguments.TakeArguments(args, out var appOptions);

    // Gradi hosting infrastrukturu.
    var builder = Host.CreateEmptyApplicationBuilder(settings: new()
    {
        ApplicationName = typeof(Program).Namespace,
        Args = appOptions,
        DisableDefaults = true,
    });
    builder.Services.AddSingleton( appArguments );
    builder.Services.AddSingleton<ICertificateManager>( new CertificateManager() );
    builder.Services.AddTransient<AppResultFormatter>();
    builder.Services.AddTransient<AppResultWriter>();
    builder.Services.AddTransient<App>();
    using var host = builder.Build();

    // Izvršava program.
    var app = host.Services.GetRequiredService<App>();
    var appResult = await app.RunAsync();

    // Kraj sukladan ishodu rada.
    return 0;
}
catch(SignerException ex)
{
    Console.Error.WriteLine( ex.Message, ex.Data.Values.Cast<object>().ToArray() );
    return 1;
}
catch(Exception ex)
{
    Console.Error.WriteLine( MAMM.Signer.Cli.Resources.UnhandledException );
    Console.Error.WriteLine( ex.Message );
    return 1;
}

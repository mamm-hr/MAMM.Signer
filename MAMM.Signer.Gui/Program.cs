using MAMM.Signer.Shared;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MAMM.Signer.Gui;

internal static class Program
{
    public static readonly Font DefaultFont = new("Segoe UI", 9F);

    public static readonly CertificateManager CertificateManager = new();

    public static T LoadJsonOrDefault<T>( string fileName ) where T : new()
    {
        string path = GetAppDataPath(fileName);
        if(!File.Exists( path ))
            return new T();
        try
        {
            return JsonSerializer.Deserialize<T>( File.ReadAllText( path ), m_jsonOptions ) ?? new T();
        }
        catch(JsonException)
        {
            return new T(); // corrupted file — fall back to defaults
        }
    }

    public static void SaveJson<T>( string fileName, T obj )
        => File.WriteAllText( GetAppDataPath(fileName), JsonSerializer.Serialize(  obj, m_jsonOptions ) );


    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault( false );
        Application.Run( new MainWindow() );
    }

    private class OidJsonConverter : JsonConverter<Oid>
    {
        public override Oid? Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
        {
            string? value = reader.GetString();
            return value is null ? null : new Oid( value );
        }

        public override void Write( Utf8JsonWriter writer, Oid value, JsonSerializerOptions options )
            => writer.WriteStringValue( value.Value );
    }

    private static readonly JsonSerializerOptions m_jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new OidJsonConverter() }
    };

    private static string GetAppDataPath( string fileName )
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company
                     ?? throw new InvalidOperationException();
        string product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product
                     ?? throw new InvalidOperationException();
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            company, product);
        Directory.CreateDirectory( folder );
        return Path.Combine( folder, fileName );
    }
}

using MAMM.Signer.Core;
using MAMM.Signer.Pkcs;

namespace MAMM.Signer.Gui;

internal class Settings()
{
    public AppOptions AppOptions { get; set; } = new();

    public Pkcs7Options Pkcs7Options { get; set; } = new();

    public static Settings Load()
        => Program.LoadJsonOrDefault<Settings>( m_fileName );

    public void Save()
        => Program.SaveJson( m_fileName, this );

    private readonly static string m_fileName = nameof(Settings) + ".json";
}

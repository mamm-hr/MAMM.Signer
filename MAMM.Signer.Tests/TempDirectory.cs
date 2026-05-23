using System.Diagnostics;

namespace MAMM.Signer.Tests;

internal class TempDirectory( string? prefix = null ) : IDisposable
{
    public string FullName => this.Info.FullName;

    public readonly DirectoryInfo Info = Directory.CreateTempSubdirectory(prefix);

    public void Dispose()
    {
        try { this.Info.Delete(true); } catch { }
    }
}

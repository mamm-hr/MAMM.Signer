namespace MAMM.Signer.Tests;

internal class TempFile : IDisposable
{
    public readonly FileInfo Info = new(Path.GetTempFileName());

    public void Dispose()
    {
        try { this.Info.Delete(); } catch { }
    }
}

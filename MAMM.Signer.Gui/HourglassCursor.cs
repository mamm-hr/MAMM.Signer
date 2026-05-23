namespace MAMM.Signer.Gui;

internal class HourglassCursor( Form form ) : IDisposable
{
    public void Dispose() => form.Cursor = m_cursor;
    private readonly Cursor m_cursor = form.Cursor;
}

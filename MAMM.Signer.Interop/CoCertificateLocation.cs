using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_ICertificateLocation = "4C0CE2BC-4CDE-4AF3-AD12-FC89304ACA4B"; // Usklađivati s IDL datotekom.
}

/// <summary>
/// Lokacije u kojima se mogu tražiti certifikati.
/// </summary>
[ComVisible(true)]
[Guid(InteropGuids.IID_ICertificateLocation)]
public enum CoCertificateLocation
{
    /// <summary>
    /// U spremištu prijavljenog korisnika.
    /// </summary>
    CurrentUser = 0,                            // Usklađivati vrijednosti s IDL datotekom.

    /// <summary>
    /// U spremištu lokalnog računala.
    /// </summary>
    LocalMachine = 1,

    /// <summary>
    /// U spojenim kriptografskim uređajima.
    /// </summary>
    SmartCardReaders = 2,
}

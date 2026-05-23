using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_ICertificatePurpose = "25CA587D-0660-48AF-9C81-AF8747DCE170"; // Usklađivati s IDL datotekom.
}

/// <summary>
/// Moguće namjene cerifikata.
/// </summary>
[ComVisible(true)]
[Guid(InteropGuids.IID_ICertificatePurpose)]
public enum CoCertificatePurpose
{
    /// <summary>
    /// Namjena nije navedena ili nije bitna.
    /// </summary>
    Unspecified = 0,                            // Usklađivati vrijednosti s IDL datotekom.

    /// <summary>
    /// Certifikat je namijenjen identifikaciji, šifriranju i slično.
    /// </summary>
    Identification = 1,

    /// <summary>
    /// Certifikat je potpisni.
    /// </summary>
    Signature = 2,
}

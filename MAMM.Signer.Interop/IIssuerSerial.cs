using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_IIssuerSerial = "A626D58D-09E1-408F-BD8F-EA0AF332DA91"; // Usklađivati s IDL datotekom.
}

[ComVisible(true)]
[Guid(InteropGuids.IID_IIssuerSerial)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IIssuerSerial
{
    [DispId( 1 )] string IssuerName { get; }

    [DispId( 2 )] string SerialNumber { get; }
}

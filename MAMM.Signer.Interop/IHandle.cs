using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_IHandle = "F3017315-0FD7-483D-8751-58244FAB3F4D"; // Ne publicirati!
}

[ComVisible(true)]
[Guid(InteropGuids.IID_IHandle)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
internal interface IHandle
{
    [DispId( 1 )] int Handle { get; }
}

using System.Runtime.InteropServices;

namespace MAMM.Signer.Interop;

internal static partial class InteropGuids
{
    public const string IID_IApplication = "AFC93446-5450-49F2-AD61-6259CF4BFCD0"; // Usklađivati s IDL datotekom.
}

[ComVisible(true)]
[Guid(InteropGuids.IID_IApplication)]
[InterfaceType(ComInterfaceType.InterfaceIsDual)]
public interface IApplication
{
    [DispId( 1 )]
    ICertificates Certificates { get; }

    [DispId( 2 )]
    IPkcs7Oids Oids { get; }

    [DispId( 3 )]
    IPkcs7 CreatePkcs7();
}

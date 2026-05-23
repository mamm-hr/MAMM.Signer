using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAMM.Signer.Gui;

internal static class Helpers
{
    public static bool IsValidFileName( string fileName )
        => -1 == fileName.IndexOfAny( Path.GetInvalidFileNameChars() );
}

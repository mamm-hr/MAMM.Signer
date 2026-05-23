using System.ComponentModel;

namespace MAMM.Signer.Interop;

internal static class ComHelpers
{
    public enum VarTypes { Unsupported, VT_UI1, VT_VARIANT };

    public static byte[] FromVariantArray(
          Array varArr
        )
    {
        if(varArr is null)
            throw new ArgumentNullException( nameof( varArr ) );
        var byteArr = new byte[varArr.Length];
        for(int i = 0; i < varArr.Length; i++)
            byteArr[i] = System.Convert.ToByte( varArr.GetValue( i ) );
        return byteArr;
    }

    public static object[] ToVariantArray(
          byte[] byteArr
        )
    {
        if(byteArr is null)
            throw new ArgumentNullException( nameof( byteArr ) );
        object[] varArr = new object[byteArr.Length];
        for(int i = 0; i < byteArr.Length; i++)
            varArr[i] = byteArr[i];
        return varArr;
    }

    public static bool TryConvert(
          object from
        , out VarTypes vartype
        , out byte[]? to
        )
    {
        // SAFEARRAY of VT_UI1, e.g. Visual Basic 6.0 Byte().
        if(from is byte[] bytes)
        {
            vartype = VarTypes.VT_UI1;
            to = bytes;
            return true;
        }

        // Variant array, e.g. VBScript Array(1, 2, 3).
        if(from is Array arr)
        {
            vartype = VarTypes.VT_VARIANT;
            to = FromVariantArray( arr );
            return true;
        }

        vartype = VarTypes.Unsupported;
        to = null;
        return false;
    }

    public static object Convert(
          byte[] data
        , VarTypes vartype
        )
        => vartype switch
        {
            VarTypes.VT_UI1 => data,
            VarTypes.VT_VARIANT => ToVariantArray( data ),
            _ => throw new ArgumentException( Resources.ComHelpers_Convert_InvalidVarType ),
        };
}

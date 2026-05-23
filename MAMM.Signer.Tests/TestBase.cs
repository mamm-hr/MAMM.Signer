using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;

namespace MAMM.Signer.Tests;

public class TestBase
{
    protected static void Print( int indent, string line )
        => Console.WriteLine( BuildIndent( indent ) + line );

    protected static void Print( int indent, string name, int? no, string value )
        => Print( indent, name + (no is null ? "" : $"[{no}]") + " = " + value );

    protected static void Print<T>( int indent, string name, int? no, T value ) where T : struct
        => Print( indent, name, no, value.ToString() ?? "n/a" );

    protected static void Print( int indent, string name, int? no, byte[] value )
        => Print( indent, name, no, value is null ? "n/a" : Convert.ToHexString( value ) );

    protected static void Print( int indent, string name, int? no, X509Certificate2? value )
    {
        Print( indent++, name, no, "" );
        if(value is null) return;
        Print( indent, "Subject", null, value.Subject );
        Print( indent, "Issuer", null, value.Issuer );
        Print( indent, "Serial", null, value.SerialNumber );
    }

    protected static void Print( int indent, string name, int? no, X509IssuerSerial value )
    {
        Print( indent++, name, no, "" );
        Print( indent, "Issuer", null, value.IssuerName );
        Print( indent, "Serial", null, value.SerialNumber );
    }

    protected static void Print( int indent, string name, int? no, Oid v )
        => Print( indent, name, no, $"(oid:{v.Value}, {v.FriendlyName ?? "n/a"})" );

    private const string TAB_STR_0 = "|  ";
    private const string TAB_STR_1 = "+- ";
    private static string BuildIndent( int indent )
    {
        if(indent <= 0)
            return string.Empty;

        if(indent == 1)
            return TAB_STR_1;

        return string.Concat( Enumerable.Repeat( TAB_STR_0, indent - 1 ) ) + TAB_STR_1;
    }
}

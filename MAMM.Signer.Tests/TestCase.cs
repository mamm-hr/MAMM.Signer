namespace MAMM.Signer.Tests;

internal class TestCase
{
    public DateTimeOffset SignDateTime;
    public int SignCertNo;
    public string SignAlg = "";
    public int? CryptCertNo = null;
    public string? CryptAlg = null;
    public string ContentFileName = "";
    public string? MessageFileName = null;
}

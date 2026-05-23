using MAMM.Signer.Core;
using MAMM.Signer.Pkcs;

namespace MAMM.Signer.Tests;

/// <summary>
/// Testira metodu <see cref="AppOperations.RunOperationAsync(AppOptions, MAMM.Signer.Pkcs.Pkcs7Options?, AppResult,
/// AppResult.OpResult, CancellationToken)"/>.
/// </summary>
[TestClass]
public class Tests_AppOperations_RunOperation : CmsTestBase
{
    private static RunSettings m_runSettings = new();

    [ClassInitialize]
    public static void OnClassInitialize( TestContext context )
    {
        Assert.IsNotNull( context );
        m_runSettings = new( context );
    }

    [TestMethod]
    public void A01_NoOptions()
    {
        using var inputFile = new TempFile();
        File.WriteAllBytes( inputFile.Info.FullName, CreateMessage() );

        var appOptions = new AppOptions {};
        var appResult = new AppResult();
        var opResult = new AppResult.OpResult(inputFile.Info);
        appResult.OpResults.Add( opResult );

        AppOperations.RunOperationAsync( appOptions, null, appResult, opResult, default );
    }

    /// <summary>
    /// </summary>
    [TestMethod]
    [DataRow( RunSettings.TESTCERT_RSA )]
    [DataRow( RunSettings.TESTCERT_ECDSA_IDENT )]
    public void A01_Signed_VerifyData_WithTrust(int signerNo)
    {
        using var signer = m_runSettings.GetTestCert(signerNo, imported: true);
        var content = CreateMessage();
    }
}

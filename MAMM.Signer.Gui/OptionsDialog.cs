using MAMM.Signer.Core;
using MAMM.Signer.Pkcs;
using MAMM.Signer.Shared;
using System.Security.Cryptography;

namespace MAMM.Signer.Gui;

internal partial class OptionsDialog : Form
{
    public OptionsDialog( AppOptions appOptions, Pkcs7Options pkcs7Options, ICertificateManager certManager )
    {
        InitializeComponent();
        this.Font = Program.DefaultFont;

        m_certManager = certManager;

        UiInitData();
        UiWriteData( appOptions, pkcs7Options );

        this.AppOptions = appOptions;
        this.Pkcs7Options = pkcs7Options;
    }

    public AppOptions AppOptions { get; private set; }

    public Pkcs7Options Pkcs7Options { get; private set; }

    private readonly ICertificateManager m_certManager;

    private void m_digestAlg_SelectedValueChanged( object sender, EventArgs e )
        => m_digestOid.Oid = (m_digestAlg.SelectedItem as DigestAlgItem)?.Oid;

    private void m_digestOid_Validated( object sender, EventArgs e )
        => (m_digestAlg.SelectedItem as DigestAlgItem)?.Oid = m_digestOid.Oid;

    private void m_encryptCert_SelectCertificate( object sender, SelectCertificateEventArgs e )
        => SelectCertificate(e
            , CertificatePurpose.Identification
            , Strings.OptionsDialog_SelectEncryptCertificateTitle
            , Strings.OptionsDialog_SelectEncryptCertificateMessage
            );

    private void OptionsDialog_FormClosing( object sender, FormClosingEventArgs e )
    {
        if(DialogResult.OK == this.DialogResult)
            (this.AppOptions, this.Pkcs7Options) = UiReadData();
    }

    private void m_outputDirButton_Click( object sender, EventArgs e )
    {
        using var dlgT = new FolderBrowserDialog()
        {
            SelectedPath = m_outputDir.Text,
            ShowNewFolderButton = true,
        };
        if(DialogResult.OK == dlgT.ShowDialog( this ))
            m_outputDir.Text = dlgT.SelectedPath;
    }

    private void m_outputDirResetButton_Click( object sender, EventArgs e )
        => m_outputDir.Text = "";

    private void m_pkcsExt_Validating( object sender, System.ComponentModel.CancelEventArgs e )
    {
        string ext = m_pkcsExt.Text;
        e.Cancel = string.IsNullOrWhiteSpace( ext )
            || '.' != ext[0]
            || 0 <= ext.IndexOfAny( Path.GetInvalidFileNameChars() );
        if(e.Cancel)
            m_pkcsExt.SelectAll();
    }

    private void m_signCert_SelectCertificate( object sender, SelectCertificateEventArgs e )
        => SelectCertificate(e
            , CertificatePurpose.Signature
            , Strings.OptionsDialog_SelectSignCertificateTitle
            , Strings.OptionsDialog_SelectSignCertificateMessage
            );

    private class DigestAlgItem( string displayName, Func<Oid?> getOid, Action<Oid?> setOid )
    {
        public Oid? Oid { get => getOid(); set => setOid( value ); }
        public override string ToString() => displayName;
    }

    private void SelectCertificate( SelectCertificateEventArgs e, CertificatePurpose purpose, string title, string message )
    {
        using(new HourglassCursor( this ))
        {
            m_certManager.LoadCertificates( e.CertificateLocation, m_includeCsp.Checked );
            if(e.Certificate is not null)
                e.Certificate = m_certManager.FindCertificate( e.Certificate.Thumbprint, validOnly: false );
            else
                e.Certificate = m_certManager.SelectCertificate(
                      m_certPurposeAll.Checked ? CertificatePurpose.Unspecified : purpose
                    , !m_allowInvalid.Checked
                    , title
                    , message
                    );
        }
    }

    private void UiInitData()
    {
        m_digestOid.OidGroup = OidGroup.HashAlgorithm;
        m_encryptOid.OidGroup = OidGroup.EncryptionAlgorithm;
    }

    private static Pkcs7Options.DigestAlgorithms UiReadDigestAlgs( ComboBox algsComboBox )
        => (algsComboBox.Tag as Pkcs7Options.DigestAlgorithms)!;

    private (AppOptions, Pkcs7Options) UiReadData()
    {
        var appOptions = new AppOptions();
        var pkcs7Options = new Pkcs7Options();

        appOptions.IgnorePurpose = m_certPurposeAll.Checked;
        appOptions.AllowInvalid = m_allowInvalid.Checked;
        appOptions.IncludeCsp = m_includeCsp.Checked;

        appOptions.Ext = m_pkcsExt.Text;
        appOptions.OutDir = string.IsNullOrEmpty(m_outputDir.Text) ? null : m_outputDir.Text;

        pkcs7Options.DefaultDigestAlgorithms = UiReadDigestAlgs( m_digestAlg );
        pkcs7Options.TrustCertificates = m_trustCertificates.Checked;

        if(m_signCert.CertificateLocation is not null)
            appOptions.SignLoc = m_signCert.CertificateLocation.Value;
        appOptions.SignCert = m_signCert.Thumbprint;

        appOptions.EncryptAlg = m_encryptOid.Oid;
        if(m_encryptCert.CertificateLocation is not null)
            appOptions.EncryptLoc = m_encryptCert.CertificateLocation.Value;
        appOptions.EncryptCert = m_encryptCert.Thumbprint;

        return (appOptions, pkcs7Options);
    }

    private void UiWriteData( AppOptions appOptions, Pkcs7Options pkcs7Options )
    {
        m_certPurposeAll.Checked = appOptions.IgnorePurpose;
        m_certPurposeContext.Checked = !appOptions.IgnorePurpose;
        m_allowInvalid.Checked = appOptions.AllowInvalid;
        m_includeCsp.Checked = appOptions.IncludeCsp;

        m_pkcsExt.Text = appOptions.Ext;
        m_outputDir.Text = appOptions.OutDir;

        UiWriteDigestAlgs( pkcs7Options.DefaultDigestAlgorithms, m_digestAlg );
        m_trustCertificates.Checked = pkcs7Options.TrustCertificates;

        m_encryptOid.Oid = appOptions.EncryptAlg;

        using(new HourglassCursor(this))
        {
            var appResult = new AppResult();
            AppOperations.SelectSignCertificate( appOptions, appResult, m_certManager, forceSilentUi: true, isOptional: true );
            m_signCert.SetCertificate( appOptions.SignLoc, appResult.SignCert );
            AppOperations.SelectEncryptCertificate( appOptions, appResult, m_certManager, forceSilentUi: true, isOptional: true );
            m_encryptCert.SetCertificate( appOptions.EncryptLoc, appResult.EncryptCert );
        }
    }

    private static void UiWriteDigestAlgs( Pkcs7Options.DigestAlgorithms algs, ComboBox algsComboBox )
    {
        static IEnumerable<DigestAlgItem> digestAlgItems( Pkcs7Options.DigestAlgorithms algs )
        {
            yield return new( "RSA CSP", () => algs.RsaCsp, v => algs.RsaCsp = v );
            yield return new( "RSA KSP", () => algs.RsaKsp, v => algs.RsaKsp = v );
            yield return new( "ECDSA P-256", () => algs.Ecdsa256, v => algs.Ecdsa256 = v );
            yield return new( "ECDSA P-384", () => algs.Ecdsa384, v => algs.Ecdsa384 = v );
            yield return new( "ECDSA P-521", () => algs.Ecdsa521, v => algs.Ecdsa521 = v );
        }
        algsComboBox.Items.Clear();
        var tagAlgs = new Pkcs7Options.DigestAlgorithms
        {
            RsaCsp = algs.RsaCsp,
            RsaKsp = algs.RsaKsp,
            Ecdsa256 = algs.Ecdsa256,
            Ecdsa384 = algs.Ecdsa384,
            Ecdsa521 = algs.Ecdsa521
        };
        foreach(var item in digestAlgItems( tagAlgs ))
            algsComboBox.Items.Add( item );
        algsComboBox.Tag = tagAlgs;
    }
}

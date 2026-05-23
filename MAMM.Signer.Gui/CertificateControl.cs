using MAMM.Signer.Shared;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace MAMM.Signer.Gui;

internal partial class CertificateControl : UserControl
{
    public event EventHandler? ShowingDialog;

    public event EventHandler<SelectCertificateEventArgs>? SelectCertificate;

    public CertificateControl()
    {
        InitializeComponent();
        UiUpdateState();
    }

    [Browsable( true )]
    [DesignerSerializationVisibility( DesignerSerializationVisibility.Visible )]
    public override string Text
    {
        get => base.Text;
        set { base.Text = value; m_frame.Text = value; }
    }

    [Browsable( false )]
    public string? Thumbprint { get => m_certificate?.Thumbprint; }

    [Browsable( false )]
    public CertificateLocation? CertificateLocation
    {
        get =>
              m_locationMy.Checked ? Shared.CertificateLocation.CurrentUser
            : m_locationReaders.Checked ? Shared.CertificateLocation.SmartCardReaders
            : null
            ;
    }

    public void SetCertificate( CertificateLocation location, X509Certificate2? certificate = null )
    {
        m_locationMy.Checked = Shared.CertificateLocation.CurrentUser == location;
        m_locationReaders.Checked = Shared.CertificateLocation.SmartCardReaders == location;
        m_certificate = certificate;
        UiWriteData();
        UiUpdateState();
    }

    protected virtual void OnShowingDialog()
        => this.ShowingDialog?.Invoke( this, EventArgs.Empty );

    protected virtual void OnSelectCertificate()
    {
        var e = new SelectCertificateEventArgs( this.CertificateLocation!.Value, certificate: null );
        this.SelectCertificate?.Invoke( this, e );
        if(e.Canceled) return;
        m_certificate = e.Certificate;
        UiWriteData();
        UiUpdateState();
    }

    protected virtual void OnLocationChanged()
    {
        if(m_certificate is null) return;
        var e = new SelectCertificateEventArgs( this.CertificateLocation!.Value, certificate: m_certificate );
        this.SelectCertificate?.Invoke( this, e );
        m_certificate = e.Certificate;
        UiWriteData();
        UiUpdateState();
    }

    private void m_clear_Click( object sender, EventArgs e )
        => SetCertificate( this.CertificateLocation!.Value, certificate: null );

    private void m_locationMy_CheckedChanged( object sender, EventArgs e )
    {
        if(!m_locationMy.Checked) OnLocationChanged();
    }

    private void m_locationReaders_CheckedChanged( object sender, EventArgs e )
    {
        if(!m_locationReaders.Checked ) OnLocationChanged();
    }

    private void m_select_Click( object sender, EventArgs e )
        => OnSelectCertificate();

    private X509Certificate2? m_certificate = null;

    private void UiUpdateState()
    {
        m_locationMy.Enabled = true;
        m_locationReaders.Enabled = true;
        m_select.Enabled = m_locationMy.Checked || m_locationReaders.Checked;
        m_clear.Enabled = m_select.Enabled;
    }

    private void UiWriteData()
    {
        m_certName.Text = m_certificate is not null ? CertHelpers.GetFriendlyOrSubjectName( m_certificate! ) : "";
        m_certIssuer.Text = m_certificate?.Issuer ?? "";
        m_certSerialNo.Text = m_certificate?.SerialNumber ?? "";
    }
}

using System.ComponentModel;
using System.Security.Cryptography;

namespace MAMM.Signer.Gui;

internal partial class OidControl : UserControl
{
    public OidControl()
    {
        InitializeComponent();
    }

    public Oid? Oid
    {
        get
        {
            if("" == m_oid.Text)
                return null;
            return Oid.FromOidValue( m_oid.Text, OidGroup.All );
        }

        set
        {
            m_oid.Text = value?.Value ?? "";
            m_name.Text = value?.FriendlyName ?? "";
        }
    }

    public OidGroup OidGroup { get; set; } = OidGroup.All;

    private void m_name_Validated( object sender, EventArgs e )
    {
        if("" == m_name.Text)
        {
            this.Oid = null;
            m_oid.Text = "";
        }
        else
        {
            this.Oid = Oid.FromFriendlyName( m_name.Text, this.OidGroup );
            m_oid.Text = this.Oid.Value;
        }
    }

    private void m_name_Validating( object sender, CancelEventArgs e )
    {
        if("" == m_name.Text)
            return;
        try { Oid.FromFriendlyName( m_name.Text, this.OidGroup ); }
        catch(CryptographicException)
        {
            m_name.SelectAll();
            e.Cancel = true;
        }
    }

    private void m_oid_Validated( object sender, EventArgs e )
    {
        if("" == m_oid.Text)
        {
            this.Oid = null;
            m_name.Text = "";
        }
        else
        {
            this.Oid = Oid.FromOidValue( m_oid.Text, this.OidGroup );
            m_name.Text = this.Oid.FriendlyName;
        }
    }

    private void m_oid_Validating( object sender, CancelEventArgs e )
    {
        if("" == m_oid.Text)
            return;
        try { Oid.FromOidValue( m_oid.Text, this.OidGroup ); }
        catch(CryptographicException)
        {
            m_oid.SelectAll();
            e.Cancel = true;
        }
    }
}

using MAMM.Signer.Core;
using MAMM.Signer.Pkcs;
using System.Text;

namespace MAMM.Signer.Gui;

internal partial class MainWindow : Form
{
    public MainWindow()
    {
        InitializeComponent();
        this.Font = Program.DefaultFont;
        UiUpdateState();
    }

    // Kraj.
    private void m_exitButton_Click( object sender, EventArgs e )
        => this.Close();

    // Kliknuta je stavka popisa datoteka.
    private void m_fileList_SelectedIndexChanged( object sender, EventArgs e )
        => SelectedFileItemChanged();

    // Izabere datoteke i napuni popis datoteka njihovim stazama.
    private void m_loadButton_Click( object sender, EventArgs e )
    {
        try
        {
            using OpenFileDialog dlg = new();

            dlg.Multiselect = true;
            dlg.Title = Strings.MainWindow_LoadButton_FileOpenDialogTitle;

            if(DialogResult.OK != dlg.ShowDialog())
                return;

            m_files.Items.Clear();
            m_details.Text = "";
            m_appResult = new();

            foreach(string path in dlg.FileNames)
                m_files.Items.Add( path );
        }
        finally
        {
            UiUpdateState();
        }
    }

    // Pokaže dijaloški okvir postavki.
    private void m_settingsButton_Click( object sender, EventArgs e )
    {
        using var frmT = new OptionsDialog(m_appOptions, m_pkcs7Options, Program.CertificateManager);
        if(DialogResult.OK != frmT.ShowDialog( this ))
            return;
        m_appOptions = frmT.AppOptions;
        m_pkcs7Options = frmT.Pkcs7Options;
    }

    // Potpiše i opcionalno šifrira izabrane datoteke.
    private async void m_signButton_Click( object sender, EventArgs e )
    {
        // Želi li korisnik i šifrirati datoteke?
        var encryptAnswer = MessageBox.Show(
              this
            , Strings.MainWindow_SignButton_EncryptPrompt
            , this.Text
            , MessageBoxButtons.YesNoCancel
            , MessageBoxIcon.Question
            , MessageBoxDefaultButton.Button2);
        if(DialogResult.Cancel == encryptAnswer)
            return;
        await RunOperationsAsync( sign: true, encrypt: DialogResult.Yes == encryptAnswer, verify: false );
    }

    // Učita postavke (perzistirane opcije) programa.
    private void MainWindow_Load( object sender, EventArgs e )
    {
        var settings = Settings.Load();
        m_appOptions = settings.AppOptions;
        m_pkcs7Options = settings.Pkcs7Options;
    }

    // Snimi postavke (perzistira opcije) programa.
    private void MainWindow_FormClosing( object sender, FormClosingEventArgs e )
    {
        var settings = new Settings()
        {
            AppOptions = m_appOptions,
            Pkcs7Options = m_pkcs7Options,
        };
        settings.Save();
    }

    // Dešifrira i ovjeri potpis u izabranim datotekama.
    private async void m_verifyButton_Click( object sender, EventArgs e )
        => await RunOperationsAsync( sign: false, encrypt: false, verify: true );

    /// <summary>
    /// Izvršne opcije programa.
    /// </summary>
    private AppOptions m_appOptions = new();

    /// <summary>
    /// Ishod zadnje operacije programa.
    /// </summary>
    private AppResult m_appResult = new();

    /// <summary>
    /// Objekt za formatiranje teksta s ishodom operacija.
    /// </summary>
    private readonly AppResultFormatter m_appResultFormatter = new(Program.CertificateManager);

    /// <summary>
    /// Konfiguracijske opcije programa.
    /// </summary>
    private Pkcs7Options m_pkcs7Options = new();

    /// <summary>
    /// Izvršava operacije.
    /// </summary>
    private async Task RunOperationsAsync( bool sign, bool encrypt, bool verify )
    {
        try
        {
            // Aktualizira izvršne opcije.
            m_appOptions.Sign = sign;
            m_appOptions.Encrypt = encrypt;
            m_appOptions.Verify = verify;

            // Reset zadnjeg ishoda operacije.
            m_appResult = new();

            // Obradi izabrane datoteke.
            using(new HourglassCursor( this ))
                m_appResult = await AppOperations.RunOperationsAsync(
                      m_files.Items.Cast<string>().Select( s => new FileInfo( s ) )
                    , m_appOptions
                    , m_pkcs7Options
                    , Program.CertificateManager
                    );
        }
        finally
        {
            UiUpdateState();
        }

        // Izvjesti ako je došlo do pogreške.
        if(m_appResultFormatter.FormatError( m_appResult, out var message ))
            MessageBox.Show( this, message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error );
    }

    /// <summary>
    /// Ažurira prikaz detalja stavke u popisu datoteka.
    /// </summary>
    private void SelectedFileItemChanged()
    {
        if(m_files.SelectedIndex < 0)
            return;
        var buffer = new StringBuilder();
        if(m_appResultFormatter.FormatPreamble( buffer, m_appResult ))
            buffer.AppendLine();
        if(m_appResultFormatter.Format( buffer, m_appResult, m_files.SelectedIndex ))
            buffer.AppendLine();
        m_details.Text = buffer.ToString();
    }

    /// <summary>
    /// Ažurira stanje sučelja.
    /// </summary>
    private void UiUpdateState()
    {
        m_signButton.Enabled = 0 < m_files.Items.Count;
        m_verifyButton.Enabled = m_signButton.Enabled;

        if(-1 == m_files.SelectedIndex && 0 < m_files.Items.Count)
            m_files.SelectedIndex = 0;
        else
            SelectedFileItemChanged();
    }
}

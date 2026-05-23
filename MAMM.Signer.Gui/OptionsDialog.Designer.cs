namespace MAMM.Signer.Gui
{
    partial class OptionsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if(disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.GroupBox m_group_01;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsDialog));
            System.Windows.Forms.Label m_label_08;
            System.Windows.Forms.Label m_label_07;
            System.Windows.Forms.Label m_label_02;
            System.Windows.Forms.Label m_label_01;
            System.Windows.Forms.TabControl m_tab_01;
            System.Windows.Forms.TabPage m_pageSignature;
            System.Windows.Forms.Label m_label_06;
            System.Windows.Forms.TabPage m_pageEncryption;
            System.Windows.Forms.Label m_label_03;
            this.m_includeCsp = new System.Windows.Forms.CheckBox();
            this.m_allowInvalid = new System.Windows.Forms.CheckBox();
            this.m_certPurposeContext = new System.Windows.Forms.RadioButton();
            this.m_certPurposeAll = new System.Windows.Forms.RadioButton();
            this.m_digestOid = new MAMM.Signer.Gui.OidControl();
            this.m_trustCertificates = new System.Windows.Forms.CheckBox();
            this.m_signCert = new MAMM.Signer.Gui.CertificateControl();
            this.m_digestAlg = new System.Windows.Forms.ComboBox();
            this.m_encryptOid = new MAMM.Signer.Gui.OidControl();
            this.m_encryptCert = new MAMM.Signer.Gui.CertificateControl();
            this.m_okButton = new System.Windows.Forms.Button();
            this.m_cancelButton = new System.Windows.Forms.Button();
            this.m_outputDirButton = new System.Windows.Forms.Button();
            this.m_outputDir = new System.Windows.Forms.TextBox();
            this.m_pkcsExt = new System.Windows.Forms.TextBox();
            this.m_outputDirResetButton = new System.Windows.Forms.Button();
            m_group_01 = new System.Windows.Forms.GroupBox();
            m_label_08 = new System.Windows.Forms.Label();
            m_label_07 = new System.Windows.Forms.Label();
            m_label_02 = new System.Windows.Forms.Label();
            m_label_01 = new System.Windows.Forms.Label();
            m_tab_01 = new System.Windows.Forms.TabControl();
            m_pageSignature = new System.Windows.Forms.TabPage();
            m_label_06 = new System.Windows.Forms.Label();
            m_pageEncryption = new System.Windows.Forms.TabPage();
            m_label_03 = new System.Windows.Forms.Label();
            m_group_01.SuspendLayout();
            m_tab_01.SuspendLayout();
            m_pageSignature.SuspendLayout();
            m_pageEncryption.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_group_01
            // 
            resources.ApplyResources(m_group_01, "m_group_01");
            m_group_01.Controls.Add(m_label_08);
            m_group_01.Controls.Add(m_label_07);
            m_group_01.Controls.Add(this.m_includeCsp);
            m_group_01.Controls.Add(this.m_allowInvalid);
            m_group_01.Controls.Add(this.m_certPurposeContext);
            m_group_01.Controls.Add(this.m_certPurposeAll);
            m_group_01.Name = "m_group_01";
            m_group_01.TabStop = false;
            // 
            // m_label_08
            // 
            resources.ApplyResources(m_label_08, "m_label_08");
            m_label_08.Name = "m_label_08";
            // 
            // m_label_07
            // 
            resources.ApplyResources(m_label_07, "m_label_07");
            m_label_07.Name = "m_label_07";
            // 
            // m_includeCsp
            // 
            resources.ApplyResources(this.m_includeCsp, "m_includeCsp");
            this.m_includeCsp.Name = "m_includeCsp";
            this.m_includeCsp.UseVisualStyleBackColor = true;
            // 
            // m_allowInvalid
            // 
            resources.ApplyResources(this.m_allowInvalid, "m_allowInvalid");
            this.m_allowInvalid.Name = "m_allowInvalid";
            this.m_allowInvalid.UseVisualStyleBackColor = true;
            // 
            // m_certPurposeContext
            // 
            resources.ApplyResources(this.m_certPurposeContext, "m_certPurposeContext");
            this.m_certPurposeContext.Name = "m_certPurposeContext";
            this.m_certPurposeContext.UseVisualStyleBackColor = true;
            // 
            // m_certPurposeAll
            // 
            resources.ApplyResources(this.m_certPurposeAll, "m_certPurposeAll");
            this.m_certPurposeAll.Checked = true;
            this.m_certPurposeAll.Name = "m_certPurposeAll";
            this.m_certPurposeAll.TabStop = true;
            this.m_certPurposeAll.UseVisualStyleBackColor = true;
            // 
            // m_label_02
            // 
            resources.ApplyResources(m_label_02, "m_label_02");
            m_label_02.Name = "m_label_02";
            // 
            // m_label_01
            // 
            resources.ApplyResources(m_label_01, "m_label_01");
            m_label_01.Name = "m_label_01";
            // 
            // m_tab_01
            // 
            resources.ApplyResources(m_tab_01, "m_tab_01");
            m_tab_01.Controls.Add(m_pageSignature);
            m_tab_01.Controls.Add(m_pageEncryption);
            m_tab_01.Name = "m_tab_01";
            m_tab_01.SelectedIndex = 0;
            // 
            // m_pageSignature
            // 
            resources.ApplyResources(m_pageSignature, "m_pageSignature");
            m_pageSignature.Controls.Add(this.m_digestOid);
            m_pageSignature.Controls.Add(this.m_trustCertificates);
            m_pageSignature.Controls.Add(this.m_signCert);
            m_pageSignature.Controls.Add(m_label_06);
            m_pageSignature.Controls.Add(this.m_digestAlg);
            m_pageSignature.Name = "m_pageSignature";
            m_pageSignature.UseVisualStyleBackColor = true;
            // 
            // m_digestOid
            // 
            resources.ApplyResources(this.m_digestOid, "m_digestOid");
            this.m_digestOid.Name = "m_digestOid";
            this.m_digestOid.Oid = null;
            this.m_digestOid.OidGroup = System.Security.Cryptography.OidGroup.All;
            this.m_digestOid.Validated += new System.EventHandler(this.m_digestOid_Validated);
            // 
            // m_trustCertificates
            // 
            resources.ApplyResources(this.m_trustCertificates, "m_trustCertificates");
            this.m_trustCertificates.Name = "m_trustCertificates";
            this.m_trustCertificates.UseVisualStyleBackColor = true;
            // 
            // m_signCert
            // 
            resources.ApplyResources(this.m_signCert, "m_signCert");
            this.m_signCert.Name = "m_signCert";
            this.m_signCert.SelectCertificate += new System.EventHandler<MAMM.Signer.Gui.SelectCertificateEventArgs>(this.m_signCert_SelectCertificate);
            // 
            // m_label_06
            // 
            resources.ApplyResources(m_label_06, "m_label_06");
            m_label_06.Name = "m_label_06";
            // 
            // m_digestAlg
            // 
            resources.ApplyResources(this.m_digestAlg, "m_digestAlg");
            this.m_digestAlg.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_digestAlg.FormattingEnabled = true;
            this.m_digestAlg.Name = "m_digestAlg";
            this.m_digestAlg.SelectedValueChanged += new System.EventHandler(this.m_digestAlg_SelectedValueChanged);
            // 
            // m_pageEncryption
            // 
            resources.ApplyResources(m_pageEncryption, "m_pageEncryption");
            m_pageEncryption.Controls.Add(this.m_encryptOid);
            m_pageEncryption.Controls.Add(this.m_encryptCert);
            m_pageEncryption.Controls.Add(m_label_03);
            m_pageEncryption.Name = "m_pageEncryption";
            m_pageEncryption.UseVisualStyleBackColor = true;
            // 
            // m_encryptOid
            // 
            resources.ApplyResources(this.m_encryptOid, "m_encryptOid");
            this.m_encryptOid.Name = "m_encryptOid";
            this.m_encryptOid.Oid = null;
            this.m_encryptOid.OidGroup = System.Security.Cryptography.OidGroup.All;
            // 
            // m_encryptCert
            // 
            resources.ApplyResources(this.m_encryptCert, "m_encryptCert");
            this.m_encryptCert.Name = "m_encryptCert";
            this.m_encryptCert.SelectCertificate += new System.EventHandler<MAMM.Signer.Gui.SelectCertificateEventArgs>(this.m_encryptCert_SelectCertificate);
            // 
            // m_label_03
            // 
            resources.ApplyResources(m_label_03, "m_label_03");
            m_label_03.Name = "m_label_03";
            // 
            // m_okButton
            // 
            resources.ApplyResources(this.m_okButton, "m_okButton");
            this.m_okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_okButton.Name = "m_okButton";
            this.m_okButton.UseVisualStyleBackColor = true;
            // 
            // m_cancelButton
            // 
            resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
            this.m_cancelButton.CausesValidation = false;
            this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelButton.Name = "m_cancelButton";
            this.m_cancelButton.UseVisualStyleBackColor = true;
            // 
            // m_outputDirButton
            // 
            resources.ApplyResources(this.m_outputDirButton, "m_outputDirButton");
            this.m_outputDirButton.Name = "m_outputDirButton";
            this.m_outputDirButton.UseVisualStyleBackColor = true;
            this.m_outputDirButton.Click += new System.EventHandler(this.m_outputDirButton_Click);
            // 
            // m_outputDir
            // 
            resources.ApplyResources(this.m_outputDir, "m_outputDir");
            this.m_outputDir.Name = "m_outputDir";
            this.m_outputDir.ReadOnly = true;
            // 
            // m_pkcsExt
            // 
            resources.ApplyResources(this.m_pkcsExt, "m_pkcsExt");
            this.m_pkcsExt.Name = "m_pkcsExt";
            this.m_pkcsExt.Validating += new System.ComponentModel.CancelEventHandler(this.m_pkcsExt_Validating);
            // 
            // m_outputDirResetButton
            // 
            resources.ApplyResources(this.m_outputDirResetButton, "m_outputDirResetButton");
            this.m_outputDirResetButton.Name = "m_outputDirResetButton";
            this.m_outputDirResetButton.UseVisualStyleBackColor = true;
            this.m_outputDirResetButton.Click += new System.EventHandler(this.m_outputDirResetButton_Click);
            // 
            // OptionsDialog
            // 
            this.AcceptButton = this.m_okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_cancelButton;
            this.Controls.Add(this.m_outputDirResetButton);
            this.Controls.Add(this.m_outputDirButton);
            this.Controls.Add(this.m_outputDir);
            this.Controls.Add(this.m_pkcsExt);
            this.Controls.Add(m_label_02);
            this.Controls.Add(m_label_01);
            this.Controls.Add(m_group_01);
            this.Controls.Add(this.m_cancelButton);
            this.Controls.Add(this.m_okButton);
            this.Controls.Add(m_tab_01);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OptionsDialog_FormClosing);
            m_group_01.ResumeLayout(false);
            m_tab_01.ResumeLayout(false);
            m_pageSignature.ResumeLayout(false);
            m_pageEncryption.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private RadioButton m_certPurposeAll;
        private RadioButton m_certPurposeContext;
        private Button m_okButton;
        private Button m_cancelButton;
        private CheckBox m_allowInvalid;
        private CheckBox m_includeCsp;
        private Button m_outputDirButton;
        private TextBox m_outputDir;
        private TextBox m_pkcsExt;
        private CertificateControl m_signCert;
        private ComboBox m_digestAlg;
        private CertificateControl m_encryptCert;
        private CheckBox m_trustCertificates;
        private OidControl m_digestOid;
        private OidControl m_encryptOid;
        private Button m_outputDirResetButton;
    }
}
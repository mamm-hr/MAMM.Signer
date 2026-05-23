namespace MAMM.Signer.Gui
{
    partial class CertificateControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label m_label_03;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CertificateControl));
            System.Windows.Forms.Label m_label_09;
            System.Windows.Forms.Label m_label_08;
            System.Windows.Forms.Label m_label_06;
            this.m_frame = new System.Windows.Forms.GroupBox();
            this.m_clear = new System.Windows.Forms.Button();
            this.m_select = new System.Windows.Forms.Button();
            this.m_certSerialNo = new System.Windows.Forms.TextBox();
            this.m_certName = new System.Windows.Forms.TextBox();
            this.m_locationReaders = new System.Windows.Forms.RadioButton();
            this.m_locationMy = new System.Windows.Forms.RadioButton();
            this.m_certIssuer = new System.Windows.Forms.TextBox();
            m_label_03 = new System.Windows.Forms.Label();
            m_label_09 = new System.Windows.Forms.Label();
            m_label_08 = new System.Windows.Forms.Label();
            m_label_06 = new System.Windows.Forms.Label();
            this.m_frame.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_label_03
            // 
            resources.ApplyResources(m_label_03, "m_label_03");
            m_label_03.Name = "m_label_03";
            // 
            // m_label_09
            // 
            resources.ApplyResources(m_label_09, "m_label_09");
            m_label_09.Name = "m_label_09";
            // 
            // m_label_08
            // 
            resources.ApplyResources(m_label_08, "m_label_08");
            m_label_08.Name = "m_label_08";
            // 
            // m_label_06
            // 
            resources.ApplyResources(m_label_06, "m_label_06");
            m_label_06.Name = "m_label_06";
            // 
            // m_frame
            // 
            resources.ApplyResources(this.m_frame, "m_frame");
            this.m_frame.Controls.Add(this.m_clear);
            this.m_frame.Controls.Add(this.m_select);
            this.m_frame.Controls.Add(m_label_09);
            this.m_frame.Controls.Add(this.m_certSerialNo);
            this.m_frame.Controls.Add(m_label_08);
            this.m_frame.Controls.Add(m_label_06);
            this.m_frame.Controls.Add(this.m_certName);
            this.m_frame.Controls.Add(this.m_locationReaders);
            this.m_frame.Controls.Add(this.m_locationMy);
            this.m_frame.Controls.Add(m_label_03);
            this.m_frame.Controls.Add(this.m_certIssuer);
            this.m_frame.Name = "m_frame";
            this.m_frame.TabStop = false;
            // 
            // m_clear
            // 
            resources.ApplyResources(this.m_clear, "m_clear");
            this.m_clear.Name = "m_clear";
            this.m_clear.UseVisualStyleBackColor = true;
            this.m_clear.Click += new System.EventHandler(this.m_clear_Click);
            // 
            // m_select
            // 
            resources.ApplyResources(this.m_select, "m_select");
            this.m_select.Name = "m_select";
            this.m_select.UseVisualStyleBackColor = true;
            this.m_select.Click += new System.EventHandler(this.m_select_Click);
            // 
            // m_certSerialNo
            // 
            resources.ApplyResources(this.m_certSerialNo, "m_certSerialNo");
            this.m_certSerialNo.Name = "m_certSerialNo";
            this.m_certSerialNo.ReadOnly = true;
            // 
            // m_certName
            // 
            resources.ApplyResources(this.m_certName, "m_certName");
            this.m_certName.Name = "m_certName";
            this.m_certName.ReadOnly = true;
            // 
            // m_locationReaders
            // 
            resources.ApplyResources(this.m_locationReaders, "m_locationReaders");
            this.m_locationReaders.Name = "m_locationReaders";
            this.m_locationReaders.UseVisualStyleBackColor = true;
            this.m_locationReaders.CheckedChanged += new System.EventHandler(this.m_locationReaders_CheckedChanged);
            // 
            // m_locationMy
            // 
            resources.ApplyResources(this.m_locationMy, "m_locationMy");
            this.m_locationMy.Name = "m_locationMy";
            this.m_locationMy.TabStop = true;
            this.m_locationMy.UseVisualStyleBackColor = true;
            this.m_locationMy.CheckedChanged += new System.EventHandler(this.m_locationMy_CheckedChanged);
            // 
            // m_certIssuer
            // 
            resources.ApplyResources(this.m_certIssuer, "m_certIssuer");
            this.m_certIssuer.Name = "m_certIssuer";
            this.m_certIssuer.ReadOnly = true;
            // 
            // CertificateControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_frame);
            this.Name = "CertificateControl";
            this.m_frame.ResumeLayout(false);
            this.m_frame.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox m_frame;
        private RadioButton m_locationReaders;
        private RadioButton m_locationMy;
        private Button m_select;
        private TextBox m_certSerialNo;
        private TextBox m_certIssuer;
        private TextBox m_certName;
        private Button m_clear;
    }
}

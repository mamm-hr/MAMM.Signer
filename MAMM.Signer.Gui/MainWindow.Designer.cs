namespace MAMM.Signer.Gui
{
    partial class MainWindow
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
            System.Windows.Forms.Label m_label_02;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            System.Windows.Forms.Panel m_panel_03;
            System.Windows.Forms.Panel m_panel_04;
            System.Windows.Forms.Panel m_panel_02;
            System.Windows.Forms.Splitter m_splitter_01;
            System.Windows.Forms.Panel m_panel_01;
            System.Windows.Forms.Label m_label_01;
            this.m_exitButton = new System.Windows.Forms.Button();
            this.m_settingsButton = new System.Windows.Forms.Button();
            this.m_verifyButton = new System.Windows.Forms.Button();
            this.m_signButton = new System.Windows.Forms.Button();
            this.m_loadButton = new System.Windows.Forms.Button();
            this.m_details = new System.Windows.Forms.TextBox();
            this.m_files = new System.Windows.Forms.ListBox();
            m_label_02 = new System.Windows.Forms.Label();
            m_panel_03 = new System.Windows.Forms.Panel();
            m_panel_04 = new System.Windows.Forms.Panel();
            m_panel_02 = new System.Windows.Forms.Panel();
            m_splitter_01 = new System.Windows.Forms.Splitter();
            m_panel_01 = new System.Windows.Forms.Panel();
            m_label_01 = new System.Windows.Forms.Label();
            m_panel_03.SuspendLayout();
            m_panel_04.SuspendLayout();
            m_panel_02.SuspendLayout();
            m_panel_01.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_label_02
            // 
            resources.ApplyResources(m_label_02, "m_label_02");
            m_label_02.Name = "m_label_02";
            // 
            // m_panel_03
            // 
            resources.ApplyResources(m_panel_03, "m_panel_03");
            m_panel_03.Controls.Add(m_panel_04);
            m_panel_03.Name = "m_panel_03";
            // 
            // m_panel_04
            // 
            resources.ApplyResources(m_panel_04, "m_panel_04");
            m_panel_04.Controls.Add(this.m_exitButton);
            m_panel_04.Controls.Add(this.m_settingsButton);
            m_panel_04.Controls.Add(this.m_verifyButton);
            m_panel_04.Controls.Add(this.m_signButton);
            m_panel_04.Controls.Add(this.m_loadButton);
            m_panel_04.Name = "m_panel_04";
            // 
            // m_exitButton
            // 
            resources.ApplyResources(this.m_exitButton, "m_exitButton");
            this.m_exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_exitButton.Name = "m_exitButton";
            this.m_exitButton.UseVisualStyleBackColor = true;
            this.m_exitButton.Click += new System.EventHandler(this.m_exitButton_Click);
            // 
            // m_settingsButton
            // 
            resources.ApplyResources(this.m_settingsButton, "m_settingsButton");
            this.m_settingsButton.Name = "m_settingsButton";
            this.m_settingsButton.UseVisualStyleBackColor = true;
            this.m_settingsButton.Click += new System.EventHandler(this.m_settingsButton_Click);
            // 
            // m_verifyButton
            // 
            resources.ApplyResources(this.m_verifyButton, "m_verifyButton");
            this.m_verifyButton.Name = "m_verifyButton";
            this.m_verifyButton.UseVisualStyleBackColor = true;
            this.m_verifyButton.Click += new System.EventHandler(this.m_verifyButton_Click);
            // 
            // m_signButton
            // 
            resources.ApplyResources(this.m_signButton, "m_signButton");
            this.m_signButton.Name = "m_signButton";
            this.m_signButton.UseVisualStyleBackColor = true;
            this.m_signButton.Click += new System.EventHandler(this.m_signButton_Click);
            // 
            // m_loadButton
            // 
            resources.ApplyResources(this.m_loadButton, "m_loadButton");
            this.m_loadButton.Name = "m_loadButton";
            this.m_loadButton.UseVisualStyleBackColor = true;
            this.m_loadButton.Click += new System.EventHandler(this.m_loadButton_Click);
            // 
            // m_panel_02
            // 
            resources.ApplyResources(m_panel_02, "m_panel_02");
            m_panel_02.Controls.Add(this.m_details);
            m_panel_02.Controls.Add(m_label_02);
            m_panel_02.Name = "m_panel_02";
            // 
            // m_details
            // 
            resources.ApplyResources(this.m_details, "m_details");
            this.m_details.BackColor = System.Drawing.SystemColors.Window;
            this.m_details.Name = "m_details";
            this.m_details.ReadOnly = true;
            // 
            // m_splitter_01
            // 
            resources.ApplyResources(m_splitter_01, "m_splitter_01");
            m_splitter_01.Name = "m_splitter_01";
            m_splitter_01.TabStop = false;
            // 
            // m_panel_01
            // 
            resources.ApplyResources(m_panel_01, "m_panel_01");
            m_panel_01.Controls.Add(this.m_files);
            m_panel_01.Controls.Add(m_label_01);
            m_panel_01.Name = "m_panel_01";
            // 
            // m_files
            // 
            resources.ApplyResources(this.m_files, "m_files");
            this.m_files.FormattingEnabled = true;
            this.m_files.Name = "m_files";
            this.m_files.SelectedIndexChanged += new System.EventHandler(this.m_fileList_SelectedIndexChanged);
            // 
            // m_label_01
            // 
            resources.ApplyResources(m_label_01, "m_label_01");
            m_label_01.Name = "m_label_01";
            // 
            // MainWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(m_panel_01);
            this.Controls.Add(m_splitter_01);
            this.Controls.Add(m_panel_02);
            this.Controls.Add(m_panel_03);
            this.Name = "MainWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            m_panel_03.ResumeLayout(false);
            m_panel_04.ResumeLayout(false);
            m_panel_02.ResumeLayout(false);
            m_panel_02.PerformLayout();
            m_panel_01.ResumeLayout(false);
            m_panel_01.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox m_details;
        private System.Windows.Forms.ListBox m_files;
        private System.Windows.Forms.Button m_exitButton;
        private System.Windows.Forms.Button m_settingsButton;
        private System.Windows.Forms.Button m_verifyButton;
        private System.Windows.Forms.Button m_signButton;
        private System.Windows.Forms.Button m_loadButton;
    }
}


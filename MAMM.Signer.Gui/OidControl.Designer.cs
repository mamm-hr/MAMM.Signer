namespace MAMM.Signer.Gui
{
    partial class OidControl
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
            System.Windows.Forms.Label m_label_01;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OidControl));
            System.Windows.Forms.Label m_label_02;
            this.m_name = new System.Windows.Forms.TextBox();
            this.m_oid = new System.Windows.Forms.TextBox();
            m_label_01 = new System.Windows.Forms.Label();
            m_label_02 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_label_01
            // 
            resources.ApplyResources(m_label_01, "m_label_01");
            m_label_01.Name = "m_label_01";
            // 
            // m_label_02
            // 
            resources.ApplyResources(m_label_02, "m_label_02");
            m_label_02.Name = "m_label_02";
            // 
            // m_name
            // 
            resources.ApplyResources(this.m_name, "m_name");
            this.m_name.Name = "m_name";
            this.m_name.Validating += new System.ComponentModel.CancelEventHandler(this.m_name_Validating);
            this.m_name.Validated += new System.EventHandler(this.m_name_Validated);
            // 
            // m_oid
            // 
            resources.ApplyResources(this.m_oid, "m_oid");
            this.m_oid.Name = "m_oid";
            this.m_oid.Validating += new System.ComponentModel.CancelEventHandler(this.m_oid_Validating);
            this.m_oid.Validated += new System.EventHandler(this.m_oid_Validated);
            // 
            // OidControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(m_label_01);
            this.Controls.Add(this.m_name);
            this.Controls.Add(m_label_02);
            this.Controls.Add(this.m_oid);
            this.Name = "OidControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox m_name;
        private TextBox m_oid;
    }
}

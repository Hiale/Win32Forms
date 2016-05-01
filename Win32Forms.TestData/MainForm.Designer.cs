namespace Hiale.Win32Forms.TestData
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnShowForm = new System.Windows.Forms.Button();
            this.cmbForms = new System.Windows.Forms.ComboBox();
            this.grpForms = new System.Windows.Forms.GroupBox();
            this.grpForms.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnShowForm
            // 
            this.btnShowForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowForm.Location = new System.Drawing.Point(179, 46);
            this.btnShowForm.Name = "btnShowForm";
            this.btnShowForm.Size = new System.Drawing.Size(75, 23);
            this.btnShowForm.TabIndex = 0;
            this.btnShowForm.Text = "Show";
            this.btnShowForm.UseVisualStyleBackColor = true;
            this.btnShowForm.Click += new System.EventHandler(this.btnShowForm_Click);
            // 
            // cmbForms
            // 
            this.cmbForms.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbForms.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbForms.FormattingEnabled = true;
            this.cmbForms.Location = new System.Drawing.Point(6, 19);
            this.cmbForms.Name = "cmbForms";
            this.cmbForms.Size = new System.Drawing.Size(248, 21);
            this.cmbForms.TabIndex = 1;
            // 
            // grpForms
            // 
            this.grpForms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpForms.Controls.Add(this.cmbForms);
            this.grpForms.Controls.Add(this.btnShowForm);
            this.grpForms.Location = new System.Drawing.Point(12, 12);
            this.grpForms.Name = "grpForms";
            this.grpForms.Size = new System.Drawing.Size(260, 82);
            this.grpForms.TabIndex = 2;
            this.grpForms.TabStop = false;
            this.grpForms.Text = "Forms";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 107);
            this.Controls.Add(this.grpForms);
            this.Name = "MainForm";
            this.Text = "Win32Forms TestData";
            this.grpForms.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnShowForm;
        private System.Windows.Forms.ComboBox cmbForms;
        private System.Windows.Forms.GroupBox grpForms;
    }
}


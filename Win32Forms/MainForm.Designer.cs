namespace Hiale.Win32Forms
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
            this.grpInput = new System.Windows.Forms.GroupBox();
            this.lstForms = new System.Windows.Forms.ListBox();
            this.btnBrowseAssembly = new System.Windows.Forms.Button();
            this.txtAssembly = new System.Windows.Forms.TextBox();
            this.grpOutput = new System.Windows.Forms.GroupBox();
            this.chkReplace = new System.Windows.Forms.CheckBox();
            this.chkUseControlName = new System.Windows.Forms.CheckBox();
            this.btnBrowseResource = new System.Windows.Forms.Button();
            this.txtResource = new System.Windows.Forms.TextBox();
            this.btnConvert = new System.Windows.Forms.Button();
            this.chkResize = new System.Windows.Forms.CheckBox();
            this.grpInput.SuspendLayout();
            this.grpOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpInput
            // 
            this.grpInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpInput.Controls.Add(this.lstForms);
            this.grpInput.Controls.Add(this.btnBrowseAssembly);
            this.grpInput.Controls.Add(this.txtAssembly);
            this.grpInput.Location = new System.Drawing.Point(12, 12);
            this.grpInput.Name = "grpInput";
            this.grpInput.Size = new System.Drawing.Size(260, 170);
            this.grpInput.TabIndex = 0;
            this.grpInput.TabStop = false;
            this.grpInput.Text = "Input - Windows Form";
            // 
            // lstForms
            // 
            this.lstForms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstForms.FormattingEnabled = true;
            this.lstForms.Location = new System.Drawing.Point(6, 45);
            this.lstForms.Name = "lstForms";
            this.lstForms.Size = new System.Drawing.Size(248, 108);
            this.lstForms.TabIndex = 3;
            this.lstForms.SelectedIndexChanged += new System.EventHandler(this.lstForms_SelectedIndexChanged);
            // 
            // btnBrowseAssembly
            // 
            this.btnBrowseAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseAssembly.Location = new System.Drawing.Point(213, 16);
            this.btnBrowseAssembly.Name = "btnBrowseAssembly";
            this.btnBrowseAssembly.Size = new System.Drawing.Size(41, 23);
            this.btnBrowseAssembly.TabIndex = 2;
            this.btnBrowseAssembly.Text = "...";
            this.btnBrowseAssembly.UseVisualStyleBackColor = true;
            this.btnBrowseAssembly.Click += new System.EventHandler(this.btnBrowseAssembly_Click);
            // 
            // txtAssembly
            // 
            this.txtAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAssembly.Location = new System.Drawing.Point(6, 19);
            this.txtAssembly.Name = "txtAssembly";
            this.txtAssembly.Size = new System.Drawing.Size(201, 20);
            this.txtAssembly.TabIndex = 1;
            this.txtAssembly.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtAssembly_KeyUp);
            // 
            // grpOutput
            // 
            this.grpOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpOutput.Controls.Add(this.chkResize);
            this.grpOutput.Controls.Add(this.chkReplace);
            this.grpOutput.Controls.Add(this.chkUseControlName);
            this.grpOutput.Controls.Add(this.btnBrowseResource);
            this.grpOutput.Controls.Add(this.txtResource);
            this.grpOutput.Location = new System.Drawing.Point(12, 188);
            this.grpOutput.Name = "grpOutput";
            this.grpOutput.Size = new System.Drawing.Size(260, 120);
            this.grpOutput.TabIndex = 4;
            this.grpOutput.TabStop = false;
            this.grpOutput.Text = "Output - Win32 Dialog";
            // 
            // chkReplace
            // 
            this.chkReplace.AutoSize = true;
            this.chkReplace.Checked = true;
            this.chkReplace.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkReplace.Location = new System.Drawing.Point(6, 69);
            this.chkReplace.Name = "chkReplace";
            this.chkReplace.Size = new System.Drawing.Size(144, 17);
            this.chkReplace.TabIndex = 8;
            this.chkReplace.Text = "Replace Dialog if it exists";
            this.chkReplace.UseVisualStyleBackColor = true;
            // 
            // chkUseControlName
            // 
            this.chkUseControlName.AutoSize = true;
            this.chkUseControlName.Checked = true;
            this.chkUseControlName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseControlName.Location = new System.Drawing.Point(6, 45);
            this.chkUseControlName.Name = "chkUseControlName";
            this.chkUseControlName.Size = new System.Drawing.Size(140, 17);
            this.chkUseControlName.TabIndex = 7;
            this.chkUseControlName.Text = "Use Control.Name as ID";
            this.chkUseControlName.UseVisualStyleBackColor = true;
            // 
            // btnBrowseResource
            // 
            this.btnBrowseResource.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseResource.Location = new System.Drawing.Point(213, 16);
            this.btnBrowseResource.Name = "btnBrowseResource";
            this.btnBrowseResource.Size = new System.Drawing.Size(41, 23);
            this.btnBrowseResource.TabIndex = 6;
            this.btnBrowseResource.Text = "...";
            this.btnBrowseResource.UseVisualStyleBackColor = true;
            this.btnBrowseResource.Click += new System.EventHandler(this.btnBrowseResource_Click);
            // 
            // txtResource
            // 
            this.txtResource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResource.Location = new System.Drawing.Point(6, 19);
            this.txtResource.Name = "txtResource";
            this.txtResource.Size = new System.Drawing.Size(201, 20);
            this.txtResource.TabIndex = 5;
            this.txtResource.TextChanged += new System.EventHandler(this.txtResource_TextChanged);
            // 
            // btnConvert
            // 
            this.btnConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConvert.Enabled = false;
            this.btnConvert.Location = new System.Drawing.Point(197, 315);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(75, 23);
            this.btnConvert.TabIndex = 10;
            this.btnConvert.Text = "Convert";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // chkResize
            // 
            this.chkResize.AutoSize = true;
            this.chkResize.Checked = true;
            this.chkResize.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkResize.Location = new System.Drawing.Point(6, 92);
            this.chkResize.Name = "chkResize";
            this.chkResize.Size = new System.Drawing.Size(133, 17);
            this.chkResize.TabIndex = 9;
            this.chkResize.Text = "Generate Resize Code";
            this.chkResize.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 345);
            this.Controls.Add(this.btnConvert);
            this.Controls.Add(this.grpOutput);
            this.Controls.Add(this.grpInput);
            this.Name = "MainForm";
            this.Text = "Win32Forms";
            this.grpInput.ResumeLayout(false);
            this.grpInput.PerformLayout();
            this.grpOutput.ResumeLayout(false);
            this.grpOutput.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpInput;
        private System.Windows.Forms.ListBox lstForms;
        private System.Windows.Forms.Button btnBrowseAssembly;
        private System.Windows.Forms.TextBox txtAssembly;
        private System.Windows.Forms.GroupBox grpOutput;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.Button btnBrowseResource;
        private System.Windows.Forms.TextBox txtResource;
        private System.Windows.Forms.CheckBox chkUseControlName;
        private System.Windows.Forms.CheckBox chkReplace;
        private System.Windows.Forms.CheckBox chkResize;
    }
}


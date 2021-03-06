﻿using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hiale.Win32Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void lstForms_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableConvertButton();
        }

        private void txtResource_TextChanged(object sender, EventArgs e)
        {
            EnableConvertButton();
        }

        private void btnBrowseAssembly_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = @"Assemblies (*.exe, *.dll)|*.exe;*.dll|All Files (*.*)|*.*";
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;
                txtAssembly.Text = dlg.FileName;
                LoadAssembly(dlg.FileName);
            }
        }

        private void btnBrowseResource_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.OverwritePrompt = false;
                dlg.Filter = @"Resource Files (*.rc)|*.rc|All Files (*.*)|*.*";
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;
                txtResource.Text = dlg.FileName;
            }
        }

        private void txtAssembly_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadAssembly(txtAssembly.Text);
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            var formResult = lstForms.SelectedItem as FormResult;
            if (formResult?.Type == null)
                return;
            Convert(formResult.Type);
        }

        private void EnableConvertButton()
        {
            bool resourceFileValid;
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new FileInfo(txtResource.Text);
                resourceFileValid = true;
            }
            catch (Exception)
            {
                resourceFileValid = false;
            }
            btnConvert.Enabled = lstForms.SelectedItem != null && resourceFileValid;
        }

        private void LoadAssembly(string fileName)
        {
            try
            {
                var assembly = Assembly.LoadFile(fileName);
                lstForms.Items.Clear();
                Task.Factory.StartNew(() =>
                {
                    foreach (var result in FormFinder.FindForms(assembly))
                    {
                        lstForms.BeginInvoke(new Action(() => { lstForms.Items.Add(result); }));
                    }
                });
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }));
            }
        }


        private void Convert(Type formType)
        {
            try
            {
                var resourceFile = new ResourceFile(txtResource.Text, chkReplace.Checked, !File.Exists(txtResource.Text));
                if (!resourceFile.IsValid())
                    throw new Exception("Resource File is not valid.");

                var useControlName = chkUseControlName.Checked;
                var generateResizeCode = chkResize.Checked;

                //execute these GDI methods on the GUI thread
                Graphics graphics = null;
                Font font = null;
                DialogUnitCalculation dialogUnitCalculation;
                try
                {
                    graphics = CreateGraphics();
                    font = new Font("MS Shell Dlg", 8);
                    dialogUnitCalculation = new DialogUnitCalculation();
                    dialogUnitCalculation.Init(graphics, font);
                    graphics.Dispose();
                    font.Dispose();
                }
                finally
                {
                    graphics?.Dispose();
                    font?.Dispose();
                }
                Task.Factory.StartNew(() =>
                {
                    btnConvert.BeginInvoke(new Action(() => { btnConvert.Enabled = false; }));
                    var converter = new FormConverter(formType, dialogUnitCalculation.ToDialogUnits, resourceFile.IsIdAvailable);
                    converter.UseControlName = useControlName;
                    converter.NoStaticControls = generateResizeCode;
                    SimpleLogger.GetLogger().Messages.Clear();
                    SimpleLogger.GetLogger().WriteLog($"Conversion started - Options: Use Control.Name {(useControlName ? "YES" : "NO")}, Replace {(resourceFile.Replace ? "YES" : "NO")}");
                    var result = converter.Convert();
                    resourceFile.Patch(result);
                    if (generateResizeCode)
                    {
                        var codeGen = new ResizeCodeGenerator(Path.GetDirectoryName(resourceFile.FileName));
                        codeGen.Generate(result.NewControlValues);
                    }
                    SimpleLogger.GetLogger().WriteLog("Done!");
                    ShowMessage(generateResizeCode);
                    btnConvert.BeginInvoke(new Action(() => { btnConvert.Enabled = true; }));
                });
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }));
            }
        }

        
        private void ShowMessage(bool resizeCode)
        {
            Invoke(new Action(() => { new ResultBox($"Done!{(resizeCode ? " See below for ControlResizer instructions!" : "")}", "Win32Forms", string.Join(Environment.NewLine, SimpleLogger.GetLogger().Messages), MessageBoxIcon.Information).ShowDialog(); }));
        }
    }
}

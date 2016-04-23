using System;
using System.CodeDom;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
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
            btnConvert.Enabled = lstForms.SelectedItem != null;
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
                //SelectResource(dlg.FileName);
            }
        }

        private void txtAssembly_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadAssembly(txtAssembly.Text);
            }
        }

        private void txtResource_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //SelectResource(txtResource.Text);
            }
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

        private void SelectResource(string fileName)
        {
            try
            {
                var resourceFile = new ResourceFile(fileName);
                if (!File.Exists(fileName))
                {
                    resourceFile.CreateNew();
                }
                resourceFile.IsValid();

                //var assembly = Assembly.LoadFile(fileName);
                //lstForms.Items.Clear();
                //Task.Factory.StartNew(() =>
                //{
                //    //foreach (var result in FormFinder.FindForms(assembly))
                //    //{
                //    //    lstForms.BeginInvoke(new Action(() => { lstForms.Items.Add(result); }));
                //    //}
                //});
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            var formResult = lstForms.SelectedItem as FormResult;
            if (formResult?.Type == null)
                return;
            Convert(formResult.Type);
        }

        private void Convert(Type formType)
        {
            try
            {
                if (!File.Exists(txtResource.Text))
                    throw new NotImplementedException("Please sleect an existing Resource File.");
                var resourceFile = new ResourceFile(txtResource.Text);
                if (!resourceFile.IsValid())
                    throw new Exception("Resource File is invalid.");

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
                    var converter = new FormConverter(formType, dialogUnitCalculation.ToDialogUnits);
                    var result = converter.Convert();
                    //CopyToClipboard(result.DialogContent);
                    UpdateResourceFile(resourceFile, result);
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

        private void UpdateResourceFile(ResourceFile resourceFile, ConvertResult result)
        {
            resourceFile.Patch(result);

        }

        private void CopyToClipboard(string text)
        {
            var clipboardThread = new Thread(() =>
            {
                Clipboard.SetText(text);
                Invoke(new Action(() => { MessageBox.Show(@"Exported to Clipboard", Text, MessageBoxButtons.OK, MessageBoxIcon.Information); }));
            });
            clipboardThread.SetApartmentState(ApartmentState.STA);
            clipboardThread.IsBackground = false;
            clipboardThread.Start();
        }
    }
}

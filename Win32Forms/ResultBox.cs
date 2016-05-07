using System.Drawing;
using System.Windows.Forms;

namespace Hiale.Win32Forms
{
    public partial class ResultBox : Form
    {
        public ResultBox(string text, string caption, string log, MessageBoxIcon icon)
        {
            InitializeComponent();
            SetIcon(icon);
            lblMessage.Text = text;
            txtLog.Text = log;
            Text = caption;
        }

        private void SetIcon(MessageBoxIcon messageBoxIcon)
        {
            switch (messageBoxIcon)
            {
                case MessageBoxIcon.None:
                    return;
                case MessageBoxIcon.Error:
                    //case MessageBoxIcon.Hand:
                    //case MessageBoxIcon.Stop:
                    picIcon.Image = SystemIcons.Error.ToBitmap();
                    break;
                case MessageBoxIcon.Question:
                    picIcon.Image = SystemIcons.Question.ToBitmap();
                    break;
                case MessageBoxIcon.Exclamation:
                    //case MessageBoxIcon.Warning:
                    picIcon.Image = SystemIcons.Exclamation.ToBitmap();
                    break;
                case MessageBoxIcon.Asterisk:
                    //case MessageBoxIcon.Information:
                    picIcon.Image = SystemIcons.Information.ToBitmap();
                    break;
            }
        }
        
        private void btnOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

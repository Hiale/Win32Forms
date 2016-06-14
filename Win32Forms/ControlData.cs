using System.Windows.Forms;

namespace Hiale.Win32Forms
{
    public class ControlData
    {
        public string ControlId { get; }

        public string ParentId { get; set; }

        public AnchorStyles Anchor { get; set; }

        public ControlData(string controlId)
        {
            ControlId = controlId;
            Anchor = AnchorStyles.Top | AnchorStyles.Left;
        }
    }
}

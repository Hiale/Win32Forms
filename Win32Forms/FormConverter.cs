using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Hiale.Win32Forms
{
    public class FormConverter
    {
        private readonly StringBuilder _stringBuilder;
        private readonly Dictionary<string, int> _commandIdMap;
        private readonly Type _formType;
        private readonly DialogUnitCalculation.CalculateDialogUnits _toDialogUnits;
        private readonly Func<string, bool> _isIdAvailable; //returns true if the given name is not yet present and can be used.

        private Form _formInstance;
        private ConvertResult _result;

        public bool UseControlName { get; set; }

        public FormConverter(Type formType, DialogUnitCalculation.CalculateDialogUnits toDialogUnits, Func<string, bool> isIdAvailable)
        {
            _stringBuilder = new StringBuilder();
            _commandIdMap = new Dictionary<string, int>();
            _toDialogUnits = toDialogUnits;
            _isIdAvailable = isIdAvailable;
            UseControlName = true;
            _formType = formType;
        }

        public ConvertResult Convert()
        {
            _stringBuilder.Clear();
            _commandIdMap.Clear();
            _result = new ConvertResult();
            _formInstance = (Form)Activator.CreateInstance(_formType);
            ProcessControl(_formInstance);
            _stringBuilder.AppendLine("END");
            _result.DialogContent = _stringBuilder.ToString();
            _result.NewResourceValue = GetDialogId();
            return _result;
        }

        private static bool IsSameOrSubclass(Type targetClass, Type baseClass)
        {
            return targetClass.IsSubclassOf(baseClass) || targetClass == baseClass;
        }

        private void AddControlType(string controlType)
        {
            _stringBuilder.Append(new string(' ', 4));
            _stringBuilder.Append(controlType);
            _stringBuilder.Append(new string(' ', 20 - (4 + controlType.Length)));
        }

        private void AddControl(Control control, string controlType, string controlId, HashSet<string> styles)
        {
            var style = GetStyles(styles);
            AddControlType(controlType);
            _stringBuilder.AppendLine(controlType.ToUpper() == "CONTROL"
                ? $"\"{control.Text}\",{controlId},\"Button\"{style},{CalculateDimension(control)}"
                : $"\"{control.Text}\",{controlId},{CalculateDimension(control)}{style}");
        }

        private void ProcessControl(Control control)
        {
            if (IsSameOrSubclass(control.GetType(), typeof (Form)))
            {
                ProcessForm(control as Form);
                _stringBuilder.AppendLine("BEGIN");
            }
            else if (IsSameOrSubclass(control.GetType(), typeof(Button)))
            {
                ProcessButton(control as Button, _formInstance);
            }
            else if (IsSameOrSubclass(control.GetType(), typeof(TextBox)))
            {
                ProcessTextBox(control as TextBox);
            }
            else if (IsSameOrSubclass(control.GetType(), typeof(Label)))
            {
                ProcessLabel(control as Label);
            }
            else if (IsSameOrSubclass(control.GetType(), typeof(GroupBox)))
            {
                ProcessGroupBox(control as GroupBox);
            }
            else if (IsSameOrSubclass(control.GetType(), typeof(CheckBox)))
            {
                ProcessCheckBox(control as CheckBox);
            }
            else if (IsSameOrSubclass(control.GetType(), typeof(RadioButton)))
            {
                ProcessRadioButton(control as RadioButton);
            }
            else if (IsSameOrSubclass(control.GetType(), typeof(ComboBox)))
            {
                ProcessComboBox(control as ComboBox);
            }
            else if (IsSameOrSubclass(control.GetType(), typeof (ListBox)))
            {
                ProcessListBox(control as ListBox);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Control Type {control.GetType()} not implemented.");
            }
            foreach (Control childControl in control.Controls)
            {
                ProcessControl(childControl);
            }
        }

        private string GetId(Control control, string defaultName)
        {
            string commandId;
            if (UseControlName)
            {
                if (!string.IsNullOrEmpty(control.Name))
                {
                    commandId = control.Name.ToUpper();
                    if (_isIdAvailable(commandId))
                    {
                        _result.NewControlValues.Add(commandId);
                        return commandId;
                    }
                }
            }
            while (true)
            {
                int value;
                if (_commandIdMap.TryGetValue(defaultName, out value))
                {
                    _commandIdMap[defaultName] = ++value;
                }
                else
                {
                    value = 1;
                    _commandIdMap.Add(defaultName, value);
                }
                commandId = $"IDC_{defaultName}{value}";
                if (_isIdAvailable(commandId))
                { 
                    _result.NewControlValues.Add(commandId);
                    return commandId;
                }
            }
        }

        private string GetDialogId()
        {
            string dialogId;
            if (UseControlName && !string.IsNullOrEmpty(_formInstance.Name))
            {
                dialogId = "IDD_" + _formInstance.Name.ToUpper();
                if (_isIdAvailable(dialogId))
                    return dialogId;
            }
            else
            {
                dialogId = "IDD_DIALOG";
                if (_isIdAvailable(dialogId))
                    return dialogId;
                var index = 1;
                while (true)
                {
                    dialogId = "IDD_DIALOG" + index;
                    index++;
                    if (_isIdAvailable(dialogId))
                        break;
                }
            }
            return dialogId;
        }

        private string CalculateDimension(Control control, bool useSpaces = false)
        {
            int x, y, width, height;
            if (control.Parent == null)
            {
                _toDialogUnits(control.Location.X, control.Location.Y, out x, out y);
            }
            else
            {
                // ReSharper disable once PossibleNullReferenceException
                var locationOnForm = control.FindForm().PointToClient(control.Parent.PointToScreen(control.Location));
                _toDialogUnits(locationOnForm.X, locationOnForm.Y, out x, out y);
            }
            _toDialogUnits(control.ClientSize.Width, control.ClientSize.Height, out width, out height);
            return useSpaces ? $"{x}, {y}, {width}, {height}" : $"{x},{y},{width},{height}";
        }

        private static HashSet<string> GetCommonStyles(Control control)
        {
            var styles = new HashSet<string>();
            //if (!control.Visible)
            //    styles.Add("NOT WS_VISIBLE");
            if (!control.Enabled)
                styles.Add("WS_DISABLED");
            if (!control.TabStop)
                styles.Add("NOT WS_TABSTOP");
            return styles;
        }

        private static void GetContentAllignmentStyle(ContentAlignment contentAlignment, IEnumerable<string> defaultStyles, HashSet<string> styles)
        {
            switch (contentAlignment)
            {
                case ContentAlignment.TopLeft:
                    styles.Add("BS_TOP");
                    styles.Add("BS_LEFT");
                    break;
                case ContentAlignment.TopCenter:
                    styles.Add("BS_TOP");
                    styles.Add("BS_CENTER");
                    break;
                case ContentAlignment.TopRight:
                    styles.Add("BS_TOP");
                    styles.Add("BS_RIGHT");
                    break;
                case ContentAlignment.MiddleLeft:
                    styles.Add("BS_VCENTER");
                    styles.Add("BS_LEFT");
                    break;
                case ContentAlignment.MiddleCenter:
                    styles.Add("BS_VCENTER");
                    styles.Add("BS_CENTER");
                    break;
                case ContentAlignment.MiddleRight:
                    styles.Add("BS_VCENTER");
                    styles.Add("BS_RIGHT");
                    break;
                case ContentAlignment.BottomLeft:
                    styles.Add("BS_BOTTOM");
                    styles.Add("BS_LEFT");
                    break;
                case ContentAlignment.BottomCenter:
                    styles.Add("BS_BOTTOM");
                    styles.Add("BS_CENTER");
                    break;
                case ContentAlignment.BottomRight:
                    styles.Add("BS_BOTTOM");
                    styles.Add("BS_RIGHT");
                    break;
            }
            foreach (var defaultStyle in defaultStyles)
            {
                styles.Remove(defaultStyle);
            }
        }

        private static string GetStyles(ICollection<string> styles, bool leadingComma = true)
        {
            var style = string.Empty;
            if (styles.Count > 0)
                style = (leadingComma ? "," : string.Empty) + string.Join(" | ", styles);
            return style;
        }

        private void ProcessForm(Form control)
        {
            //var dialogName = "IDD_" + control.GetType().Name.ToUpper();
            var dialogName = GetDialogId();

            var styles = new HashSet<string>();
            var exStyles = new HashSet<string>();
            styles.Add("DS_SETFONT");
            styles.Add("WS_POPUP");
            switch (control.FormBorderStyle)
            {
                case FormBorderStyle.None:
                    //empty
                    break;
                case FormBorderStyle.FixedSingle:
                    styles.Add("WS_CAPTION");
                    styles.Add("WS_SYSMENU");
                    break;
                case FormBorderStyle.Fixed3D:
                    exStyles.Add("WS_EX_STATICEDGE");
                    styles.Add("WS_CAPTION");
                    styles.Add("WS_SYSMENU");
                    break;
                case FormBorderStyle.FixedDialog:
                    styles.Add("DS_MODALFRAME");
                    styles.Add("WS_CAPTION");
                    styles.Add("WS_SYSMENU");
                    break;
                case FormBorderStyle.Sizable:
                    styles.Add("WS_THICKFRAME");
                    styles.Add("WS_CAPTION");
                    styles.Add("WS_SYSMENU");
                    break;
                case FormBorderStyle.FixedToolWindow:
                    styles.Add("WS_CAPTION");
                    styles.Add("WS_SYSMENU");
                    exStyles.Add("WS_EX_TOOLWINDOW");
                    break;
                case FormBorderStyle.SizableToolWindow:
                    styles.Add("WS_CAPTION");
                    styles.Add("WS_SYSMENU");
                    styles.Add("WS_THICKFRAME");
                    exStyles.Add("WS_EX_TOOLWINDOW");
                    break;
            }
            if (control.FormBorderStyle == FormBorderStyle.FixedSingle ||
                control.FormBorderStyle == FormBorderStyle.Fixed3D ||
                control.FormBorderStyle == FormBorderStyle.FixedDialog ||
                control.FormBorderStyle == FormBorderStyle.Sizable)
            {
                if (control.MinimizeBox)
                    styles.Add("WS_MINIMIZEBOX");
                if (control.MaximizeBox)
                    styles.Add("WS_MAXIMIZEBOX");
            }
            if (!control.Enabled)
                styles.Add("WS_DISABLED");
            if (control.TopMost)
                styles.Add("DS_SYSMODAL");
            if (control.ShowInTaskbar)
                exStyles.Add("WS_EX_APPWINDOW");
            if (control.StartPosition == FormStartPosition.CenterScreen ||
                control.StartPosition == FormStartPosition.CenterParent)
                styles.Add("DS_CENTER");

            _stringBuilder.AppendLine($"{dialogName} DIALOGEX {CalculateDimension(control, true)}");
            _stringBuilder.AppendLine("STYLE " + GetStyles(styles, false));
            if (exStyles.Any())
                _stringBuilder.AppendLine("EXSTYLE " + GetStyles(exStyles, false));
            _stringBuilder.AppendLine($"CAPTION \"{control.Text}\"");
            _stringBuilder.AppendLine("FONT 8, \"MS Shell Dlg\", 0, 0, 0x1"); //ToDo
        }

        private void ProcessButton(Button control, Form form)
        {
            var styles = GetCommonStyles(control);
            GetContentAllignmentStyle(control.TextAlign, new[] { "BS_CENTER", "BS_VCENTER" }, styles);
            AddControl(control, control.Equals(form.AcceptButton) ? "DEFPUSHBUTTON" : "PUSHBUTTON", GetId(control, "BUTTON"), styles);
        }

        private void ProcessLabel(Label control)
        {
            var styles = GetCommonStyles(control);
            styles.Remove("NOT WS_TABSTOP");
            if (control.AutoEllipsis)
                styles.Add("SS_WORDELLIPSIS");
            GetContentAllignmentStyle(control.TextAlign, new[] { "BS_TOP", "BS_LEFT" }, styles);
            AddControl(control, "LTEXT", "IDC_STATIC", styles);
        }

        private void ProcessGroupBox(GroupBox control)
        {
            var styles = GetCommonStyles(control);
            styles.Remove("NOT WS_TABSTOP");
            AddControl(control, "GROUPBOX", "IDC_STATIC", styles);
        }

        private void ProcessTextBox(TextBox control)
        {
            var styles = GetCommonStyles(control);
            if (control.Multiline)
                styles.Add("ES_MULTILINE");
            else if (control.PasswordChar != '\0')
                styles.Add("ES_PASSWORD");
            if (control.ReadOnly)
                styles.Add("ES_READONLY");
            switch (control.TextAlign)
            {
                case HorizontalAlignment.Left: //default
                    break;
                case HorizontalAlignment.Right:
                    styles.Add("ES_RIGHT");
                    break;
                case HorizontalAlignment.Center:
                    styles.Add("ES_CENTER");
                    break;
            }
            AddControlType("EDITTEXT");
            _stringBuilder.Append(GetId(control, "EDIT") + ",");
            _stringBuilder.Append(CalculateDimension(control));
            _stringBuilder.Append(GetStyles(styles));
            _stringBuilder.Append(Environment.NewLine);
        }

        private void ProcessCheckBox(CheckBox control)
        {
            var styles = GetCommonStyles(control);
            GetContentAllignmentStyle(control.TextAlign, new[] { "BS_VCENTER", "BS_LEFT" }, styles);
            styles.Add(control.ThreeState ? "BS_AUTO3STATE" : "BS_AUTOCHECKBOX");
            AddControl(control, "CONTROL", GetId(control, "CHECK"), styles);
        }

        private void ProcessRadioButton(RadioButton control)
        {
            var styles = GetCommonStyles(control);
            GetContentAllignmentStyle(control.TextAlign, new[] { "BS_VCENTER", "BS_LEFT" }, styles);
            styles.Add("BS_AUTORADIOBUTTON");
            AddControl(control, "CONTROL", GetId(control, "RADIO"), styles);
        }

        private void ProcessComboBox(ComboBox control)
        {
            var styles = GetCommonStyles(control);
            switch (control.DropDownStyle)
            {
                case ComboBoxStyle.Simple:
                    styles.Add("CBS_SIMPLE");
                    break;
                case ComboBoxStyle.DropDown:
                    styles.Add("CBS_DROPDOWN");
                    break;
                case ComboBoxStyle.DropDownList:
                    styles.Add("CBS_DROPDOWNLIST");
                    break;
            }
            if (control.Sorted)
                styles.Add("CBS_SORT");
            AddControl(control, "COMBOBOX", GetId(control, "COMBO"), styles);

            AddControlType("COMBOBOX");
            _stringBuilder.Append(GetId(control, "COMBO") + ",");
            _stringBuilder.Append(CalculateDimension(control));
            _stringBuilder.Append(GetStyles(styles));
            _stringBuilder.Append(Environment.NewLine);

        }

        private void ProcessListBox(ListBox control)
        {
            var styles = GetCommonStyles(control);
            switch (control.SelectionMode)
            {
                case SelectionMode.None:
                    styles.Add("LBS_NOSEL");
                    break;
                case SelectionMode.One: //default
                    break;
                case SelectionMode.MultiSimple:
                    styles.Add("LBS_MULTIPLESEL");
                    break;
                case SelectionMode.MultiExtended:
                    styles.Add("LBS_EXTENDEDSEL");
                    break;
            }
            if (control.Sorted)
                styles.Add("LBS_SORT");

            AddControlType("LISTBOX");
            _stringBuilder.Append(GetId(control, "LIST") + ",");
            _stringBuilder.Append(CalculateDimension(control));
            _stringBuilder.Append(GetStyles(styles));
            _stringBuilder.Append(Environment.NewLine);
        }

    }
}

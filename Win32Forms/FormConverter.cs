using System;
using System.Collections.Generic;
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

        private static string GetStyles(ICollection<string> styles)
        {
            var style = string.Empty;
            if (styles.Count > 0)
                style = "," + string.Join(" | ", styles);
            return style;
        }

        private void AddControl(Control control, string controlType, string controlId, List<string> styles)
        {
            var style = GetStyles(styles);
            AddControlType(controlType);
            if (controlType.ToUpper() == "CONTROL")
            {
                _stringBuilder.AppendLine($"\"{control.Text}\",{controlId},\"Button\",{style},{CalculateDimension(control)}");
            }
            else
            {
                _stringBuilder.AppendLine($"\"{control.Text}\",{controlId},{CalculateDimension(control)}{style}");
            }
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
                AddControl(control, "GROUPBOX", "IDC_STATIC", ProcessCommonStyles(control));
            }
            else if (IsSameOrSubclass(control.GetType(), typeof(CheckBox)))
            {
                ProcessCheckBox(control as CheckBox);
            }
            else if (IsSameOrSubclass(control.GetType(), typeof(RadioButton)))
            {
                ProcessRadioButton(control as RadioButton);
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

        private string CalculateDimension(Control control)
        {
            int x, y, width, height;
            if (control.Parent == null)
            {
                _toDialogUnits(control.Location.X, control.Location.Y, out x, out y);
            }
            else
            {
                var locationOnForm = control.FindForm().PointToClient(control.Parent.PointToScreen(control.Location));
                _toDialogUnits(locationOnForm.X, locationOnForm.Y, out x, out y);
            }
            _toDialogUnits(control.ClientSize.Width, control.ClientSize.Height, out width, out height);
            return $"{x},{y},{width},{height}";
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

        private static List<string> ProcessCommonStyles(Control control)
        {
            var styles = new List<string>();
            //if (!control.Visible)
            //    styles.Add("NOT WS_VISIBLE");
            if (!control.Enabled)
                styles.Add("WS_DISABLED");
            return styles;

            //Horizontal Alignment
            //Default -> ""
            //Left -> BS_LEFT
            //Right -> BS_RIGHT
            //Center -> BS_CENTER

            //Vertical Alignment
            //Default -> ""
            //Top -> BS_TOP
            //Bottom -> BS_BOTTOM
            //Center -> BS_VCENTER

            //Default Button
            //False -> PUSHBUTTON
            //True -> DEFPUSHBUTTON

            //Multiline -> BS_MULTILINE

            //ID -> IDOK

            //Right Align Text -> WS_EX_RIGHT   ???

        }

        private void ProcessForm(Form control)
        {
            var dialogName = "IDD_" + control.GetType().Name.ToUpper();
            _stringBuilder.AppendLine($"{dialogName} DIALOGEX {CalculateDimension(control)}");
            _stringBuilder.AppendLine("STYLE DS_SETFONT | DS_MODALFRAME | DS_FIXEDSYS | WS_POPUP | WS_CAPTION | WS_SYSMENU"); //ToDo
            _stringBuilder.AppendLine($"CAPTION \"{control.Text}\"");
            _stringBuilder.AppendLine($"FONT 8, \"MS Shell Dlg\", 0, 0, 0x1"); //ToDo
        }

        private void ProcessButton(Button control, Form form)
        {
            var styles = ProcessCommonStyles(control);
            AddControl(control, control.Equals(form.AcceptButton) ? "DEFPUSHBUTTON" : "PUSHBUTTON", GetId(control, "BUTTON"), styles);
        }

        private void ProcessLabel(Label control)
        {
            var styles = ProcessCommonStyles(control);
            AddControl(control, "LTEXT", "IDC_STATIC", styles);
        }

        private void ProcessTextBox(TextBox control)
        {
            var styles = ProcessCommonStyles(control);
            AddControlType("EDITTEXT");
            _stringBuilder.Append(GetId(control, "EDIT") + ",");
            _stringBuilder.Append(CalculateDimension(control));
            _stringBuilder.Append(GetStyles(styles));
            _stringBuilder.Append(Environment.NewLine);
        }

        private void ProcessCheckBox(CheckBox control)
        {
            var styles = ProcessCommonStyles(control);
            styles.Add(control.ThreeState ? "BS_AUTO3STATE" : "BS_AUTOCHECKBOX");
            AddControl(control, "CONTROL", GetId(control, "CHECK"), styles);
        }

        private void ProcessRadioButton(RadioButton control)
        {
            var styles = ProcessCommonStyles(control);
            styles.Add("BS_AUTORADIOBUTTON");
            AddControl(control, "CONTROL", GetId(control, "RADIO"), styles);
        }

    }
}

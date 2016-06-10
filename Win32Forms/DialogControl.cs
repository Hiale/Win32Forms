using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hiale.Win32Forms
{
    public enum ControlPropertiesOrder
    {
        IdDimensionsStyles,
        TextIdDimensionStyles,
        TextIdSubtypeStylesDimension,
        ReferenceIdDimensionStyles
    }

    public class DialogControl
    {
        public static List<DialogControl> ControlTypes; 

        public string Type { get; }

        public string SubType { get; }

        public string Id { get; set; }

        public ControlPropertiesOrder PropertiesOrder { get; }

        public string DefaultId { get; }

        public bool IsDefaultStatic { get; }

        private static readonly Regex RegEx;
        private static readonly Regex RegExText;

        private DialogControl(string type, ControlPropertiesOrder propertiesOrder, string defaultId, bool defaultStatic = false)
        {
            Type = type;
            PropertiesOrder = propertiesOrder;
            DefaultId = defaultId;
            IsDefaultStatic = defaultStatic;
        }

        private DialogControl(string type, string subType, ControlPropertiesOrder propertiesOrder, string defaultId) : this(type, propertiesOrder, defaultId)
        {
            SubType = subType;
        }

        static DialogControl()
        {
            RegEx = new Regex(@"^\s*(\S+)\s+(.*)$");
            RegExText = new Regex(@"""+");
            ControlTypes = new List<DialogControl>
            {
                new DialogControl("PUSHBUTTON", ControlPropertiesOrder.TextIdDimensionStyles, "BUTTON"),
                new DialogControl("DEFPUSHBUTTON", ControlPropertiesOrder.TextIdDimensionStyles, "BUTTON"),
                new DialogControl("LTEXT", ControlPropertiesOrder.TextIdDimensionStyles, "TEXT", true),
                new DialogControl("CTEXT", ControlPropertiesOrder.TextIdDimensionStyles, "TEXT", true),
                new DialogControl("RTEXT", ControlPropertiesOrder.TextIdDimensionStyles, "TEXT", true),
                new DialogControl("GROUPBOX", ControlPropertiesOrder.TextIdDimensionStyles, "GROUP", true),
                new DialogControl("EDITTEXT", ControlPropertiesOrder.IdDimensionsStyles, "EDIT"),
                new DialogControl("CONTROL", "CHECK", ControlPropertiesOrder.TextIdSubtypeStylesDimension, "CHECK"),
                new DialogControl("CONTROL", "RADIO", ControlPropertiesOrder.TextIdSubtypeStylesDimension, "RADIO"),
                new DialogControl("COMBOBOX", ControlPropertiesOrder.IdDimensionsStyles, "COMBO"),
                new DialogControl("LISTBOX", ControlPropertiesOrder.IdDimensionsStyles, "LIST")
            };
        }


        public static DialogControl GetControlType(string type)
        {
            return ControlTypes.FirstOrDefault(controlType => controlType.Type == type);
        }

        public static DialogControl GetControlType(string type, string subType)
        {
            return ControlTypes.FirstOrDefault(controlType => controlType.Type == type && controlType.SubType == subType);
        }

        public static string ParseId(string input)
        {
            try
            {
                var match = RegEx.Match(input);
                if (!match.Success)
                    return null;
                var type = match.Groups[1].Value;
                var properties = match.Groups[2].Value;
                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(properties))
                    return null;
                var targetTypes = ControlTypes.Where(controlType => controlType.Type == type).ToList();
                ControlPropertiesOrder propertiesOrder;
                if (targetTypes.Count == 1)
                {
                    var targetType = targetTypes.First();
                    propertiesOrder = targetType.PropertiesOrder;
                }
                else
                {
                    var orders = new HashSet<ControlPropertiesOrder>();
                    foreach (var controlType in targetTypes)
                    {
                        orders.Add(controlType.PropertiesOrder);
                    }
                    if (orders.Count != 1)
                        return null;
                    propertiesOrder = orders.First();
                }
                var id = string.Empty;
                switch (propertiesOrder)
                {
                    case ControlPropertiesOrder.IdDimensionsStyles:

                        var parts = properties.Split(',');
                        id = parts[0];
                        break;
                    case ControlPropertiesOrder.TextIdDimensionStyles:
                    case ControlPropertiesOrder.TextIdSubtypeStylesDimension:
                        var text = FindText(properties);
                        properties = properties.Replace(text, string.Empty);
                        if (properties.StartsWith(","))
                            properties = properties.Substring(1);
                        id = properties.Substring(0, properties.IndexOf(",", StringComparison.Ordinal));
                        break;
                }
                if (string.IsNullOrEmpty(id))
                    return null;
                return id.ToUpper() == "IDC_STATIC" ? null : id;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string FindText(string input)
        {
            int? start = null;
            int? end = null;

            //find single occurrences of the doublw quote char ", in case the actual text contains a double quote it gets escaped by adding another double quote.
            foreach (Match match in RegExText.Matches(input))
            {
                if (match.Length % 2 == 0)
                    continue;
                if (start == null)
                {
                    start = match.Index;
                    continue;
                }
                end = match.Index + match.Length - 1;
                break;
            }
            if (start.HasValue && end.HasValue)
                return input.Substring(start.Value, end.Value - start.Value + 1);
            return string.Empty;
        }

        public override string ToString()
        {
            return Type;
        }
    }
}

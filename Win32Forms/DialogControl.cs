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

        public bool IsStatic { get; }

        private static readonly Regex RegEx;

        private DialogControl(string type, ControlPropertiesOrder propertiesOrder, string defaultId)
        {
            Type = type;
            PropertiesOrder = propertiesOrder;
            DefaultId = defaultId;
        }

        private DialogControl(string type, string subType, ControlPropertiesOrder propertiesOrder, string defaultId) : this(type, propertiesOrder, defaultId)
        {
            SubType = subType;
        }

        private DialogControl(string type, ControlPropertiesOrder propertiesOrder) : this(type, propertiesOrder, null)
        {
            IsStatic = true;
        }

        static DialogControl()
        {
            RegEx = new Regex(@"^\s*(\S+)\s+(.*)$");
            ControlTypes = new List<DialogControl>
            {
                new DialogControl("PUSHBUTTON", ControlPropertiesOrder.TextIdDimensionStyles, "BUTTON"),
                new DialogControl("DEFPUSHBUTTON", ControlPropertiesOrder.TextIdDimensionStyles, "BUTTON"),
                new DialogControl("LTEXT", ControlPropertiesOrder.TextIdDimensionStyles),
                new DialogControl("GROUPBOX", ControlPropertiesOrder.TextIdDimensionStyles),
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
                bool isStatic;
                if (targetTypes.Count == 1)
                {
                    var targetType = targetTypes.First();
                    propertiesOrder = targetType.PropertiesOrder;
                    isStatic = targetType.IsStatic;
                }
                else
                {
                    var orders = new HashSet<ControlPropertiesOrder>();
                    var isStaticList = new HashSet<bool>();
                    foreach (var controlType in targetTypes)
                    {
                        orders.Add(controlType.PropertiesOrder);
                        isStaticList.Add(controlType.IsStatic);

                    }
                    if (orders.Count != 1 || isStaticList.Count != 1)
                        return null;
                    propertiesOrder = orders.First();
                    isStatic = isStaticList.First();
                }
                if (isStatic)
                    return null;
                switch (propertiesOrder)
                {
                    case ControlPropertiesOrder.IdDimensionsStyles:
                        var parts = properties.Split(',');
                        return parts[0];
                    case ControlPropertiesOrder.TextIdDimensionStyles:
                    case ControlPropertiesOrder.TextIdSubtypeStylesDimension:
                        var text = FindText(properties);
                        properties = properties.Replace(text, string.Empty);
                        if (properties.StartsWith(","))
                            properties = properties.Substring(1);
                        return properties.Substring(0, properties.IndexOf(",", StringComparison.Ordinal));
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        private static string FindText(string input)
        {
            var start = input.IndexOf("\"", StringComparison.Ordinal);
            var end = input.LastIndexOf("\"", StringComparison.Ordinal);
            return input.Substring(start, end - start + 1);
        }

        public override string ToString()
        {
            return Type;
        }
    }
}

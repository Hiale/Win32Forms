using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.Win32Forms
{
    public class ResourceHeaderEntry
    {
        public string Name { get; }

        public int Value { get; set; }

        public int LineNumber { get; }

        public ResourceHeaderEntry(string name, int value, int lineNumber)
        {
            Name = name;
            Value = value;
            LineNumber = lineNumber;
        }

        public override string ToString()
        {
            return $"[{LineNumber}] {Name}: {Value}";
        }
    }
}

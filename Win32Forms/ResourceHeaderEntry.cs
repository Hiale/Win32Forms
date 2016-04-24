
namespace Hiale.Win32Forms
{
    public class ResourceHeaderEntry
    {
        public string Name { get; }

        public int Value { get; set; }

        public int LineNumber { get; }

        public bool Special { get; }

        public ResourceHeaderEntry(string name, int value, int lineNumber, bool special = false)
        {
            Name = name;
            Value = value;
            LineNumber = lineNumber;
            Special = special;
        }

        public override string ToString()
        {
            return $"[{LineNumber}] {Name}: {Value}";
        }
    }
}

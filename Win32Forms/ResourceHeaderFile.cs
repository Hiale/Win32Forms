using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hiale.Win32Forms
{
    public class ResourceHeaderFile
    {
        private class ResourceHeaderEntry
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

        private const string NextResourceValue = "_APS_NEXT_RESOURCE_VALUE";
        private const string NextControlValue = "_APS_NEXT_CONTROL_VALUE";

        public string FileName { get; }

        private List<string> _lines; 
        private readonly List<ResourceHeaderEntry> _lineMap; 

        public ResourceHeaderFile(string fileName)
        {
            _lines = new List<string>();
            _lineMap = new List<ResourceHeaderEntry>();
            FileName = fileName;
            Read();
        }

        public bool IsValid()
        {
            return true;
        }

        public void AddResource(string name)
        {
            AddEntry(NextResourceValue, name);
        }

        public void AddControl(string name)
        {
            AddEntry(NextControlValue, name);
        }

        public void Write()
        {
            File.WriteAllLines(FileName, _lines, Encoding.UTF8);
        }

        private void Read()
        {
            _lines = File.ReadLines(FileName, Encoding.UTF8).ToList();
            CreateLineMap();
        }

        private static string GenerateEntry(string name, int value)
        {
            var entry = "#define " + name;
            if (entry.Length < 40)
                return entry + new string(' ', 40 - entry.Length) + value;
            return entry + " " + value;
        }

        private void AddEntry(string type, string name)
        {
            var value = GetNextValue(type);
            var line = FindLine(value);
            _lines.Insert(line, GenerateEntry(name, value));
            CreateLineMap();
            IncrementValue(type);
        }

        private int GetNextValue(string name)
        {
            foreach (var entry in _lineMap.Where(entry => entry.Name == name))
            {
                return entry.Value;
            }
            throw new Exception("Invalid Resource Header File");
        }

        private void CreateLineMap()
        {
            _lineMap.Clear();
            for (var i = 0; i < _lines.Count; i++)
            {
                var regEx = new Regex(@"#define\s+(\S*)\s+(\d+)");
                var match = regEx.Match(_lines[i]);
                if (match.Success)
                {
                    int value;
                    if (int.TryParse(match.Groups[2].Value, out value))
                    {
                        _lineMap.Add(new ResourceHeaderEntry(match.Groups[1].Value, value, i));
                    }
                }
            }
        }

        private int FindLine(int newId)
        {
            foreach (var entry in _lineMap)
            {
                if (newId < entry.Value)
                {
                    return entry.LineNumber;
                }
            }
            return 4;
        }

        private void IncrementValue(string name)
        {
            foreach (var entry in _lineMap.Where(entry => entry.Name == name))
            {
                var newValue = entry.Value + 1;
                _lines[entry.LineNumber] = _lines[entry.LineNumber].Replace(entry.Value.ToString(), newValue.ToString());
                entry.Value = newValue;
            }
        }
    }
}

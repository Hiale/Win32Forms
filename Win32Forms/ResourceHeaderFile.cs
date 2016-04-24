﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Hiale.Win32Forms
{
    public class ResourceHeaderFile
    {
        private const string NextResourceValue = "_APS_NEXT_RESOURCE_VALUE";
        private const string NextControlValue = "_APS_NEXT_CONTROL_VALUE";

        private List<string> _lines;

        public string FileName { get; }

        public List<ResourceHeaderEntry> Entries { get; }

        public ResourceHeaderFile(string fileName, bool embedded = false)
        {
            _lines = new List<string>();
            Entries = new List<ResourceHeaderEntry>();
            FileName = fileName;
            if (embedded)
                ReadEmbedded();
            else
                Read();
        }

        public bool IsValid()
        {
            return GetNextValue(NextResourceValue) > -1 && GetNextValue(NextControlValue) > -1;
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

        private void ReadEmbedded()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetType().Namespace + ".Template.resource.h";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    _lines.Add(line);
                }
            }
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
            if (value < 0)
                throw new Exception("Invalid Resource Header File.");
            var line = FindLine(value);
            _lines.Insert(line, GenerateEntry(name, value));
            CreateLineMap();
            IncrementValue(type);
        }

        private int GetNextValue(string name)
        {
            foreach (var entry in Entries.Where(entry => entry.Name == name))
            {
                return entry.Value;
            }
            return -1;
        }

        private void CreateLineMap()
        {
            Entries.Clear();
            for (var i = 0; i < _lines.Count; i++)
            {
                var regEx = new Regex(@"#define\s+(\S*)\s+(\d+)");
                var match = regEx.Match(_lines[i]);
                if (match.Success)
                {
                    int value;
                    if (int.TryParse(match.Groups[2].Value, out value))
                        Entries.Add(new ResourceHeaderEntry(match.Groups[1].Value, value, i));
                }
            }
        }

        private int FindLine(int newId) //ToDo: Controls are written ahead of Resources
        {
            foreach (var entry in Entries)
            {
                if (entry.Name == "_APS_NEXT_RESOURCE_VALUE" || entry.Name == "_APS_NEXT_COMMAND_VALUE" || entry.Name == "_APS_NEXT_CONTROL_VALUE" || entry.Name == "_APS_NEXT_SYMED_VALUE")
                    continue;
                if (newId < entry.Value)
                    return entry.LineNumber;
            }
            return 4;
        }

        private void IncrementValue(string name)
        {
            foreach (var entry in Entries.Where(entry => entry.Name == name))
            {
                var newValue = entry.Value + 1;
                _lines[entry.LineNumber] = _lines[entry.LineNumber].Replace(entry.Value.ToString(), newValue.ToString());
                entry.Value = newValue;
            }
        }
    }
}

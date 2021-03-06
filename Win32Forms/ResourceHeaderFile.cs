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

        public ResourceFile ResourceFile { get; }

        public List<ResourceHeaderEntry> Entries { get; }

        public ResourceHeaderFile(string fileName, ResourceFile resourceFile, bool embedded = false)
        {
            _lines = new List<string>();
            Entries = new List<ResourceHeaderEntry>();
            FileName = fileName;
            ResourceFile = resourceFile;
            if (embedded)
                ReadEmbedded();
            else
                Read();
        }

        public bool IsValid()
        {
            return GetValue(NextResourceValue) > -1 && GetValue(NextControlValue) > -1;
        }

        public int AddResource(string name)
        {
            return AddEntry(NextResourceValue, name);
        }

        public int AddControl(string name)
        {
            return AddEntry(NextControlValue, name);
        }

        public void RemoveEntry(string name)
        {
            var regEx = new Regex($@"#define\s+{name}\s+.+");
            var line = -1;
            for (var i = 0; i < _lines.Count; i++)
            {
                if (!regEx.IsMatch(_lines[i]))
                    continue;
                line = i;
                break;
            }
            if (line < 0)
                return;
            _lines.RemoveAt(line);
            CreateLineMap();
        }

        public void Write()
        {
            File.WriteAllLines(FileName, _lines, Encoding.UTF8);
        }

        private void Read()
        {
            _lines = File.ReadLines(FileName, Extension.GetEncoding(FileName)).ToList();
            CreateLineMap();
        }

        private void ReadEmbedded()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetType().Namespace + ".Template.resource.h";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var regEx = new Regex(@"Used by (.*)");
                bool checkRegEx = true;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (checkRegEx && regEx.IsMatch(line))
                    {
                        line = regEx.Replace(line, "Used by " + Path.GetFileName(ResourceFile.FileName));
                        checkRegEx = false;
                    }
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

        private int AddEntry(string type, string name)
        {
            var existingValue = GetValue(name);
            if (existingValue != null)
                return existingValue.Value;
            var value = GetValue(type);
            if (value == null)
                throw new Exception("Invalid Resource Header File.");
            var line = FindLine(value.Value);
            _lines.Insert(line, GenerateEntry(name, value.Value));
            CreateLineMap();
            IncrementValue(type);
            return value.Value;
        }

        private int? GetValue(string name)
        {
            foreach (var entry in Entries.Where(entry => entry.Name == name))
            {
                return entry.Value;
            }
            return null;
        }

        private void CreateLineMap()
        {
            Entries.Clear();
            var regEx = new Regex(@"#define\s+(\S*)\s+(\d+)");
            for (var i = 0; i < _lines.Count; i++)
            {
                var match = regEx.Match(_lines[i]);
                if (!match.Success)
                    continue;
                int value;
                if (!int.TryParse(match.Groups[2].Value, out value))
                    continue;
                var name = match.Groups[1].Value;
                var special = name == "_APS_NEXT_RESOURCE_VALUE" || name == "_APS_NEXT_COMMAND_VALUE" || name == "_APS_NEXT_CONTROL_VALUE" || name == "_APS_NEXT_SYMED_VALUE";
                Entries.Add(new ResourceHeaderEntry(match.Groups[1].Value, value, i, special));
            }
        }

        private int FindLine(int newId)
        {
            var lineNumber = -1;
            foreach (var entry in Entries)
            {
                if (entry.Special)
                    continue;
                if (newId < entry.Value)
                    return entry.LineNumber;
                lineNumber = entry.LineNumber + 1;
            }
            return lineNumber < 0 ? 4 : lineNumber;
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

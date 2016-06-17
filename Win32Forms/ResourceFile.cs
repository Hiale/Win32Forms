using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
// ReSharper disable AssignNullToNotNullAttribute

namespace Hiale.Win32Forms
{
    public class ResourceFile
    {
        public string FileName { get; }

        public bool Replace { get; }

        private ResourceHeaderFile _resourceHeaderFile;
        private string _fileContent;
        private readonly bool _embedded;
        private bool _oldDialogFound;
        private readonly HashSet<string> _replaceOldControlIds; 

        public ResourceFile(string fileName, bool replace, bool embedded = false)
        {
            FileName = fileName;
            Replace = replace;
            _embedded = embedded;
            if (embedded)
                ReadEmbedded();
            else
                Read();
            _replaceOldControlIds = new HashSet<string>();
        }

        public bool IsValid()
        {
            return _resourceHeaderFile != null && _resourceHeaderFile.IsValid();
        }

        private void Read()
        {
            _fileContent = File.ReadAllText(FileName, Extension.GetEncoding(FileName));
            FindHeaderFile();
        }

        private void ReadEmbedded()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetType().Namespace + ".Template.Resource.rc";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream, Encoding.Unicode))
            {
                _fileContent = reader.ReadToEnd();
            }
            const string defaultResourceHeaderFileName = "resource{0}.h";

            var resourceHeaderFileName = Path.Combine(Path.GetDirectoryName(FileName), string.Format(defaultResourceHeaderFileName, string.Empty));
            if (File.Exists(resourceHeaderFileName))
            {
                int index = 0;
                while (true)
                {
                    index++;
                    resourceHeaderFileName = Path.Combine(Path.GetDirectoryName(FileName), string.Format(defaultResourceHeaderFileName, index));
                    if (File.Exists(resourceHeaderFileName))
                        continue;
                    _fileContent = _fileContent.Replace(string.Format(defaultResourceHeaderFileName, string.Empty), string.Format(defaultResourceHeaderFileName, index));
                    break;
                }
            }
            _resourceHeaderFile = new ResourceHeaderFile(resourceHeaderFileName, this, true);
        }

        private void Write()
        {
            File.WriteAllText(FileName, _fileContent, Encoding.Unicode);
        }

        public static string CreateBackup(string fileName)
        {
            var backupFile = Path.Combine(Path.GetDirectoryName(fileName), $"{Path.GetFileNameWithoutExtension(fileName)} (Win32Forms Backup {DateTime.Now.ToString("yyyyMMddTHHmmss")}){Path.GetExtension(fileName)}");
            File.Copy(fileName, backupFile);
            return backupFile;
        }

        public void Patch(ConvertResult result)
        {
            if (Replace && _oldDialogFound)
            {
                SimpleLogger.GetLogger().WriteLog("Dialog replaced");
                PatchReplace(result);
                return;
            }
            //patch header file
            _resourceHeaderFile.AddResource(result.NewResourceValue);
            foreach (var newControlValue in result.NewControlValues)
            {
                var controlId = _resourceHeaderFile.AddControl(newControlValue.ControlId);
                SimpleLogger.GetLogger().WriteLog($"Added control '{newControlValue.ControlId}' with id {controlId}");
            }
            if (!_embedded)
            {
                var backupFileHeader = CreateBackup(_resourceHeaderFile.FileName);
                SimpleLogger.GetLogger().WriteLog($"Created Backup file '{backupFileHeader}'");
            }
            _resourceHeaderFile.Write();

            //patch resource file
            var index = FindInserIndex();
            if (index > -1)
            {
                _fileContent = _fileContent.Insert(index, Environment.NewLine + Environment.NewLine + result.DialogContent);
            }
            else
            {
                _fileContent = CreateNewDialogSection(result.DialogContent);
            }
            if (!_embedded)
            {
                var backupFile = CreateBackup(FileName);
                SimpleLogger.GetLogger().WriteLog($"Created Backup file '{backupFile}'");
            }
            Write();
        }

        private void PatchReplace(ConvertResult result)
        {
            //patch header file
            var resourceHaderChanged = false;
            foreach (var newControlValue in result.NewControlValues)
            {
                if (_replaceOldControlIds.Contains(newControlValue.ControlId))
                {
                    _replaceOldControlIds.Remove(newControlValue.ControlId);
                    continue;
                }
                var controlId = _resourceHeaderFile.AddControl(newControlValue.ControlId);
                SimpleLogger.GetLogger().WriteLog($"Added control '{newControlValue.ControlId}' with id {controlId}");
                resourceHaderChanged = true;
            }
            foreach (var removedOldControlId in _replaceOldControlIds)
            {
                _resourceHeaderFile.RemoveEntry(removedOldControlId);
                SimpleLogger.GetLogger().WriteLog($"Removed control '{removedOldControlId}'");
                resourceHaderChanged = true;
            }

            if (resourceHaderChanged)
            {
                var backupFileHeader = CreateBackup(_resourceHeaderFile.FileName);
                SimpleLogger.GetLogger().WriteLog($"Created Backup file '{backupFileHeader}'");
                _resourceHeaderFile.Write();
            }

            //patch resource file
            ReplaceExistingDialog(result);

            var backupFile = CreateBackup(FileName);
            SimpleLogger.GetLogger().WriteLog($"Created Backup file '{backupFile}'");
            Write();
        }

        public bool IsIdAvailable(string id, bool isForm)
        {
            if (Replace)
            {
                if (isForm)
                {
                    FindDialogControls(id);
                    return true;
                }
                if (_replaceOldControlIds.Contains(id))
                    return true;
            }
            return _resourceHeaderFile.Entries.All(resourceHeaderEntry => resourceHeaderEntry.Name != id);
        }

        private string CreateNewDialogSection(string dialogContent)
        {
            var stringBuilder = new StringBuilder(_fileContent);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(new string('/', 77));
            stringBuilder.AppendLine("//");
            stringBuilder.AppendLine("// Dialog");
            stringBuilder.AppendLine("//");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(dialogContent);
            return stringBuilder.ToString();
        }

        private int FindInserIndex()
        {
            var regEx = new Regex(@"\S+ DIALOGEX.*?^BEGIN.*?^END", RegexOptions.Multiline | RegexOptions.Singleline);
            var matches = regEx.Matches(_fileContent);
            if (matches.Count < 1)
                return -1;
            var lastMatch = matches[matches.Count - 1];
            return lastMatch.Index + lastMatch.Length;
        }

        private Regex FindExistingDialog(string id)
        {
            var regEx = new Regex($"{id} DIALOGEX.*?^BEGIN(.*?)^END", RegexOptions.Multiline | RegexOptions.Singleline);
            var matches = regEx.Matches(_fileContent);
            return matches.Count == 1 ? regEx : null;
        }

        private void ReplaceExistingDialog(ConvertResult result)
        {
            var regEx = FindExistingDialog(result.NewResourceValue);
            if (regEx != null)
            {
                _fileContent = regEx.Replace(_fileContent, result.DialogContent);
            }
        }

        private void FindDialogControls(string id)
        {
            var dialogRegEx = FindExistingDialog(id);
            if (dialogRegEx == null)
                return;
            _oldDialogFound = true;
            var controls = dialogRegEx.Match(_fileContent).Groups[1].Value;
            if (string.IsNullOrEmpty(controls))
                return;
            var lines = controls.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var controlId = DialogControl.ParseId(line);
                if (string.IsNullOrEmpty(controlId))
                    continue;
                _replaceOldControlIds.Add(controlId);
            }
        }

        private void FindHeaderFile()
        {
            var basePath = Path.GetDirectoryName(FileName);
            var regEx = new Regex(@"#include +""(.*)""");
            var matches = regEx.Matches(_fileContent);
            foreach (Match match in matches)
            {
                var headerFile = Path.Combine(basePath, match.Groups[1].Value);
                if (!File.Exists(headerFile))
                    continue;
                var resourceHeaderFile = new ResourceHeaderFile(headerFile, this);
                if (!resourceHeaderFile.IsValid())
                    continue;
                _resourceHeaderFile = resourceHeaderFile;
                return;
            }
        }
    }
}

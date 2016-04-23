using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Hiale.Win32Forms
{
    public class ResourceFile
    {
        public string FileName { get; }

        private ResourceHeaderFile _resourceHeaderFile;
        private string _fileContent;

        public ResourceFile(string fileName)
        {
            FileName = fileName;
            Read();
        }

        public bool IsValid()
        {
            return _resourceHeaderFile != null;
        }

        private void Read()
        {
            _fileContent = File.ReadAllText(@FileName, Encoding.UTF8);
            FindHeaderFile();
        }

        private void Write()
        {
            File.WriteAllText(FileName, _fileContent);
        }

        public static void CreateBackup(string fileName)
        {
            var backupFile = Path.Combine(Path.GetDirectoryName(fileName), $"{Path.GetFileNameWithoutExtension(fileName)} (Win32Forms Backup {DateTime.Now.ToString("yyyyMMddTHHmmss")}){Path.GetExtension(fileName)}");
            File.Copy(fileName, backupFile);
        }

        public void CreateNew()
        {
            throw new NotImplementedException();
        }

        public void Patch(ConvertResult result)
        {
            //patch header file
            _resourceHeaderFile.AddResource(result.NewResourceValue);
            foreach (var newControlValue in result.NewControlValues)
            {
                _resourceHeaderFile.AddControl(newControlValue);
            }
            CreateBackup(_resourceHeaderFile.FileName);
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
            CreateBackup(FileName);
            Write();
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
                var resourceHeaderFile = new ResourceHeaderFile(headerFile);
                if (!resourceHeaderFile.IsValid())
                    continue;
                _resourceHeaderFile = resourceHeaderFile;
                return;
            }
        }
    }
}

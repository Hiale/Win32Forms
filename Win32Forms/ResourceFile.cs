using System;
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

        private ResourceHeaderFile _resourceHeaderFile;
        private string _fileContent;
        private readonly bool _embedded;

        public ResourceFile(string fileName, bool embedded = false)
        {
            FileName = fileName;
            _embedded = embedded;
            if (embedded)
                ReadEmbedded();
            else
                Read();
        }

        public bool IsValid()
        {
            return _resourceHeaderFile != null && _resourceHeaderFile.IsValid();
        }

        private void Read()
        {
            _fileContent = File.ReadAllText(FileName, Encoding.UTF8);
            FindHeaderFile();
        }

        private void ReadEmbedded()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetType().Namespace + ".Template.Resource.rc";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
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
            _resourceHeaderFile = new ResourceHeaderFile(resourceHeaderFileName, true);
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

        public void Patch(ConvertResult result)
        {
            //patch header file
            _resourceHeaderFile.AddResource(result.NewResourceValue);
            foreach (var newControlValue in result.NewControlValues)
            {
                _resourceHeaderFile.AddControl(newControlValue);
            }
            if (!_embedded)
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
            if (!_embedded)
                CreateBackup(FileName);
            Write();
        }

        public bool IsIdAvailable(string id)
        {
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

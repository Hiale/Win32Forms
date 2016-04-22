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

        public void CreateNew()
        {
            throw new NotImplementedException();
        }

        public void Patch(ConvertResult result)
        {
            _resourceHeaderFile.AddResource(result.NewResourceValue);
            foreach (var newControlValue in result.NewControlValues)
            {
                _resourceHeaderFile.AddControl(newControlValue);
            }
            _resourceHeaderFile.Write();
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

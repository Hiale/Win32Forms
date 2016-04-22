using System.Collections.Generic;

namespace Hiale.Win32Forms
{
    public class ConvertResult
    {
        public string DialogContent { get; set; }

        public string NewResourceValue { get; set; }

        public List<string> NewControlValues { get; private set; }

        public ConvertResult()
        {
            NewControlValues = new List<string>();
        }
    }
}

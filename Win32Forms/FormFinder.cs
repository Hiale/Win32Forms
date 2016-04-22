using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Hiale.Win32Forms
{
    public static class FormFinder
    {
        public static IEnumerable<FormResult> FindForms(Assembly assembly)
        {
            return assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Form))).Select(type => new FormResult(type));
        }
    }
}

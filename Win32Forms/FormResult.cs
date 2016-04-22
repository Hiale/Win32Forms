using System;

namespace Hiale.Win32Forms
{
    public class FormResult
    {
        public Type Type { get; }

        public FormResult(Type type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return $"{Type.Name} ({Type.FullName})";
        }
    }
}

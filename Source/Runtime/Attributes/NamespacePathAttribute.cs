using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NamespacePathAttribute : CursoriamAttribute
    {
        public NamespacePathAttribute(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}
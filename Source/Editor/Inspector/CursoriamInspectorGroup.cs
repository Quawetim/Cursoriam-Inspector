using System.Collections.Generic;

namespace BadPractice.Cursoriam.Inspector.Editor
{
    public sealed class CursoriamInspectorGroup
    {
        public CursoriamInspectorGroup(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public List<CursoriamInspectorElement> Elements { get; } = new();
    }
}
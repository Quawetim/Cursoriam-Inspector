using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace BadPractice.Cursoriam.Inspector.Editor
{
    public class CursoriamInspectorField : CursoriamInspectorElement
    {
        public CursoriamInspectorField(MemberInfo memberInfo, object target, SerializedProperty property, int indentLevel = 0,
            CursoriamInspectorElement parent = null) : base(memberInfo, target)
        {
            Property = property;
            IndentLevel = indentLevel;
            Parent = parent;
        }

        public FieldInfo Field { get; }

        public SerializedProperty Property { get; }

        public int IndentLevel { get; }

        public CursoriamInspectorElement Parent { get; }

        public List<CursoriamInspectorElement> Children { get; } = new();
    }
}
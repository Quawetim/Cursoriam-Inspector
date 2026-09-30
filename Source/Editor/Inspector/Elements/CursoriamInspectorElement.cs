using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace BadPractice.Cursoriam.Inspector.Editor
{
    public class CursoriamInspectorElement
    {
        public CursoriamInspectorElement(MemberInfo memberInfo, object target)
        {
            Member = memberInfo;
            Target = target;
            Name = memberInfo.Name;

            DisplayName = ObjectNames.NicifyVariableName(memberInfo.Name);

            var customAttributes = memberInfo.GetCustomAttributes(typeof(Attribute), true) as Attribute[] ?? Array.Empty<Attribute>();
            Attributes.AddRange(customAttributes);
        }

        public MemberInfo Member { get; }

        public object Target { get; }

        public string Name { get; }

        public string DisplayName { get; set; }

        public List<Attribute> Attributes { get; } = new();

        public bool IsVisible { get; set; } = true;

        public bool IsEnabled { get; set; } = true;

        public int Order { get; set; }
    }
}
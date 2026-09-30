using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace BadPractice.Cursoriam.Inspector.Editor
{
    public sealed class CursoriamInspectorCollection : CursoriamInspectorField
    {
        // property path to field
        private readonly Dictionary<string, CursoriamInspectorField> _elements = new();

        public CursoriamInspectorCollection(MemberInfo memberInfo, object target, SerializedProperty property, Type elementType,
            int indentLevel = 0, CursoriamInspectorElement parent = null)
            : base(memberInfo, target, property, indentLevel, parent)
        {
            ElementType = elementType;
        }

        public CursoriamInspectorField GetElement(string propertyPath, SerializedProperty itemProperty, object itemTarget)
        {
            if (string.IsNullOrEmpty(propertyPath))
                return null;

            if (_elements.TryGetValue(propertyPath, out var rowField))
            {
                if (ReferenceEquals(rowField.Target, itemTarget))
                    return rowField;

                Clear(propertyPath);
            }

            var index = GetPropertyArrayIndex(propertyPath);
            var displayName = itemTarget != null
                ? ObjectNames.NicifyVariableName(itemTarget.GetType().Name)
                : $"Element {index}";

            rowField = new(Member, itemTarget, itemProperty, IndentLevel + 1, this)
            {
                DisplayName = displayName
            };

            if (itemTarget != null)
            {
                CursoriamInspectorEditor.CollectFields(itemProperty.serializedObject, rowField.Children, itemTarget.GetType(),
                    itemTarget, itemProperty.propertyPath, IndentLevel + 1, 0, rowField);
            }

            _elements.Add(propertyPath, rowField);
            return rowField;
        }

        public void Clear(string propertyPath)
        {
            _elements.Remove(propertyPath);
        }

        public void Clear()
        {
            _elements.Clear();
        }

        private int GetPropertyArrayIndex(string propertyPath)
        {
            var openBracket = propertyPath.LastIndexOf('[');
            var closeBracket = propertyPath.LastIndexOf(']');
            if (openBracket >= 0 && closeBracket > openBracket)
            {
                var indexStr = propertyPath.Substring(openBracket + 1, closeBracket - openBracket - 1);
                if (int.TryParse(indexStr, out var index))
                    return index;
            }
            return -1;
        }

        public Type ElementType { get; }
    }
}
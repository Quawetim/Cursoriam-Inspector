using BadPractice.Cursoriam.Extension.Editor;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using BadPractice.Cursoriam.Logger;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(30)]
    internal sealed class MinMaxDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var property = element.Property;
            var minAttr = element.Member.GetCustomAttribute<Attributes.MinAttribute>(true);
            var maxAttr = element.Member.GetCustomAttribute<Attributes.MaxAttribute>(true);

            var propertyField = new PropertyField(property, element.DisplayName);
            propertyField.TrackPropertyValue(property, prop =>
            {
                switch (prop.propertyType)
                {
                    case SerializedPropertyType.Integer:
                    {
                        var val = prop.intValue;
                        var originalVal = val;

                        if (minAttr != null)
                        {
                            val = Mathf.Max(val, (int)minAttr.MinValue);
                        }

                        if (maxAttr != null)
                        {
                            val = Mathf.Min(val, (int)maxAttr.MaxValue);
                        }

                        if (val != originalVal)
                        {
                            prop.TrySetValue(val);
                        }

                        break;
                    }
                    case SerializedPropertyType.Float:
                    {
                        var val = prop.floatValue;
                        var originalVal = val;

                        if (minAttr != null)
                        {
                            val = Mathf.Max(val, minAttr.MinValue);
                        }

                        if (maxAttr != null)
                        {
                            val = Mathf.Min(val, maxAttr.MaxValue);
                        }

                        if (!Mathf.Approximately(val, originalVal))
                        {
                            prop.TrySetValue(val);
                        }

                        break;
                    }
                    default:
                    {
                        CursoriamLogger.Error($"Атрибуты [Min]/[Max] не поддерживаются для типа полей ({prop.propertyType})! (Поле: {element.Name})");
                        break;
                    }
                }
            });

            return propertyField;
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            if (element.Property == null)
                return false;

            var hasMin = element.Member.IsDefined(typeof(Attributes.MinAttribute), true);
            var hasMax = element.Member.IsDefined(typeof(Attributes.MaxAttribute), true);

            return base.CanDrawInternal(element) && (hasMin || hasMax);
        }
    }
}
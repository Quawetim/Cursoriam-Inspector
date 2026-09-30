using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(90)]
    internal sealed class NonSerializedFieldDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var textField = new TextField(element.DisplayName);
            textField.SetEnabled(false);

            string GetCurrentValueString()
            {
                var valueStr = "null";
                try
                {
                    var val = element.Member switch
                    {
                        FieldInfo field => field.GetValue(element.Target),
                        PropertyInfo property => property.GetValue(element.Target, null),
                        _ => null
                    };

                    if (val != null)
                    {
                        valueStr = val.ToString();
                    }
                }
                catch (Exception e)
                {
                    valueStr = $"Exception: {e.GetType().Name}";
                }

                return valueStr;
            }

            textField.value = GetCurrentValueString();
            textField.schedule.Execute(() =>
            {
                var nextValue = GetCurrentValueString();
                if (textField.value != nextValue)
                {
                    textField.value = nextValue;
                }
            }).Every(500);

            return textField;
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element.Property == null &&
                   element.Member.IsDefined(typeof(ShowNonSerializedFieldAttribute), true);
        }
    }
}
using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using BadPractice.Cursoriam.Inspector.Enum;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(100)]
    internal sealed class ButtonDrawer : CursoriamInspectorDrawer<CursoriamInspectorMethod>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorMethod element)
        {
            var buttonAttr = element.Member.GetCustomAttribute<ButtonAttribute>(true);

            var label = string.IsNullOrWhiteSpace(buttonAttr?.Text)
                ? ObjectNames.NicifyVariableName(element.Name)
                : buttonAttr.Text;

            var button = new Button()
                .SetName(element.Name)
                .SetText(label);

            var shouldEnable = buttonAttr == null || IsEnabled(buttonAttr.EnableMode);
            button.SetEnabled(shouldEnable);

            if (buttonAttr != null)
            {
                if (buttonAttr.IsCustomColors)
                {
                    button.SetBackColor(buttonAttr.CustomBackgroundColor);
                    button.SetColor(buttonAttr.CustomTextColor);
                }
                else
                {
                    if (buttonAttr.PresetBackgroundColor != EInspectorColor.Default)
                    {
                        button.SetBackColor(buttonAttr.PresetBackgroundColor.ToColor());
                    }

                    if (buttonAttr.PresetTextColor != EInspectorColor.Default)
                    {
                        button.SetColor(buttonAttr.PresetTextColor.ToColor());
                    }
                }

                button.SetFontStyle(FontStyle.Bold);
            }

            button.SetAction(() =>
            {
                if (buttonAttr != null && !IsEnabled(buttonAttr.EnableMode))
                    return;

                var targetObject = (Object)element.Target;
                Undo.RecordObject(targetObject, $"Invoke {element.Name}");

                element.Method.Invoke(element.Target, null);

                if (targetObject)
                {
                    EditorUtility.SetDirty(targetObject);
                }
            });

            return button;
        }

        protected override bool CanDrawInternal(CursoriamInspectorMethod element)
        {
            return base.CanDrawInternal(element) &&
                   element.Method != null;
        }

        private bool IsEnabled(EButtonEnableMode mode)
        {
            return mode switch
            {
                EButtonEnableMode.Always => true,
                EButtonEnableMode.Editor => !Application.isPlaying,
                EButtonEnableMode.Playmode => Application.isPlaying,
                _ => true
            };
        }
    }
}
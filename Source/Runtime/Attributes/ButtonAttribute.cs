using BadPractice.Cursoriam.Inspector.Enum;
using System;
using UnityEngine;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ButtonAttribute : CursoriamAttribute
    {
        public ButtonAttribute(string text = null, EButtonEnableMode enableMode = EButtonEnableMode.Always,
            EInspectorColor backgroundColor = EInspectorColor.Default, EInspectorColor textColor = EInspectorColor.Default)
        {
            Text = text;
            EnableMode = enableMode;
            PresetBackgroundColor = backgroundColor;
            PresetTextColor = textColor;
            IsCustomColors = false;
        }

        public ButtonAttribute(string text, float bgR, float bgG, float bgB,
            float textR = 1f, float textG = 1f, float textB = 1f,
            EButtonEnableMode enableMode = EButtonEnableMode.Always)
        {
            Text = text;
            EnableMode = enableMode;
            CustomBackgroundColor = new(bgR, bgG, bgB, 1f);
            CustomTextColor = new(textR, textG, textB, 1f);
            IsCustomColors = true;
        }

        public bool IsCustomColors { get; }

        public string Text { get; }

        public EButtonEnableMode EnableMode { get; }

        public EInspectorColor PresetBackgroundColor { get; } = EInspectorColor.Default;

        public EInspectorColor PresetTextColor { get; } = EInspectorColor.Default;

        public Color CustomBackgroundColor { get; } = Color.clear;

        public Color CustomTextColor { get; } = Color.clear;
    }
}
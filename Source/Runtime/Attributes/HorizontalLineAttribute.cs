using BadPractice.Cursoriam.Inspector.Enum;
using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class HorizontalLineAttribute : CursoriamAttribute
    {
        public HorizontalLineAttribute(float height = 1f, float marginTop = 8f, float marginBottom = 8f,
            EInspectorColor colorPreset = EInspectorColor.Gray)
        {
            Height = height;
            MarginTop = marginTop;
            MarginBottom = marginBottom;
            ColorPreset = colorPreset;
        }

        public float Height { get; }

        public float MarginTop { get; }

        public float MarginBottom { get; }

        public EInspectorColor ColorPreset { get; }
    }
}
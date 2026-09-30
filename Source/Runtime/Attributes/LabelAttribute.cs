using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class LabelAttribute : CursoriamAttribute
    {
        public LabelAttribute(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}
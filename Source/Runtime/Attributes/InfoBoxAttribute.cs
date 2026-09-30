using BadPractice.Cursoriam.Inspector.Enum;
using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class InfoBoxAttribute : CursoriamAttribute
    {
        public InfoBoxAttribute(string title, string text, EInfoBoxType type = EInfoBoxType.Info)
        {
            Title = title;
            Text = text;
            Type = type;
        }

        public string Title { get; }

        public string Text { get; }

        public EInfoBoxType Type { get; }
    }
}
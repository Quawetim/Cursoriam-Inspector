using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MinAttribute : CursoriamAttribute
    {
        public MinAttribute(float minValue)
        {
            MinValue = minValue;
        }

        public MinAttribute(int minValue)
        {
            MinValue = minValue;
        }

        public float MinValue { get; }
    }
}
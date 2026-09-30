using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MaxAttribute : CursoriamAttribute
    {
        public MaxAttribute(float maxValue)
        {
            MaxValue = maxValue;
        }

        public MaxAttribute(int maxValue)
        {
            MaxValue = maxValue;
        }

        public float MaxValue { get; }
    }
}
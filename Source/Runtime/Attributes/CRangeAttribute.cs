using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CRangeAttribute : CursoriamAttribute
    {
        public CRangeAttribute(float min, float max, float step = 0f,
            string axisX = "X", string axisY = "Y", string axisZ = "Z")
        {
            Min = min;
            Max = max;
            Step = step;
            AxisNames = CheckAxisNames(axisX, axisY, axisZ);
        }

        public CRangeAttribute(int min, int max, int step = 0,
            string axisX = "X", string axisY = "Y", string axisZ = "Z")
        {
            Min = min;
            Max = max;
            Step = step;
            AxisNames = CheckAxisNames(axisX, axisY, axisZ);
        }

        private string[] CheckAxisNames(string axisX, string axisY, string axisZ)
        {
            axisX ??= string.Empty;
            axisY ??= string.Empty;
            axisZ ??= string.Empty;

            return new[] { axisX, axisY, axisZ };
        }

        public float Min { get; }

        public float Max { get; }

        public float Step { get; }

        public string[] AxisNames { get; }
    }
}
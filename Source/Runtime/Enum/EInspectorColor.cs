using UnityEngine;

namespace BadPractice.Cursoriam.Inspector.Enum
{
    public enum EInspectorColor
    {
        Default, White, Black,
        Red, Green, Blue,
        Yellow, Gray, Purple
    }

    public static class InspectorColorExtensions
    {
        public static Color ToColor(this EInspectorColor color)
        {
            return color switch
            {
                EInspectorColor.White => Color.white,
                EInspectorColor.Black => Color.black,
                EInspectorColor.Red => new(1f, 0.35f, 0.35f, 1f),
                EInspectorColor.Green => new(0.35f, 1f, 0.45f, 1f),
                EInspectorColor.Blue => new(0.35f, 0.65f, 1f, 1f),
                EInspectorColor.Yellow => new(1f, 0.85f, 0.35f, 1f),
                EInspectorColor.Gray => new(0.6f, 0.6f, 0.6f, 1f),
                EInspectorColor.Purple => new(0.5f, 0.35f, 0.65f, 1f),
                _ => Color.white
            };
        }
    }
}
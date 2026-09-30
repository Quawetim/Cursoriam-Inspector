using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ShowAssetPreviewAttribute : CursoriamAttribute
    {
        public ShowAssetPreviewAttribute(int width = 64, int height = 64)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }
    }
}
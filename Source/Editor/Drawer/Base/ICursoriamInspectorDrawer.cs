using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    internal interface ICursoriamInspectorDrawer
    {
        public VisualElement CreateVisualElement(CursoriamInspectorElement element);

        public bool CanDraw(CursoriamInspectorElement element);

        public bool IsDecorator { get; }
    }
}
using UnityEngine;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    internal abstract class CursoriamInspectorDrawer<TElement> : ICursoriamInspectorDrawer
        where TElement : CursoriamInspectorElement
    {
        protected readonly Color COLOR_BTN_GREEN = new(0.15f, 0.6f, 0.3f);
        protected readonly Color COLOR_BTN_RED = new(0.75f, 0.2f, 0.2f);
        protected readonly Color COLOR_BTN_RED_DARK = new(0.4f, 0.15f, 0.15f);
        protected readonly Color COLOR_BTN_BLUE = new(0.15f, 0.45f, 0.6f);
        protected readonly Color COLOR_BTN_PURPLE = new(0.5f, 0.35f, 0.65f);
        protected readonly Color COLOR_BTN_PURPLE_DARK = new(0.25f, 0.18f, 0.32f);
        protected readonly Color COLOR_BTN_CYAN = new(0.1f, 0.55f, 0.55f);

        public VisualElement CreateVisualElement(CursoriamInspectorElement element)
        {
            var visualElement = element is TElement concrete
                ? CreateVisualElementInternal(concrete)
                : null;

            return visualElement;
        }

        public bool CanDraw(CursoriamInspectorElement element)
        {
            return element is TElement concrete && CanDrawInternal(concrete);
        }

        protected abstract VisualElement CreateVisualElementInternal(TElement element);

        protected virtual bool CanDrawInternal(TElement element) => true;

        public virtual bool IsDecorator => false;
    }
}
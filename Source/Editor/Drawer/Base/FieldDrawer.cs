using BadPractice.Cursoriam.Inspector.Editor.Logic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(100)]
    internal sealed class FieldDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            return new PropertyField(element.Property);
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element.Property != null;
        }
    }
}
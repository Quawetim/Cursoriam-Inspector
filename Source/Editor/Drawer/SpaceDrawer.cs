using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(-100)]
    internal sealed class SpaceDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var spaceAttr = (SpaceAttribute)System.Attribute.GetCustomAttribute(element.Member, typeof(SpaceAttribute), true);

            var height = spaceAttr is { height: > 0 }
                ? spaceAttr.height
                : 8f;

            var space = new VisualElement()
                .SetHeight(height);

            return space;
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element is CursoriamInspectorCollection &&
                   element.Member != null &&
                   element.Member.IsDefined(typeof(SpaceAttribute), true);
        }

        public override bool IsDecorator => true;
    }
}
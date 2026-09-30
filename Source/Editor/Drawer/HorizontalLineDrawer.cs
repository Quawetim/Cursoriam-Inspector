using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using BadPractice.Cursoriam.Inspector.Enum;
using System.Reflection;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(2)]
    internal sealed class HorizontalLineDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var attributes = element.Member.GetCustomAttributes<HorizontalLineAttribute>(true);

            var container = new VisualElement();

            foreach (var attr in attributes)
            {
                var color = attr.ColorPreset.ToColor();
                var line = new VisualElement()
                    .SetHeight(attr.Height)
                    .SetMargin(top: attr.MarginTop, bottom: attr.MarginBottom)
                    .SetBackColor(color);
                container.Add(line);
            }

            return container;
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element.Member.IsDefined(typeof(HorizontalLineAttribute), true);
        }

        public override bool IsDecorator => true;
    }
}
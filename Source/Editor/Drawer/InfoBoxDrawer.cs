using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using BadPractice.Cursoriam.Inspector.Enum;
using System.Reflection;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(1)]
    internal sealed class InfoBoxDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var attributes = element.Member.GetCustomAttributes<InfoBoxAttribute>(true);

            var container = new VisualElement();

            foreach (var attr in attributes)
            {
                var messageType = attr.Type switch
                {
                    EInfoBoxType.None => HelpBoxMessageType.None,
                    EInfoBoxType.Info => HelpBoxMessageType.Info,
                    EInfoBoxType.Warning => HelpBoxMessageType.Warning,
                    EInfoBoxType.Error => HelpBoxMessageType.Error,
                    _ => HelpBoxMessageType.Info
                };

                var helpBox = new VisualElement()
                    .AsInfoBox(attr.Title, attr.Text, messageType);
                container.Add(helpBox);
            }

            return container;
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element.Member.IsDefined(typeof(InfoBoxAttribute), true);
        }

        public override bool IsDecorator => true;
    }
}
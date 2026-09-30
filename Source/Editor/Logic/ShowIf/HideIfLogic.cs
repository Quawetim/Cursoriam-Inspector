using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using System.Reflection;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Logic
{
    [LogicPriority(50)]
    internal sealed class HideIfLogic : CursoriamLogic<CursoriamInspectorField>
    {
        private readonly ShowIfLogic _showIfLogic = new();

        protected override void ProcessInternal(CursoriamInspectorField element, VisualElement visualElement)
        {
            if (visualElement == null)
                return;

            var hideIf = element.Member.GetCustomAttribute<HideIfAttribute>(true);
            if (hideIf == null)
                return;

            _showIfLogic.Process(element, visualElement);

            element.IsVisible = !element.IsVisible;
            visualElement.SetDisplayStyle(element.IsVisible ? DisplayStyle.Flex : DisplayStyle.None);
        }
    }
}
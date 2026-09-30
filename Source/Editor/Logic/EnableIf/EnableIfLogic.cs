using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Logic
{
    [LogicPriority(50)]
    internal sealed class EnableIfLogic : CursoriamLogic<CursoriamInspectorField>
    {
        private readonly ShowIfLogic _showIfLogic = new();

        protected override void ProcessInternal(CursoriamInspectorField element, VisualElement visualElement)
        {
            if (visualElement == null)
                return;

            if (!element.Member.IsDefined(typeof(EnableIfAttribute), true))
                return;

            _showIfLogic.Process(element, visualElement);

            element.IsEnabled = element.IsVisible;
            element.IsVisible = true;

            visualElement.SetDisplayStyle(DisplayStyle.Flex);
            visualElement.SetEnabled(element.IsEnabled);
        }
    }
}
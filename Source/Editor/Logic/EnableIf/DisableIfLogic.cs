using BadPractice.Cursoriam.Inspector.Attributes;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Logic
{
    [LogicPriority(50)]
    internal sealed class DisableIfLogic : CursoriamLogic<CursoriamInspectorField>
    {
        private readonly EnableIfLogic _enableIfLogic = new();

        protected override void ProcessInternal(CursoriamInspectorField element, VisualElement visualElement)
        {
            if (visualElement == null)
                return;

            if (!element.Member.IsDefined(typeof(DisableIfAttribute), true))
                return;

            _enableIfLogic.Process(element, visualElement);

            element.IsEnabled = !element.IsEnabled;
            visualElement.SetEnabled(element.IsEnabled);
        }
    }
}
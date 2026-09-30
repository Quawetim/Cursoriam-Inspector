using BadPractice.Cursoriam.Inspector.Attributes;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Logic
{
    [LogicPriority(50)]
    internal sealed class ReadOnlyLogic : CursoriamLogic<CursoriamInspectorField>
    {
        protected override void ProcessInternal(CursoriamInspectorField element, VisualElement visualElement)
        {
            if (visualElement == null)
                return;

            if (!element.Member.IsDefined(typeof(ReadOnlyAttribute), true))
                return;

            element.IsEnabled = false;
            visualElement.SetEnabled(false);
        }
    }
}
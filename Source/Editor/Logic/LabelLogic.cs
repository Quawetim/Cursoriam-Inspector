using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using System.Reflection;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Logic
{
    [LogicPriority(10)]
    internal sealed class LabelLogic : CursoriamLogic<CursoriamInspectorField>
    {
        protected override void ProcessInternal(CursoriamInspectorField element, VisualElement visualElement)
        {
            if (visualElement == null)
                return;

            var labelAttr = element.Member.GetCustomAttribute<LabelAttribute>(true);
            if (labelAttr == null)
                return;

            element.DisplayName = labelAttr.Text;

            switch (visualElement)
            {
                case BindableElement _ when labelAttr.Text != null:
                {
                    var labelComponent = visualElement.Q<Label>();
                    labelComponent?.SetText(labelAttr.Text);
                    break;
                }
                case Label label:
                {
                    label.SetText(labelAttr.Text);
                    break;
                }
            }
        }
    }
}
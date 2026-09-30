using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Logic
{
    public interface ICursoriamLogic
    {
        public void Process(CursoriamInspectorElement element, VisualElement visualElement);
    }
}
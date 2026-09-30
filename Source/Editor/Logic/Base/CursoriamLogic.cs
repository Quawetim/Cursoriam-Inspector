using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Logic
{
    internal abstract class CursoriamLogic<TElement> : ICursoriamLogic
        where TElement : CursoriamInspectorElement
    {
        public void Process(CursoriamInspectorElement element, VisualElement visualElement)
        {
            if (element is not TElement concrete)
                return;

            ProcessInternal(concrete, visualElement);
        }

        protected abstract void ProcessInternal(TElement element, VisualElement visualElement);
    }
}
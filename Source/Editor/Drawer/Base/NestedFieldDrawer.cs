using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(90)]
    internal sealed class NestedFieldDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var foldout = new Foldout()
                .SetText(element.DisplayName)
                .SetValue(element.Property.isExpanded);

            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.target != evt.currentTarget)
                    return;

                if (element.Property.isExpanded == evt.newValue)
                    return;

                element.Property.isExpanded = evt.newValue;
                element.Property.serializedObject.ApplyModifiedProperties();
            });

            foreach (var child in element.Children)
            {
                var childVisual = CursoriamInspectorEditor.CreateVisualElement(child);
                if (childVisual != null)
                {
                    foldout.Add(childVisual);
                }
            }

            return foldout;
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element.Property != null &&
                   element.Children.Count > 0;
        }
    }
}
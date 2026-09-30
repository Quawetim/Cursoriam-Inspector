using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(5)]
    internal sealed class RequiredDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var property = element.Property;
            var attr = element.Member.GetCustomAttribute<RequiredAttribute>(true);

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                var msg = $"Ошибка: Используйте [Required] только со ссылками на объекты! (Поле: {element.Name})";
                return new HelpBox().Setup(msg, HelpBoxMessageType.Error);
            }

            var message = string.IsNullOrWhiteSpace(attr.Message)
                ? $"Поле ({ObjectNames.NicifyVariableName(element.Name)}) обязательно для заполнения!"
                : attr.Message;

            var errorHelpBox = new HelpBox()
                .Setup(message, HelpBoxMessageType.Error);

            void UpdateState()
            {
                var isMissing = property.objectReferenceValue == null;
                errorHelpBox.SetDisplayStyle(isMissing ? DisplayStyle.Flex : DisplayStyle.None);
            }

            UpdateState();

            errorHelpBox.TrackPropertyValue(property, _ => UpdateState());

            return errorHelpBox;
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element.Property != null &&
                   element.Member.IsDefined(typeof(RequiredAttribute), true);
        }

        public override bool IsDecorator => true;
    }
}
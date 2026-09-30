using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using BadPractice.Cursoriam.Logger;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(75)]
    internal sealed class RequireInterfaceDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var property = element.Property;

            var attribute = element.Member.GetCustomAttribute<RequireInterfaceAttribute>(true);
            if (attribute == null && element.Parent is CursoriamInspectorField parentField)
            {
                attribute = parentField.Member.GetCustomAttribute<RequireInterfaceAttribute>(true);
            }

            if (attribute == null)
                return new PropertyField(property);

            var requiredInterface = attribute.InterfaceType;

            var container = new VisualElement();
            var errorHelpBox = new HelpBox().SetDisplayStyle(DisplayStyle.None);
            container.Add(errorHelpBox);

            void ShowError(string message)
            {
                CursoriamLogger.Error(message);
                errorHelpBox
                    .Setup(message, HelpBoxMessageType.Error)
                    .SetDisplayStyle(DisplayStyle.Flex);
            }

            void HideError()
            {
                errorHelpBox.SetDisplayStyle(DisplayStyle.None);
            }

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                var singleField = BuildObjectField(property, element.DisplayName, requiredInterface, ShowError, HideError);
                container.Add(singleField);
                return container;
            }

            var errorMsg = $"Ошибка: Используйте [RequireInterface] только со ссылками на объекты (Поле: {element.Name})";
            return new HelpBox().Setup(errorMsg, HelpBoxMessageType.Error);
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            if (!base.CanDrawInternal(element) || element.Property == null)
                return false;

            if (element.Property.isArray && element.Property.propertyType != SerializedPropertyType.String)
                return false;

            if (element.Member.IsDefined(typeof(RequireInterfaceAttribute), true))
                return true;

            if (element.Parent is CursoriamInspectorField parentField)
                return parentField.Member.IsDefined(typeof(RequireInterfaceAttribute), true);

            return false;
        }

        private ObjectField BuildObjectField(SerializedProperty property, string displayName, Type requiredInterface,
            Action<string> onError, Action onSuccess)
        {
            var objectField = new ObjectField(displayName)
            {
                objectType = typeof(UnityEngine.Object),
                allowSceneObjects = true,
            }
            .SetValue(property.objectReferenceValue)
            .SetFlexGrow(1);

            objectField.RegisterValueChangedCallback(evt =>
            {
                var assignedObject = evt.newValue;

                if (!assignedObject)
                {
                    property.objectReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                    onSuccess?.Invoke();
                    return;
                }

                var targetGo = assignedObject switch
                {
                    GameObject go => go,
                    Component comp => comp.gameObject,
                    _ => null
                };

                if (targetGo && targetGo.GetComponent(requiredInterface))
                {
                    var component = targetGo.GetComponent(requiredInterface);
                    property.objectReferenceValue = component.gameObject;
                    onSuccess?.Invoke();
                }
                else
                {
                    onError?.Invoke($"Объект ({assignedObject.name}) не реализует интерфейс ({requiredInterface.Name})!");
                    property.objectReferenceValue = null;
                    objectField.SetValueWithoutNotify(null);
                }

                property.serializedObject.ApplyModifiedProperties();
            });

            objectField.TrackPropertyValue(property, prop =>
            {
                objectField.SetValueWithoutNotify(prop.objectReferenceValue);
            });

            return objectField;
        }
    }
}
using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(50)]
    internal sealed class ReferenceSelectorDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        private const string UNITY_CLASS_BASE_FIELD_INPUT = "unity-base-field__input";
        private const string UNITY_CLASS_FOLDOUT_TOGGLE = "unity-foldout__toggle";

        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var rowRoot = new VisualElement();

            RefreshUI(rowRoot, element);

            rowRoot.TrackPropertyValue(element.Property, _ => RefreshUI(rowRoot, element));

            return rowRoot;
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element.Member != null &&
                   element.Member.IsDefined(typeof(ReferenceSelectorAttribute), true) &&
                   element.Property != null &&
                   element.Property.propertyType == SerializedPropertyType.ManagedReference;
        }

        private void RefreshUI(VisualElement root, CursoriamInspectorField element)
        {
            root.Unbind();
            root.Clear();

            element.Property.serializedObject.Update();
            var currentObject = element.Property.managedReferenceValue;
            var currentType = currentObject?.GetType();

            var buttonText = currentType != null
                ? ObjectNames.NicifyVariableName(currentType.Name)
                : "Null (Not Assigned)";

            if (currentObject == null)
            {
                DrawNullRow(root, element, buttonText, () => RefreshUI(root, element));
            }
            else
            {
                DrawFoldout(root, element, currentType, currentObject, buttonText, () => RefreshUI(root, element));
            }
        }

        private void DrawNullRow(VisualElement root, CursoriamInspectorField element, string buttonText, Action onTypeChanged)
        {
            var nullRow = new VisualElement()
                .SetFlex(FlexDirection.Row)
                .SetJustifyContent(Justify.SpaceBetween)
                .SetAlignItems(Align.Center);
            root.Add(nullRow);

            nullRow.Add(new Label(element.DisplayName));

            var dropdownButton = new Button(() => OpenSelectorMenu(element, onTypeChanged))
                .SetText(buttonText)
                .SetWidth(250);
            dropdownButton.AddToClassList(UNITY_CLASS_BASE_FIELD_INPUT);
            nullRow.Add(dropdownButton);
        }

        private void DrawFoldout(VisualElement root, CursoriamInspectorField element, Type currentType, object currentObject, string buttonText, Action onTypeChanged)
        {
            var title = GetFoldoutTitle(element, out var titleProperty);

            var foldout = new Foldout()
                .SetText(title);
            foldout.SetValueWithoutNotify(element.Property.isExpanded);
            root.Add(foldout);

            if (titleProperty != null)
            {
                foldout.TrackPropertyValue(titleProperty, _ =>
                {
                    var newTitle = GetFoldoutTitle(element, out var _);
                    foldout.SetText(newTitle);
                });
            }

            var foldoutHeader = foldout.Q<VisualElement>(className: UNITY_CLASS_FOLDOUT_TOGGLE);
            if (foldoutHeader != null)
            {
                var dropdownButton = new Button(() => OpenSelectorMenu(element, onTypeChanged))
                    .SetText(buttonText)
                    .SetWidth(250f)
                    .SetMargin(2f, 4f, 2f, 10f);
                dropdownButton.AddToClassList(UNITY_CLASS_BASE_FIELD_INPUT);
                foldoutHeader.Add(dropdownButton);
            }

            foldout.RegisterValueChangedCallback(evt =>
            {
                if (evt.target != evt.currentTarget)
                    return;

                if (element.Property.isExpanded == evt.newValue)
                    return;

                element.Property.isExpanded = evt.newValue;
                element.Property.serializedObject.ApplyModifiedProperties();
            });

            var fieldsContainer = new VisualElement();
            foldout.Add(fieldsContainer);

            element.Children.Clear();

            CursoriamInspectorEditor.CollectFields(element.Property.serializedObject, element.Children, currentType,
                currentObject, element.Property.propertyPath, 0, 0, element);

            foreach (var child in element.Children)
            {
                var childUi = CursoriamInspectorEditor.CreateVisualElement(child);
                if (childUi != null)
                {
                    fieldsContainer.Add(childUi);
                }
            }

            fieldsContainer.Bind(element.Property.serializedObject);
        }

        private void OpenSelectorMenu(CursoriamInspectorField element, Action onTypeChanged)
        {
            var menu = new GenericMenu();

            var currentObject = element.Property.managedReferenceValue;
            var isNullSelected = currentObject == null;

            menu.AddItem(new("Null"), isNullSelected, () =>
            {
                ApplyNewType(element, null);
                onTypeChanged?.Invoke();
            });

            menu.AddSeparator(string.Empty);

            var baseType = element.Member is FieldInfo field
                ? field.FieldType
                : typeof(object);

            if (element.Parent is CursoriamInspectorCollection collection)
            {
                baseType = collection.ElementType;
            }

            var availableTypes = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.Name);

            foreach (var type in availableTypes)
            {
                var isCurrent = currentObject != null && currentObject.GetType() == type;
                var displayName = ObjectNames.NicifyVariableName(type.Name);

                menu.AddItem(new(displayName), isCurrent, () =>
                {
                    ApplyNewType(element, type);
                    onTypeChanged?.Invoke();
                });
            }

            menu.ShowAsContext();
        }

        private void ApplyNewType(CursoriamInspectorField element, Type newType)
        {
            var property = element.Property;

            Undo.RecordObject(property.serializedObject.targetObject, $"Change Class Type for {element.DisplayName}");

            var propertyValues = CollectPropertyValues(property);

            if (newType == null)
            {
                property.managedReferenceValue = null;
            }
            else
            {
                property.managedReferenceValue = Activator.CreateInstance(newType);
                TryRestorePropertyValues(property, propertyValues);
            }

            property.serializedObject.ApplyModifiedProperties();

            if (element.Parent is CursoriamInspectorCollection collection)
            {
                collection.Clear(property.propertyPath);
            }
        }

        private string GetFoldoutTitle(CursoriamInspectorField element, out SerializedProperty stringProperty)
        {
            stringProperty = null;
            var prop = element.Property;

            if (prop?.managedReferenceValue == null)
                return element.DisplayName;

            var endProperty = prop.GetEndProperty();
            var iterator = prop.Copy();

            if (!iterator.NextVisible(true))
                return element.DisplayName;

            while (!SerializedProperty.EqualContents(iterator, endProperty))
            {
                if (iterator.propertyType == SerializedPropertyType.String)
                {
                    stringProperty = iterator.Copy();

                    if (!string.IsNullOrEmpty(iterator.stringValue))
                        return iterator.stringValue;

                    return $"{ObjectNames.NicifyVariableName(prop.managedReferenceValue.GetType().Name)}";
                }

                if (!iterator.NextVisible(false))
                    break;
            }

            return element.DisplayName;
        }

        private Dictionary<string, object> CollectPropertyValues(SerializedProperty prop)
        {
            var values = new Dictionary<string, object>();
            if (prop.managedReferenceValue == null)
                return values;

            var endProperty = prop.GetEndProperty();
            var iterator = prop.Copy();

            if (!iterator.NextVisible(true))
                return values;

            while (!SerializedProperty.EqualContents(iterator, endProperty))
            {
                var relativePath = iterator.propertyPath[(prop.propertyPath.Length + 1)..];

                values[relativePath] = iterator.boxedValue;

                if (!iterator.NextVisible(false))
                    break;
            }

            return values;
        }

        private void TryRestorePropertyValues(SerializedProperty prop, Dictionary<string, object> savedValues)
        {
            if (prop.managedReferenceValue == null || savedValues.Count == 0)
                return;

            var endProperty = prop.GetEndProperty();
            var iterator = prop.Copy();

            if (!iterator.NextVisible(true))
                return;

            while (!SerializedProperty.EqualContents(iterator, endProperty))
            {
                var relativePath = iterator.propertyPath[(prop.propertyPath.Length + 1)..];

                if (savedValues.TryGetValue(relativePath, out var oldValue))
                {
                    try
                    {
                        iterator.boxedValue = oldValue;
                    }
                    catch
                    {
                        // ignore
                    }
                }

                if (!iterator.NextVisible(false))
                    break;
            }
        }
    }
}
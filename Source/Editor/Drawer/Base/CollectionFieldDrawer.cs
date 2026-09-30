using BadPractice.Cursoriam.Inspector.Editor.Logic;
using System.Collections;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(80)]
    internal sealed class CollectionFieldDrawer : CursoriamInspectorDrawer<CursoriamInspectorCollection>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorCollection element)
        {
            var listView = new ListView
            {
                showFoldoutHeader = true,
                showAddRemoveFooter = true,
                showBoundCollectionSize = true,
                headerTitle = element.DisplayName,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                itemsSource = new ArrayList(new object[element.Property.arraySize]),
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                showBorder = true,

                makeItem = () => new(),
                bindItem = (visualElement, index) => BindItem(index, element, visualElement),
                style =
                {
                    maxHeight = 520f
                }
            };

            var isUpdating = false;

            listView.onAdd += (view) =>
            {
                isUpdating = true;
                AddElement(view, element);
                isUpdating = false;
            };

            listView.onRemove += (view) =>
            {
                isUpdating = true;
                RemoveElement(view, element);
                isUpdating = false;
            };

            listView.TrackPropertyValue(element.Property, prop =>
            {
                if (isUpdating)
                    return;

                prop.serializedObject.Update();
                UpdateItemsSource(listView, prop.arraySize);
            });

            listView.itemIndexChanged += (oldIndex, newIndex) =>
            {
                isUpdating = true;
                MoveElement(listView, element, oldIndex, newIndex);
                isUpdating = false;
            };

            var sizeField = listView.Q<TextField>();
            sizeField?.RegisterValueChangedCallback(evt =>
            {
                if (isUpdating)
                    return;

                if (!int.TryParse(evt.newValue, out var newSize))
                    return;

                if (newSize < 0)
                {
                    newSize = 0;
                }

                if (element.Property.arraySize == newSize)
                    return;

                isUpdating = true;
                OnSizeChange(listView, element, newSize);
                isUpdating = false;
            });

            var scrollView = listView.Q<ScrollView>();
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;

            return listView;
        }

        protected override bool CanDrawInternal(CursoriamInspectorCollection element)
        {
            return base.CanDrawInternal(element) &&
                   element.Property is { isArray: true };
        }

        private void BindItem(int index, CursoriamInspectorCollection element, VisualElement visualElement)
        {
            visualElement.Unbind();
            visualElement.Clear();

            element.Property.serializedObject.Update();

            if (index >= element.Property.arraySize)
                return;

            var itemProperty = element.Property.GetArrayElementAtIndex(index);
            if (itemProperty == null)
                return;

            var itemTarget = itemProperty.boxedValue;
            var rowField = element.GetElement(itemProperty.propertyPath, itemProperty, itemTarget);

            var rowElement = CursoriamInspectorEditor.CreateVisualElement(rowField);
            if (rowElement != null)
            {
                visualElement.Add(rowElement);
                visualElement.Bind(itemProperty.serializedObject);
            }
        }

        private void AddElement(BaseListView view, CursoriamInspectorCollection element)
        {
            Undo.RecordObject(element.Property.serializedObject.targetObject, "Add Element");

            var newIndex = element.Property.arraySize;
            element.Property.InsertArrayElementAtIndex(newIndex);

            ClearReference(element.Property, newIndex);

            element.Property.serializedObject.ApplyModifiedProperties();
            UpdateItemsSource(view, element.Property.arraySize);
        }

        private void RemoveElement(BaseListView view, CursoriamInspectorCollection element)
        {
            var index = view.selectedIndex;
            if (index < 0 || index >= element.Property.arraySize)
            {
                index = element.Property.arraySize - 1;
            }

            Undo.RecordObject(element.Property.serializedObject.targetObject, "Remove Element");

            element.Property.DeleteArrayElementAtIndex(index);
            element.Property.serializedObject.ApplyModifiedProperties();

            UpdateItemsSource(view, element.Property.arraySize);
        }

        private void MoveElement(BaseListView view, CursoriamInspectorCollection element, int oldIndex, int newIndex)
        {
            Undo.RecordObject(element.Property.serializedObject.targetObject, "Reorder Element");

            element.Property.MoveArrayElement(oldIndex, newIndex);
            element.Property.serializedObject.ApplyModifiedProperties();

            UpdateItemsSource(view, element.Property.arraySize);
        }

        private void UpdateItemsSource(BaseListView view, int size)
        {
            view.itemsSource = new ArrayList(new object[size]);
            view.Rebuild();
        }

        private bool ClearReference(SerializedProperty arrayProperty, int index, bool stopOnExistingObject = false)
        {
            var itemProp = arrayProperty.GetArrayElementAtIndex(index);

            if (itemProp.propertyType != SerializedPropertyType.ManagedReference)
                return false;

            if (itemProp.managedReferenceValue != null)
                return stopOnExistingObject;

            itemProp.managedReferenceValue = null;
            return false;
        }

        private void OnSizeChange(BaseListView view, CursoriamInspectorCollection element, int newSize)
        {
            Undo.RecordObject(element.Property.serializedObject.targetObject, "Change Size");

            element.Property.arraySize = newSize;

            for (var i = newSize - 1; i >= 0; i--)
            {
                if (ClearReference(element.Property, i, stopOnExistingObject: true))
                    break;
            }

            element.Property.serializedObject.ApplyModifiedProperties();
            UpdateItemsSource(view, newSize);
        }
    }
}
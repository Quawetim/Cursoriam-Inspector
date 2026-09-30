using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Extension.Editor;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using BadPractice.Cursoriam.Logger;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(20)]
    internal sealed class CRangeDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var property = element.Property;
            var attr = element.Member.GetCustomAttribute<CRangeAttribute>(true);

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    return CreateIntSlider(property, element.DisplayName, attr);
                }
                case SerializedPropertyType.Float:
                {
                    return CreateFloatSlider(property, element.DisplayName, attr);
                }
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector3Int:
                {
                    return CreateVectorSlider(property, element.DisplayName, attr);
                }
                default:
                {
                    CursoriamLogger.Error($"Невозможно отрисовать ({attr.GetType().Name}), тип ({property.propertyType}) не поддерживается.");
                    return new PropertyField(property);
                }
            }
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element.Property != null &&
                   element.Member.IsDefined(typeof(CRangeAttribute), true);
        }

        private VisualElement CreateIntSlider(SerializedProperty property, string label, CRangeAttribute attr)
        {
            var slider = new SliderInt()
                .Setup(label, (int)attr.Min, (int)attr.Max, property.intValue);

            ConfigureTicks(slider, attr);

            slider.RegisterValueChangedCallback(evt =>
            {
                var newVal = evt.newValue;
                if (attr.Step > 0f)
                {
                    var steps = Mathf.RoundToInt((newVal - attr.Min) / attr.Step);
                    newVal = (int)(attr.Min + (steps * attr.Step));
                    newVal = Mathf.Clamp(newVal, (int)attr.Min, (int)attr.Max);
                    slider.SetValueWithoutNotify(newVal);
                }
                property.intValue = newVal;
                property.serializedObject.ApplyModifiedProperties();
            });

            slider.TrackPropertyValue(property, p => slider.SetValueWithoutNotify(p.intValue));
            return slider;
        }

        private VisualElement CreateFloatSlider(SerializedProperty property, string label, CRangeAttribute attr)
        {
            var slider = new Slider()
                .Setup(label, attr.Min, attr.Max, property.floatValue);

            ConfigureTicks(slider, attr);

            slider.RegisterValueChangedCallback(evt =>
            {
                var newVal = evt.newValue;
                if (attr.Step > 0f)
                {
                    var steps = Mathf.Round((newVal - attr.Min) / attr.Step);
                    newVal = attr.Min + (steps * attr.Step);
                    newVal = Mathf.Clamp(newVal, attr.Min, attr.Max);
                    slider.SetValueWithoutNotify(newVal);
                }
                property.floatValue = newVal;
                property.serializedObject.ApplyModifiedProperties();
            });

            slider.TrackPropertyValue(property, p => slider.SetValueWithoutNotify(p.floatValue));
            return slider;
        }

        private VisualElement CreateVectorSlider(SerializedProperty property, string label, CRangeAttribute attr)
        {
            var panel = new VisualElement()
                .AsBoxPanel(label);

            var componentCount = property.propertyType switch
            {
                SerializedPropertyType.Vector2 => 2,
                SerializedPropertyType.Vector2Int => 2,
                SerializedPropertyType.Vector3 => 3,
                SerializedPropertyType.Vector3Int => 3,
                _ => 0
            };

            var isIntVector = property.propertyType is SerializedPropertyType.Vector2Int or SerializedPropertyType.Vector3Int;

            Vector3 GetCurrentVector() => property.propertyType switch
            {
                SerializedPropertyType.Vector2 => property.vector2Value,
                SerializedPropertyType.Vector2Int => (Vector2)property.vector2IntValue,
                SerializedPropertyType.Vector3 => property.vector3Value,
                _ => property.vector3IntValue
            };

            for (var i = 0; i < componentCount; i++)
            {
                var index = i;
                var axisName = attr.AxisNames[index];
                var currentVector = GetCurrentVector();
                var initialValue = currentVector[index];

                var slider = new Slider()
                    .Setup(axisName, attr.Min, attr.Max, initialValue, true);

                ConfigureTicks(slider, attr);

                slider.RegisterValueChangedCallback(evt =>
                {
                    var newVal = evt.newValue;

                    if (isIntVector || (attr.Step > 0f && Mathf.Approximately(attr.Step % 1f, 0f)))
                    {
                        var newValInt = Mathf.RoundToInt(newVal);
                        if (attr.Step > 0f)
                        {
                            var steps = Mathf.RoundToInt((newValInt - attr.Min) / attr.Step);
                            newValInt = (int)(attr.Min + (steps * attr.Step));
                        }
                        newVal = Mathf.Clamp(newValInt, attr.Min, attr.Max);
                    }
                    else if (attr.Step > 0f)
                    {
                        var steps = Mathf.Round((newVal - attr.Min) / attr.Step);
                        newVal = attr.Min + (steps * attr.Step);
                        newVal = Mathf.Clamp(newVal, attr.Min, attr.Max);
                    }

                    slider.SetValueWithoutNotify(newVal);

                    var v = GetCurrentVector();
                    v[index] = newVal;
                    property.TrySetValue(v);
                });

                slider.TrackPropertyValue(property, p =>
                {
                    var v = GetCurrentVector();
                    slider.SetValueWithoutNotify(v[index]);
                });

                panel.Add(slider);
            }

            return panel;
        }

        private void ConfigureTicks(VisualElement slider, CRangeAttribute attr)
        {
            if (attr.Step <= 0)
                return;

            var range = attr.Max - attr.Min;
            if (range <= 0)
                return;

            var totalSteps = Mathf.FloorToInt(range / attr.Step);
            if (totalSteps is <= 0 or > 40)
                return;

            var ticksContainer = new VisualElement()
                .SetFlex(FlexDirection.Row)
                .SetJustifyContent(Justify.SpaceBetween)
                .SetHeight(4f)
                .SetMargin(left: 130f, right: 55f, bottom: -2f)
                .SetPosition(Position.Absolute);

            var tickColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);

            for (var i = 0; i <= totalSteps; i++)
            {
                var tick = new VisualElement()
                    .SetWidth(1f)
                    .SetHeight(4f)
                    .SetBackColor(tickColor);
                ticksContainer.Add(tick);
            }

            slider.Add(ticksContainer);
        }
    }
}
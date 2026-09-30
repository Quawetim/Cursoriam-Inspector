using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Enum;
using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Logic
{
    [LogicPriority(50)]
    internal sealed class ShowIfLogic : CursoriamLogic<CursoriamInspectorField>
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        protected override void ProcessInternal(CursoriamInspectorField element, VisualElement visualElement)
        {
            if (visualElement == null)
                return;

            var showIf = element.Member.GetCustomAttribute<ShowIfAttribute>(true);
            if (showIf?.Conditions == null || showIf.Conditions.Length == 0)
                return;

            var isAllValid = showIf.Operator == EConditionOperator.And;

            foreach (var conditionName in showIf.Conditions)
            {
                var conditionResult = EvaluateCondition(element.Target, conditionName, showIf.IsMatchValueMode, showIf.TargetValue);

                if (showIf.Operator == EConditionOperator.And)
                {
                    isAllValid &= conditionResult;

                    if (!isAllValid)
                        break;
                }
                else if (showIf.Operator == EConditionOperator.Or)
                {
                    isAllValid |= conditionResult;

                    if (isAllValid)
                        break;
                }
            }

            element.IsVisible = isAllValid;
            visualElement.SetDisplayStyle(isAllValid ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private bool EvaluateCondition(object target, string conditionName, bool isMatchValueMode, object targetValue)
        {
            var targetType = target.GetType();

            if (TryEvaluateField(target, targetType, conditionName, isMatchValueMode, targetValue, out var fieldResult))
                return fieldResult;

            if (TryEvaluateProperty(target, targetType, conditionName, isMatchValueMode, targetValue, out var propResult))
                return propResult;

            if (TryEvaluateMethod(target, targetType, conditionName, isMatchValueMode, targetValue, out var methodResult))
                return methodResult;

            return true;
        }

        private bool TryEvaluateField(object target, Type targetType, string name, bool isMatchValueMode, object targetValue, out bool result)
        {
            var field = targetType.GetField(name, BINDING_FLAGS);
            if (field != null)
            {
                var fieldValue = field.GetValue(target);
                result = CheckValue(fieldValue, isMatchValueMode, targetValue);
                return true;
            }

            result = false;
            return false;
        }

        private bool TryEvaluateProperty(object target, Type targetType, string name, bool isMatchValueMode, object targetValue, out bool result)
        {
            var prop = targetType.GetProperty(name, BINDING_FLAGS);
            if (prop != null)
            {
                var propValue = prop.GetValue(target, null);
                result = CheckValue(propValue, isMatchValueMode, targetValue);
                return true;
            }

            result = false;
            return false;
        }

        private bool TryEvaluateMethod(object target, Type targetType, string name, bool isMatchValueMode, object targetValue, out bool result)
        {
            var method = targetType.GetMethod(name, BINDING_FLAGS);
            if (method != null && method.GetParameters().Length == 0)
            {
                var methodValue = method.Invoke(target, null);
                result = CheckValue(methodValue, isMatchValueMode, targetValue);
                return true;
            }

            result = false;
            return false;
        }

        private bool CheckValue(object memberValue, bool isMatchValueMode, object targetValue)
        {
            if (isMatchValueMode)
                return Equals(memberValue?.ToString(), targetValue?.ToString());

            if (memberValue is bool boolVal)
                return boolVal;

            return true;
        }
    }
}
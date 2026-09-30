using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Drawer
{
    [LogicPriority(120)]
    internal sealed class ValidateInputDrawer : CursoriamInspectorDrawer<CursoriamInspectorField>
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        protected override VisualElement CreateVisualElementInternal(CursoriamInspectorField element)
        {
            var attributes = element.Member.GetCustomAttributes<ValidateInputAttribute>(true);
            var targetType = element.Target.GetType();

            var container = new VisualElement();
            var validators = new List<(MethodInfo Method, HelpBox Box)>();

            foreach (var attr in attributes)
            {
                var method = targetType.GetMethod(attr.ValidatorMethodName, BINDING_FLAGS);

                if (method == null)
                {
                    var msg = $"Ошибка: Метод-валидатор ({attr.ValidatorMethodName}) не найден в классе ({targetType.Name})!";
                    container.Add(new HelpBox().Setup(msg, HelpBoxMessageType.Error));
                    continue;
                }

                if (method.ReturnType != typeof(bool) || method.GetParameters().Length > 0)
                {
                    var msg = $"Ошибка: Метод ({attr.ValidatorMethodName}) должен возвращать bool и не иметь параметров!";
                    container.Add(new HelpBox().Setup(msg, HelpBoxMessageType.Error));
                    continue;
                }

                var message = string.IsNullOrWhiteSpace(attr.ErrorMessage)
                    ? $"Введено некорректное значение в поле ({ObjectNames.NicifyVariableName(element.Name)})!"
                    : attr.ErrorMessage;

                var warningBox = new HelpBox().Setup(message, HelpBoxMessageType.Warning);
                container.Add(warningBox);

                validators.Add((method, warningBox));
            }

            void RunValidation()
            {
                foreach (var validator in validators)
                {
                    var isValid = false;
                    try
                    {
                        isValid = (bool)validator.Method.Invoke(element.Target, null);
                    }
                    catch
                    {
                        isValid = false;
                    }

                    validator.Box.SetDisplayStyle(isValid ? DisplayStyle.None : DisplayStyle.Flex);
                }
            }

            RunValidation();

            container.TrackPropertyValue(element.Property, _ => RunValidation());
            return container;
        }

        protected override bool CanDrawInternal(CursoriamInspectorField element)
        {
            return base.CanDrawInternal(element) &&
                   element.Property != null &&
                   element.Member.IsDefined(typeof(ValidateInputAttribute), true);
        }

        public override bool IsDecorator => true;
    }
}
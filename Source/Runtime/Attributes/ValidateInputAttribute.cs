using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ValidateInputAttribute : CursoriamAttribute
    {
        public ValidateInputAttribute(string validatorMethodName, string errorMessage = null)
        {
            ValidatorMethodName = validatorMethodName;
            ErrorMessage = errorMessage;
        }

        public string ValidatorMethodName { get; }

        public string ErrorMessage { get; }
    }
}
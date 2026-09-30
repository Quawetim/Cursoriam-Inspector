using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RequiredAttribute : CursoriamAttribute
    {
        public RequiredAttribute(string message = null)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
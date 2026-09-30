using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class OnValueChangedAttribute : CursoriamAttribute
    {
        public OnValueChangedAttribute(string callbackMethodName = null)
        {
            CallbackMethodName = callbackMethodName;
        }

        public string CallbackMethodName { get; }
    }
}
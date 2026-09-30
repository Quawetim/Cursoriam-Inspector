using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ShowNonSerializedFieldAttribute : CursoriamAttribute
    {
    }
}
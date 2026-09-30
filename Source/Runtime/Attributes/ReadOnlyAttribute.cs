using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReadOnlyAttribute : CursoriamAttribute, ICursoriamMetaAttribute
    {
    }
}
using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReferenceGuardAttribute : CursoriamAttribute, ICursoriamMetaAttribute
    {
        public ReferenceGuardAttribute()
        {
            Id = null;
        }

        public ReferenceGuardAttribute(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
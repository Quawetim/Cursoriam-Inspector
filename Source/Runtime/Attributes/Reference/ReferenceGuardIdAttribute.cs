using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ReferenceGuardIdAttribute : CursoriamAttribute
    {
        public ReferenceGuardIdAttribute(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
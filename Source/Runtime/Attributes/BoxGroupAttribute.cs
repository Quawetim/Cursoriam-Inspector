using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class BoxGroupAttribute : CursoriamAttribute
    {
        public BoxGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }

        public string GroupName { get; }
    }
}
using System;

namespace BadPractice.Cursoriam.Inspector.Editor.Logic
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal sealed class LogicPriorityAttribute : Attribute
    {
        public LogicPriorityAttribute(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; }
    }
}
using BadPractice.Cursoriam.Inspector.Enum;
using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public sealed class HideIfAttribute : ShowIfAttribute
    {
        public HideIfAttribute(string conditionName)
            : base(conditionName) { }

        public HideIfAttribute(string conditionName, object targetValue)
            : base(conditionName, targetValue) { }

        public HideIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
            : base(conditionOperator, conditions) { }
    }
}
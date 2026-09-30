using BadPractice.Cursoriam.Inspector.Enum;
using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public sealed class DisableIfAttribute : EnableIfAttribute
    {
        public DisableIfAttribute(string conditionName)
            : base(conditionName) { }

        public DisableIfAttribute(string conditionName, object targetValue)
            : base(conditionName, targetValue) { }

        public DisableIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
            : base(conditionOperator, conditions) { }
    }
}
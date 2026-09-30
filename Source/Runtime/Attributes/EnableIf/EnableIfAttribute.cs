using BadPractice.Cursoriam.Inspector.Enum;
using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public class EnableIfAttribute : ShowIfAttribute
    {
        public EnableIfAttribute(string conditionName)
            : base(conditionName) { }

        public EnableIfAttribute(string conditionName, object targetValue)
            : base(conditionName, targetValue) { }

        public EnableIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
            : base(conditionOperator, conditions) { }
    }
}
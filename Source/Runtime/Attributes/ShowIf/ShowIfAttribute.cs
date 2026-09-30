using BadPractice.Cursoriam.Inspector.Enum;
using System;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
    public class ShowIfAttribute : CursoriamAttribute, ICursoriamMetaAttribute
    {
        public ShowIfAttribute(string conditionName)
        {
            Conditions = new[] { conditionName };
            Operator = EConditionOperator.And;
            IsMatchValueMode = false;
            TargetValue = null;
        }

        public ShowIfAttribute(string conditionName, object targetValue)
        {
            Conditions = new[] { conditionName };
            Operator = EConditionOperator.And;
            IsMatchValueMode = true;
            TargetValue = targetValue;
        }

        public ShowIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
        {
            Conditions = conditions;
            Operator = conditionOperator;
            IsMatchValueMode = false;
            TargetValue = null;
        }

        public string[] Conditions { get; }

        public object TargetValue { get; }

        public EConditionOperator Operator { get; }

        public bool IsMatchValueMode { get; }
    }
}
using System.Reflection;

namespace BadPractice.Cursoriam.Inspector.Editor
{
    public sealed class CursoriamInspectorMethod : CursoriamInspectorElement
    {
        public CursoriamInspectorMethod(MemberInfo memberInfo, object target, MethodInfo methodInfo)
            : base(memberInfo, target)
        {
            Method = methodInfo;
        }

        public MethodInfo Method { get; }
    }
}
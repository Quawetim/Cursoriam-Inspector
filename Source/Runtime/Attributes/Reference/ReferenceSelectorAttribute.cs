using System;
using UnityEngine;

namespace BadPractice.Cursoriam.Inspector.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReferenceSelectorAttribute : PropertyAttribute
    {
    }
}
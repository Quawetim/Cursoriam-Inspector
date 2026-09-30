using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BadPractice.Cursoriam.Inspector
{
    internal sealed class UnityFieldsContractResolver : DefaultContractResolver
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            if (objectType.Assembly == typeof(Vector3).Assembly)
            {
                return base.GetSerializableMembers(objectType);
            }

            var members = new List<MemberInfo>();
            var currentType = objectType;
            while (currentType != null && currentType != typeof(object))
            {
                var fields = currentType.GetFields(BINDING_FLAGS);

                foreach (var field in fields)
                {
                    var isUnitySerializable = (field.IsPublic && !field.IsDefined(typeof(HideInInspector), true))
                                              || field.IsDefined(typeof(SerializeField), true)
                                              || field.IsDefined(typeof(SerializeReference), true);

                    if (!isUnitySerializable)
                        continue;

                    members.Add(field);
                }

                currentType = currentType.BaseType;
            }

            return members;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.PropertyName = member.Name;
            property.Writable = true;
            property.Readable = true;
            return property;
        }
    }
}
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Logger;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BadPractice.Cursoriam.Inspector
{
    public sealed class ReferenceGuardBinder : ISerializationBinder
    {
        private static readonly Dictionary<string, Type> _typesById = new();
        private static readonly Dictionary<Type, string> _idByTypes = new();

        static ReferenceGuardBinder()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        var attr = type.GetCustomAttribute<ReferenceGuardIdAttribute>();
                        if (attr != null)
                        {
                            _typesById[attr.Id] = type;
                            _idByTypes[type] = attr.Id;
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            if (_idByTypes.TryGetValue(serializedType, out var typeId))
            {
                typeName = typeId;
            }
            else
            {
                typeName = serializedType.FullName;
                CursoriamLogger.Warning($"У класса ({serializedType.Name}) отсутствует атрибут [{nameof(ReferenceGuardIdAttribute)}]!");
            }
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            return (_typesById.TryGetValue(typeName, out var type)
                ? type
                : Type.GetType(typeName))!;
        }
    }
}
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using BadPractice.Cursoriam.Logger;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BadPractice.Cursoriam.Inspector.Editor
{
    internal sealed class ReferenceGuardObserver : AssetModificationProcessor
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                if (!path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                    continue;

                var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (assetType == null)
                    continue;

                var fields = assetType.GetFields(BINDING_FLAGS);
                var hasBackupProvider = false;

                foreach (var field in fields)
                {
                    if (!field.IsDefined(typeof(ReferenceGuardAttribute), true))
                        continue;

                    hasBackupProvider = true;
                    break;
                }

                if (!hasBackupProvider)
                    continue;

                var rootObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (!rootObject)
                    continue;

                ProcessObjectFields(rootObject);
            }

            return paths;
        }

        private static void ProcessObjectFields(UnityEngine.Object rootObject)
        {
            var fields = rootObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (!field.IsDefined(typeof(ReferenceGuardAttribute), true))
                    continue;

                var serializedObject = new SerializedObject(rootObject);
                var property = serializedObject.FindProperty(field.Name);
                if (property == null)
                    continue;

                var dummyElement = new CursoriamInspectorField(field, rootObject, property);
                Backup(dummyElement);
            }
        }

        private static void Backup(CursoriamInspectorField element)
        {
            var logic = new ReferenceGuardLogic();

            if (!logic.InitializeContext(element))
                return;

            if (!logic.IsAutoBackupActive)
                return;

            if (HasMissing(element, logic))
            {
                var rootObject = element.Property.serializedObject.targetObject;
                CursoriamLogger.Warning($"Авто-бэкап для ({rootObject.name}) приостановлен: обнаружены невосстановленные Missing Type поля. Сделайте Restore.");
                return;
            }

            logic.Backup();
        }

        private static bool HasMissing(CursoriamInspectorField parentElement, ReferenceGuardLogic logic)
        {
            var fieldInfo = parentElement.Member as FieldInfo;
            var settingsInstance = fieldInfo?.GetValue(parentElement.Target);
            if (settingsInstance == null)
                return false;

            var fields = settingsInstance.GetType().GetFields(BINDING_FLAGS);

            foreach (var field in fields)
            {
                if (!field.IsDefined(typeof(ReferenceGuardAttribute), true) ||
                    !field.IsDefined(typeof(SerializeReference), true))
                    continue;

                var arrayProperty = parentElement.Property.FindPropertyRelative(field.Name);
                if (arrayProperty == null)
                    continue;

                var hasMissingType = logic.HasMissingTypes(arrayProperty);
                if (hasMissingType)
                    return true;
            }

            return false;
        }
    }
}
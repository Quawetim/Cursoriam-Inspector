using BadPractice.Cursoriam.Extension.Editor;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Logger;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor.Logic
{
    [LogicPriority(40)]
    internal sealed class ReferenceGuardLogic : CursoriamLogic<CursoriamInspectorField>
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly JsonSerializerSettings _jsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = new ReferenceGuardBinder(),
            ContractResolver = new UnityFieldsContractResolver(),
            Formatting = Formatting.Indented
        };

        private string _backupPath;
        private ReferenceGuardData _data;
        private CursoriamInspectorField _element;

        public bool InitializeContext(CursoriamInspectorField element)
        {
            if (element.Property == null)
                return false;

            var rootObject = element.Property.serializedObject.targetObject;
            if (!rootObject)
                return false;

            var assetPath = AssetDatabase.GetAssetPath(rootObject);
            if (string.IsNullOrWhiteSpace(assetPath))
                return false;

            var directory = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrWhiteSpace(directory))
                return false;

            _backupPath = Path.Combine(directory, $"{rootObject.name}_backup.json");
            _element = element;

            LoadData();
            return true;
        }

        public void Backup()
        {
            if (_element == null || string.IsNullOrWhiteSpace(_backupPath))
                return;

            var fieldInfo = _element.Member as FieldInfo;
            var settingsInstance = fieldInfo?.GetValue(_element.Target);
            if (settingsInstance == null)
                return;

            _data ??= new();
            _data.SavedFields.Clear();
            _data.IsCleared = false;

            var fields = settingsInstance.GetType().GetFields(BINDING_FLAGS);
            var anyFieldsSaved = false;
            var isAllTargetsEmpty = true;

            foreach (var field in fields)
            {
                var targetId = GetGuardTargetId(field);
                if (targetId == null)
                    continue;

                var rawValue = field.GetValue(settingsInstance);
                if (field.FieldType.IsArray)
                {
                    if (rawValue is Array { Length: > 0 })
                    {
                        isAllTargetsEmpty = false;
                    }
                }
                else if (rawValue != null)
                {
                    isAllTargetsEmpty = false;
                }

                if (rawValue == null)
                    continue;

                _data.SavedFields[targetId] = rawValue;
                anyFieldsSaved = true;
            }

            if (anyFieldsSaved)
            {
                _data.IsCleared = isAllTargetsEmpty;
                Save();
            }
        }

        public void Restore()
        {
            if (_element == null || _data == null || string.IsNullOrWhiteSpace(_backupPath))
                return;

            var rootUnityObject = _element.Property.serializedObject.targetObject;
            if (!rootUnityObject)
                return;

            var settingsFieldInfo = _element.Member as FieldInfo;
            var settingsInstance = settingsFieldInfo?.GetValue(_element.Target);
            if (settingsInstance == null)
                return;

            var fields = settingsInstance.GetType().GetFields(BINDING_FLAGS);
            var anyRestored = false;

            Undo.RecordObject(rootUnityObject, "Restore Backup Data");

            foreach (var field in fields)
            {
                var targetId = GetGuardTargetId(field);
                if (targetId == null)
                    continue;

                if (!_data.SavedFields.TryGetValue(targetId, out var rawData))
                    continue;

                try
                {
                    var arrayProperty = _element.Property.FindPropertyRelative(field.Name);
                    if (arrayProperty == null)
                        continue;

                    var jsonList = rawData switch
                    {
                        IList directList => directList,
                        Dictionary<string, object> rootDict when rootDict.TryGetValue("$values", out var valuesObj) => valuesObj as IList,
                        _ => null
                    };

                    if (jsonList == null)
                        continue;

                    arrayProperty.ClearArray();
                    arrayProperty.arraySize = jsonList.Count;

                    for (var i = 0; i < jsonList.Count; i++)
                    {
                        var elementProperty = arrayProperty.GetArrayElementAtIndex(i);

                        if (jsonList[i] is not Dictionary<string, object> elementDict)
                            continue;

                        if (elementDict.TryGetValue("$type", out var typeIdObj) && typeIdObj is string typeId)
                        {
                            var targetType = new ReferenceGuardBinder().BindToType(null, typeId);
                            if (targetType != null)
                            {
                                elementProperty.managedReferenceValue = Activator.CreateInstance(targetType);
                            }
                        }

                        foreach (var kvp in elementDict)
                        {
                            if (kvp.Key.StartsWith("$"))
                                continue;

                            var relativeProp = elementProperty.FindPropertyRelative(kvp.Key);
                            if (relativeProp != null)
                            {
                                Apply(relativeProp, kvp.Value);
                            }
                        }
                    }

                    anyRestored = true;
                }
                catch (Exception ex)
                {
                    CursoriamLogger.Error($"Ошибка восстановления поля {field.Name}: {ex.Message}");
                }
            }

            if (!anyRestored)
                return;

            _element.Property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(rootUnityObject);

            SceneView.lastActiveSceneView?.ShowNotification(new("Reference Guard done!"));
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(_backupPath) || _data == null)
                return;

            try
            {
                var json = JsonConvert.SerializeObject(_data, Formatting.Indented, _jsonSettings);
                File.WriteAllText(_backupPath, json);
            }
            catch (Exception ex)
            {
                CursoriamLogger.Error($"Не удалось сохранить бекап: {ex.Message}");
            }
        }

        public void ToggleAutoBackup()
        {
            _data ??= new();
            _data.IsAutoBackupActive = !_data.IsAutoBackupActive;
        }

        public void ToggleAutoRestore()
        {
            _data ??= new();
            _data.IsAutoRestoreActive = !_data.IsAutoRestoreActive;
        }

        public bool HasMissingTypes(SerializedProperty property)
        {
            if (!property.isArray)
                return false;

            for (var i = 0; i < property.arraySize; i++)
            {
                var elementProp = property.GetArrayElementAtIndex(i);
                var isMissing = elementProp.propertyType == SerializedPropertyType.ManagedReference &&
                                elementProp.managedReferenceValue == null &&
                                !string.IsNullOrEmpty(elementProp.managedReferenceFullTypename);

                if (isMissing)
                    return true;
            }

            return false;
        }

        protected override void ProcessInternal(CursoriamInspectorField element, VisualElement visualElement)
        {
            if (element.Property == null || element.Member == null)
                return;

            if (!element.Member.IsDefined(typeof(ReferenceGuardAttribute), true))
                return;

            if (!InitializeContext(element))
                return;

            if (_data == null || !_data.IsAutoRestoreActive || _data.IsCleared)
                return;

            var fieldInfo = element.Member as FieldInfo;
            var settingsInstance = fieldInfo?.GetValue(element.Target);
            if (settingsInstance == null)
                return;

            var nestedFields = settingsInstance.GetType().GetFields(BINDING_FLAGS);
            var anyRestored = false;
            var rootObject = element.Property.serializedObject.targetObject;

            foreach (var field in nestedFields)
            {
                var targetId = GetGuardTargetId(field);
                if (targetId == null)
                    continue;

                var propPath = $"{element.Member.Name}.{field.Name}";
                var arrayProperty = element.Property.serializedObject.FindProperty(propPath);
                if (arrayProperty == null)
                    continue;

                var hasMissingTypes = HasMissingTypes(arrayProperty);
                var isPropertyEmpty = arrayProperty.isArray
                    ? arrayProperty.arraySize == 0
                    : arrayProperty.managedReferenceValue == null;

                if (hasMissingTypes || isPropertyEmpty)
                {
                    Restore();
                    anyRestored = true;
                    break;
                }
            }

            if (anyRestored)
            {
                element.Property.serializedObject.Update();
                CursoriamLogger.Trace($"Поврежденные поля ассета ({rootObject.name}) были автоматически восстановлены при инициализации.");
            }
        }

        private void LoadData()
        {
            if (IsBackupExists)
            {
                try
                {
                    var json = File.ReadAllText(_backupPath);
                    var jsonConverters = new JsonConverter[]
                    {
                        new DictionaryJsonConverter(),
                        new StringEnumConverter(new DefaultNamingStrategy()),
                    };

                    _data = JsonConvert.DeserializeObject<ReferenceGuardData>(json, jsonConverters);
                }
                catch
                {
                    // ignore
                }
            }

            _data ??= new();
        }

        private void Apply(SerializedProperty property, object value)
        {
            if (value == null)
                return;

            if (property.TrySetValue(value, false))
                return;

            if (!property.hasChildren || value is not Dictionary<string, object> subDict)
                return;

            foreach (var subKvp in subDict)
            {
                if (subKvp.Key.StartsWith("$"))
                    continue;

                var subProp = property.FindPropertyRelative(subKvp.Key);
                if (subProp != null)
                {
                    Apply(subProp, subKvp.Value);
                }
            }
        }

        private string GetGuardTargetId(FieldInfo field)
        {
            var guardAttr = field.GetCustomAttribute<ReferenceGuardAttribute>(true);
            if (guardAttr == null || !field.IsDefined(typeof(SerializeReference), true))
                return null;

            if (!string.IsNullOrWhiteSpace(guardAttr.Id))
                return guardAttr.Id;

            CursoriamLogger.Warning($"[ReferenceGuard] на поле ({field.DeclaringType?.Name}.{field.Name}) используется без явного ID! " +
                                    $"ID сгенерирован автоматически: \"{field.Name}\".");

            return field.Name;
        }

        public bool IsBackupExists => !string.IsNullOrWhiteSpace(_backupPath) && File.Exists(_backupPath);

        public bool IsAutoBackupActive => _data is { IsAutoBackupActive: true };

        public bool IsAutoRestoreActive => _data is { IsAutoRestoreActive: true };
    }
}
using BadPractice.Cursoriam.Extension;
using BadPractice.Cursoriam.Inspector.Attributes;
using BadPractice.Cursoriam.Inspector.Editor.Drawer;
using BadPractice.Cursoriam.Inspector.Editor.Logic;
using BadPractice.Cursoriam.Logger;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BadPractice.Cursoriam.Inspector.Editor
{
    [CanEditMultipleObjects]
    internal abstract class CursoriamInspectorEditor : UnityEditor.Editor
    {
        private const int MAX_RECURSION_DEPTH = 5;
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static CursoriamInspectorEditor Instance { get; private set; }

        private static readonly List<ICursoriamInspectorDrawer> _drawers = new();
        private static readonly List<ICursoriamLogic> _logic = new();
        private readonly List<CursoriamInspectorGroup> _groups = new();
        private readonly List<CursoriamInspectorElement> _elements = new();
        private bool _hasCustomAttributes;

        private void OnEnable()
        {
            Instance = this;
            RegisterAttributes();
            RebuildElements();
        }

        public override VisualElement CreateInspectorGUI()
        {
            if (!_hasCustomAttributes)
                return null;

            var rootContainer = new VisualElement();

            // default script
            var scriptProperty = serializedObject.FindProperty("m_Script");
            if (scriptProperty != null)
            {
                var scriptField = new PropertyField(scriptProperty);
                scriptField.SetEnabled(false);
                rootContainer.Add(scriptField);
            }

            // rest
            var groupTargetContainer = rootContainer;
            foreach (var group in _groups)
            {
                var isActualGroup = !string.IsNullOrWhiteSpace(group.Name);

                if (isActualGroup)
                {
                    var boxPanel = new VisualElement()
                        .AsBoxPanel(group.Name);
                    rootContainer.Add(boxPanel);
                    groupTargetContainer = boxPanel;
                }

                foreach (var element in group.Elements)
                {
                    var elementUi = CreateVisualElement(element);

                    if (elementUi == null)
                        continue;

                    groupTargetContainer.Add(elementUi);
                }
            }

            return rootContainer;
        }

        internal static VisualElement CreateVisualElement(CursoriamInspectorElement element, Type ignoredDecoratorType = null)
        {
            var elementRoot = new VisualElement();

            DrawDecorators(element, elementRoot, isTop: true, ignoredDecoratorType);

            VisualElement visualElement = null;
            ICursoriamInspectorDrawer mainDrawer = null;

            foreach (var drawer in _drawers)
            {
                if (!drawer.CanDraw(element) || drawer.IsDecorator)
                    continue;

                if (ignoredDecoratorType != null && drawer.GetType() == ignoredDecoratorType)
                    continue;

                mainDrawer = drawer;
                break;
            }

            if (mainDrawer != null)
            {
                visualElement = mainDrawer.CreateVisualElement(element);
            }
            else if (element is CursoriamInspectorField { Property: not null } fieldElement)
            {
                visualElement = new PropertyField(fieldElement.Property, element.DisplayName);
            }

            if (visualElement != null)
            {
                if (element is CursoriamInspectorField fieldElement)
                {
                    SetupLogic(fieldElement, visualElement);
                    SetupValueChangeCallbacks(fieldElement, visualElement);
                }

                elementRoot.Add(visualElement);
            }

            DrawDecorators(element, elementRoot, isTop: false, ignoredDecoratorType);
            return elementRoot;
        }

        private static void DrawDecorators(CursoriamInspectorElement element, VisualElement container, bool isTop, Type ignoredType)
        {
            foreach (var drawer in _drawers)
            {
                if (!drawer.CanDraw(element) || !drawer.IsDecorator)
                    continue;

                if (ignoredType != null && drawer.GetType() == ignoredType)
                    continue;

                var priorityAttr = drawer.GetType().GetCustomAttribute<LogicPriorityAttribute>();
                var priority = priorityAttr?.Priority ?? 50;

                var shouldDraw = isTop ? priority < 100 : priority > 100;
                if (!shouldDraw)
                    continue;

                var decoratorUi = drawer.CreateVisualElement(element);
                if (decoratorUi != null)
                {
                    container.Add(decoratorUi);
                }
            }
        }

        private static void SetupLogic(CursoriamInspectorField element, VisualElement visualElement)
        {
            void ExecuteLogic() => ProcessLogic(element, visualElement);

            ExecuteLogic();

            if (element.Property != null)
            {
                visualElement.TrackPropertyValue(element.Property, _ => ExecuteLogic());
            }
            else
            {
                visualElement.schedule.Execute(ExecuteLogic).Every(500);
            }
        }

        private static void SetupValueChangeCallbacks(CursoriamInspectorField element, VisualElement visualElement)
        {
            if (element.Property == null)
                return;

            visualElement.TrackPropertyValue(element.Property, _ => Instance?.CheckElementValueChanged(element));
        }

        internal static void ProcessLogic(CursoriamInspectorElement element, VisualElement visualElement)
        {
            element.IsVisible = true;
            element.IsEnabled = true;

            foreach (var logic in _logic)
            {
                logic.Process(element, visualElement);
            }
        }

        internal static void CollectFields(SerializedObject obj, List<CursoriamInspectorElement> elements,
            Type type, object targetObject, string parentPath, int indentLevel, int currentDepth, CursoriamInspectorField parent = null)
        {
            if (currentDepth > MAX_RECURSION_DEPTH)
                return;

            if (!Instance)
                return;

            var allFields = Instance.CollectFieldsUpwards(type);

            foreach (var field in allFields)
            {
                var isSerializable = !field.IsDefined(typeof(HideInInspector), true) &&
                                     (field.IsPublic ||
                                      field.IsDefined(typeof(SerializeField), true) ||
                                      field.IsDefined(typeof(SerializeReference), true));

                if (!isSerializable)
                    continue;

                var propertyPath = string.IsNullOrEmpty(parentPath)
                    ? field.Name
                    : $"{parentPath}.{field.Name}";

                var property = obj.FindProperty(propertyPath);
                if (property == null)
                    continue;

                if (CollectArrayField(field, targetObject, indentLevel, property, elements, parent))
                    continue;

                if (CollectCustomField(obj, field, targetObject, indentLevel, property, propertyPath, elements, currentDepth, parent))
                    continue;

                CollectField(field, targetObject, indentLevel, property, elements, parent);
            }
        }

        private static bool CollectArrayField(FieldInfo field, object targetObject, int indentLevel,
            SerializedProperty property, List<CursoriamInspectorElement> elements, CursoriamInspectorField parent = null)
        {
            var fieldType = field.FieldType;
            var isArrayOrList = fieldType.IsArray && fieldType != typeof(string);
            var isGenericList = fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>);

            if (!isArrayOrList && !isGenericList)
                return false;

            try
            {
                var elementType = isArrayOrList
                    ? fieldType.GetElementType()
                    : fieldType.GetGenericArguments()[0];

                var collectionElement = new CursoriamInspectorCollection(field, targetObject, property, elementType, indentLevel, parent);
                RegisterElement(collectionElement, elements);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool CollectCustomField(SerializedObject obj, FieldInfo field, object targetObject, int indentLevel,
            SerializedProperty property, string propertyPath, List<CursoriamInspectorElement> elements, int currentDepth,
            CursoriamInspectorField parent = null)
        {
            var fieldType = field.FieldType;
            var isCustomSerializable = fieldType.IsDefined(typeof(SerializableAttribute), false) &&
                                       !fieldType.IsPrimitive &&
                                       fieldType.Namespace != null &&
                                       !fieldType.Namespace.StartsWith("UnityEngine") &&
                                       !fieldType.Namespace.StartsWith("System");

            if (!isCustomSerializable)
                return false;

            var nestedTarget = targetObject != null
                ? field.GetValue(targetObject)
                : null;

            var parentElement = CollectField(field, targetObject, indentLevel, property, elements, parent);
            CollectFields(obj, parentElement.Children, fieldType, nestedTarget, propertyPath, indentLevel + 1, currentDepth + 1, parentElement);
            return true;
        }

        private static CursoriamInspectorField CollectField(FieldInfo field, object targetObject, int indentLevel,
            SerializedProperty property, List<CursoriamInspectorElement> elements, CursoriamInspectorField parent = null)
        {
            var element = new CursoriamInspectorField(field, targetObject, property, indentLevel, parent);
            RegisterElement(element, elements);
            return element;
        }

        private static void RegisterElement(CursoriamInspectorField element, List<CursoriamInspectorElement> elements)
        {
            Instance.EvaluateOrder(element);
            elements.Add(element);
        }

        private void RegisterAttributes()
        {
            _logic.Clear();
            _drawers.Clear();

            var editorAssembly = Assembly.GetExecutingAssembly();
            var types = editorAssembly.GetTypes();

            var drawersWithPriority = new List<(ICursoriamInspectorDrawer drawer, int priority)>();
            var logicWithPriority = new List<(ICursoriamLogic logic, int priority)>();

            foreach (var type in types)
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;

                var priorityAttr = type.GetCustomAttribute<LogicPriorityAttribute>();
                var priority = priorityAttr?.Priority ?? 50;

                if (typeof(ICursoriamInspectorDrawer).IsAssignableFrom(type))
                {
                    if (Activator.CreateInstance(type) is ICursoriamInspectorDrawer drawer)
                    {
                        drawersWithPriority.Add((drawer, priority));
                    }
                }
                else if (typeof(ICursoriamLogic).IsAssignableFrom(type))
                {
                    if (Activator.CreateInstance(type) is ICursoriamLogic logic)
                    {
                        logicWithPriority.Add((logic, priority));
                    }
                }
            }

            drawersWithPriority.Sort((a, b) => a.priority.CompareTo(b.priority));
            logicWithPriority.Sort((a, b) => a.priority.CompareTo(b.priority));

            foreach (var item in drawersWithPriority)
            {
                _drawers.Add(item.drawer);
            }

            foreach (var item in logicWithPriority)
            {
                _logic.Add(item.logic);
            }
        }

        private void RebuildElements()
        {
            if (!target)
                return;

            var targetType = target.GetType();
            _hasCustomAttributes = false;
            _elements.Clear();
            _groups.Clear();

            CollectFields(serializedObject, _elements, targetType, target, string.Empty, 0, 0);
            CollectMethods(targetType);
            CollectNonSerializedMembers(targetType);

            _elements.Sort((a, b) => a.Order.CompareTo(b.Order));

            var groupMap = new Dictionary<string, CursoriamInspectorGroup>();

            foreach (var element in _elements)
            {
                var groupAttr = element.Member.GetCustomAttribute<BoxGroupAttribute>(true);
                if (groupAttr == null)
                {
                    var singleGroup = new CursoriamInspectorGroup(null);
                    singleGroup.Elements.Add(element);
                    _groups.Add(singleGroup);
                }
                else
                {
                    if (!groupMap.TryGetValue(groupAttr.GroupName, out var group))
                    {
                        group = new(groupAttr.GroupName);
                        groupMap[groupAttr.GroupName] = group;
                        _groups.Add(group);
                    }

                    group.Elements.Add(element);
                }
            }
        }

        private List<FieldInfo> CollectFieldsUpwards(Type type)
        {
            var allFields = new List<FieldInfo>();
            var bindingFlags = BINDING_FLAGS | BindingFlags.DeclaredOnly;

            var typeHierarchy = new List<Type>();
            var currentType = type;

            while (true)
            {
                if (currentType == null || currentType == typeof(object))
                    break;

                if (currentType == typeof(MonoBehaviour) || currentType == typeof(ScriptableObject))
                    break;

                typeHierarchy.Add(currentType);
                currentType = currentType.BaseType;
            }

            for (var i = typeHierarchy.Count - 1; i >= 0; i--)
            {
                var fields = typeHierarchy[i].GetFields(bindingFlags);
                allFields.AddRange(fields);
            }

            return allFields;
        }

        private void CollectMethods(Type type)
        {
            var methods = type.GetMethods(BINDING_FLAGS);

            foreach (var method in methods)
            {
                if (!method.IsDefined(typeof(ButtonAttribute), true))
                    continue;

                var element = new CursoriamInspectorMethod(method, target, method);
                EvaluateOrder(element);
                _elements.Add(element);
            }
        }

        private void CollectNonSerializedMembers(Type type)
        {
            var members = type.GetMembers(BINDING_FLAGS);

            foreach (var member in members)
            {
                if (member.MemberType is not (MemberTypes.Field or MemberTypes.Property))
                    continue;

                if (!member.IsDefined(typeof(ShowNonSerializedFieldAttribute), true))
                    continue;

                if (_elements.Exists(e => e.Member == member))
                    continue;

                var element = new CursoriamInspectorElement(member, target);
                EvaluateOrder(element);
                _elements.Add(element);
            }
        }

        private void CheckElementValueChanged(CursoriamInspectorField element)
        {
            serializedObject.ApplyModifiedProperties();

            // check on field
            var currentAttr = element.Member.GetCustomAttribute<OnValueChangedAttribute>(true);
            if (currentAttr == null)
                return;

            var callbackName = currentAttr.CallbackMethodName;
            var targetObject = element.Target;

            // check parent
            if (string.IsNullOrWhiteSpace(callbackName) && element.Parent != null)
            {
                var parentAttr = element.Parent.Member.GetCustomAttribute<OnValueChangedAttribute>(true);
                callbackName = parentAttr?.CallbackMethodName;
                targetObject = element.Parent.Target;
            }

            // invoke
            if (!string.IsNullOrWhiteSpace(callbackName))
            {
                var callbackMethod = targetObject.GetType().GetMethod(callbackName, BINDING_FLAGS);

                if (callbackMethod == null && target)
                {
                    callbackMethod = target.GetType().GetMethod(callbackName, BINDING_FLAGS);
                }

                if (callbackMethod != null)
                {
                    var invoker = target.GetType().GetMethod(callbackName, BINDING_FLAGS) != null ? target : targetObject;
                    var parameters = callbackMethod.GetParameters();

                    switch (parameters.Length)
                    {
                        case 0:
                        {
                            callbackMethod.Invoke(invoker, null);
                            break;
                        }
                        case 1 when element.Property != null && element.Member is FieldInfo fieldInfo:
                        {
                            callbackMethod.Invoke(invoker, new[] { fieldInfo.GetValue(element.Target) });
                            break;
                        }
                    }
                }
                else
                {
                    CursoriamLogger.Error($"Метод ({callbackName}) не найден!");
                }
            }
        }

        private void EvaluateOrder(CursoriamInspectorElement element)
        {
            var orderAttr = element.Member.GetCustomAttribute<OrderAttribute>(true);
            element.Order = orderAttr?.Order ?? 0;

            foreach (var attr in element.Attributes)
            {
                if (attr is not CursoriamAttribute)
                    continue;

                _hasCustomAttributes = true;
                break;
            }
        }
    }

    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    internal sealed class CursoriamMonoBehaviourEditor : CursoriamInspectorEditor
    {
    }

    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    internal sealed class CursoriamScriptableObjectEditor : CursoriamInspectorEditor
    {
    }
}
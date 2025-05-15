using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

// SOURCE: https://www.youtube.com/watch?v=xcGPr04Mgm4
namespace VS.Utilities.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class RequireInterfaceAttribute : PropertyAttribute
    {
        public readonly Type InterfaceType;

        public RequireInterfaceAttribute(Type interfaceType)
        {
            this.InterfaceType = interfaceType;
        }
    }

    [Serializable]
    public class InterfaceReference<TInterface, TObject> where TObject : Object where TInterface : class
    {
        [SerializeField, HideInInspector] private TObject underlyingValue;

        public TInterface Value
        {
            get => underlyingValue switch
            {
                null => null,
                TInterface @interface => @interface,
                _ => throw new InvalidOperationException($"{underlyingValue} needs to implement {typeof(TInterface)}.")
            };
            set => underlyingValue = value switch
            {
                null => null,
                TObject newValue => newValue,
                _ => throw new ArgumentException($"{value} needs to be of type {typeof(TObject)}.", string.Empty)
            };
        }

        public TObject UnderlyingValue
        {
            get => underlyingValue;
            set => underlyingValue = value;
        }
        
        public InterfaceReference() { }
        
        public InterfaceReference(TObject value) => underlyingValue = value;
        
        public InterfaceReference(TInterface @interface) => underlyingValue = @interface as TObject;
    }
    
    [Serializable]
    public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object> where TInterface : class { }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InterfaceReference<>))]
    [CustomPropertyDrawer(typeof(InterfaceReference<,>))]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        private const string UnderlyingValueFieldName = "underlyingValue";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var underlyingProperty = property.FindPropertyRelative(UnderlyingValueFieldName);
            var args = GetArguments(fieldInfo);
            
            EditorGUI.BeginProperty(position, label, property);
            var assignedObject = EditorGUI.ObjectField(position, label, underlyingProperty.objectReferenceValue, typeof(Object), true);

            if (assignedObject != null)
            {
                if (assignedObject is GameObject gameObject)
                {
                    ValidateAndAssignObject(underlyingProperty, gameObject.GetComponent(args.InterfaceType), gameObject.name, args.InterfaceType.Name);
                }
                else
                {
                    ValidateAndAssignObject(underlyingProperty, assignedObject, args.InterfaceType.Name);
                }
            }
            else
            {
                underlyingProperty.objectReferenceValue = null;
            }
            
            EditorGUI.EndProperty();
        }

        private static InterfaceArgs GetArguments(FieldInfo fieldInfo)
        {
            Type objectType = null;
            Type interfaceType = null;
            Type fieldType = fieldInfo.FieldType;

            bool TryGetTypesFromInterfaceReference(Type type, out Type objType, out Type intfType)
            {
                objType = intfType = null;
                
                if (type?.IsGenericType != true) return false;
                
                var genericType = type.GetGenericTypeDefinition();
                if (genericType == typeof(InterfaceReference<>)) type = type.BaseType;

                if (type?.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
                {
                    var types = type.GetGenericArguments();
                    intfType = types[0];
                    objectType = types[1];
                    
                    return true;
                }

                return false;
            }

            void GetTypesFromList(Type type, out Type objType, out Type intfType)
            {
                objType = intfType = null;
                
                var listInterface  = type.GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(List<>));

                if (listInterface != null)
                {
                    var elementType = listInterface.GetGenericArguments()[0];
                    TryGetTypesFromInterfaceReference(elementType, out objType, out intfType);
                }
            }

            if (!TryGetTypesFromInterfaceReference(fieldType, out objectType, out interfaceType))
            {
                GetTypesFromList(fieldType, out objectType, out interfaceType);
            }
            
            return new InterfaceArgs(objectType, interfaceType);
        }

        private static void ValidateAndAssignObject(SerializedProperty property, Object targetObject,
            string componentNameOrType, string interfaceName = null)
        {
            if (targetObject != null)
            {
                property.objectReferenceValue = targetObject;
            }
            else
            {
                var message = interfaceName != null
                    ? $"GameObject '{componentNameOrType}'"
                    : "assigned object";

                Debug.LogWarning($"The {message} does not have a component that implements '{interfaceName}'.");
                property.objectReferenceValue = null;
            }
        }
    }
#endif

    public struct InterfaceArgs
    {
        public readonly Type ObjectType;
        public readonly Type InterfaceType;

        public InterfaceArgs(Type objectType, Type interfaceType)
        {
            ObjectType = objectType;
            InterfaceType = interfaceType;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SoftObjectAttribute : PropertyAttribute
    {
    }


#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(SoftObjectAttribute))]
    public class SoftObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(fieldInfo.FieldType == typeof(SoftObjectPath) || fieldInfo.FieldType == typeof(SoftObject))
            {
                string objName = "NULL";
                long fileID = 0;
                if(property.objectReferenceValue != null)
                {
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(((SoftObjectPath)property.objectReferenceValue).m_AssetPath);
                    if(obj != null)
                    {
                        objName = obj.name;

                        string guid;
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.objectReferenceValue, out guid, out fileID);
                    }
                }
                string displayName = string.Format($"{property.displayName} [{objName}]");
                property.objectReferenceValue = (SoftObjectPath)EditorGUI.ObjectField(position, new GUIContent(displayName, fileID.ToString()), property.objectReferenceValue, fieldInfo.FieldType, true);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
#endif
}
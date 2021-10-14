using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using UnityEditor;

namespace Framework.Core.Editor
{
    [CustomPropertyDrawer(typeof(SoftObjectAttribute))]
    public class SoftObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (fieldInfo.FieldType == typeof(SoftObjectPath) || fieldInfo.FieldType == typeof(SoftObject))
            {
                string objName = "NULL";
                // long fileID = 0;
                if (property.objectReferenceValue != null)
                {
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(((SoftObjectPath)property.objectReferenceValue).assetPath);
                    if (obj != null)
                    {
                        objName = obj.name;

                        // string guid;
                        // AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.objectReferenceValue, out guid, out fileID);
                    }
                }

                // 提示SoftObjectPath指向的对象名称
                string displayName = string.Format($"{property.displayName} [{objName}] [{RedirectorDB.GetLocalID(property.objectReferenceValue)}]");
                property.objectReferenceValue = (SoftObjectPath)EditorGUI.ObjectField(position, new GUIContent(displayName, objName/*, fileID.ToString()*/), property.objectReferenceValue, fieldInfo.FieldType, true);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}
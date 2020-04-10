using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
    public enum ReferenceType : int
    {
        Prefab
    }

    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public sealed class ReferenceAttribute : PropertyAttribute
    {
        public ReferenceType RefType { get; private set; }
        public ReferenceAttribute(ReferenceType refType)
        {
            RefType = refType;
        }
    }


#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ReferenceAttribute))]
    public class ReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var reference = (ReferenceAttribute)attribute;
            switch (reference.RefType)
            {
                case ReferenceType.Prefab:
                    HandlePrefabReference(position, property, label);
                    break;
            }
        }

        // 仅string类型，且属性为RerferenceType.Prefab的才处理
        private void HandlePrefabReference(Rect position, SerializedProperty property, GUIContent label)
        {
            if (SerializedPropertyType.String != property.propertyType)
            {
                EditorGUI.PropertyField(position, property);
                return;
            }

            {
                //根据路径得到一个类型为GameObject的对象
                var prefab = (GameObject)AssetDatabase.LoadAssetAtPath(property.stringValue, typeof(GameObject));
                
                Rect rc = new Rect(position.x, position.y, position.width - 80 - 10, position.height);
                var obj = (GameObject)EditorGUI.ObjectField(rc, property.displayName, prefab, typeof(GameObject), false);
                
                //得到prefab的路径
                string newPath = AssetDatabase.GetAssetPath(obj);
                //设置路径
                property.stringValue = newPath;

                rc = new Rect(position.x + position.width - 80, position.y, 80, position.height);
                GUI.Button(rc, "Apply");
            }
        }
    }

#endif
}
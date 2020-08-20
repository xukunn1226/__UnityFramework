using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Core
{
    public class PopupAttribute : PropertyAttribute
    {
        public object[] dataSet;

        public Type dataType;
        
        public PopupAttribute(Type dataType, params object[] list)
        {
            this.dataType = dataType;
            this.dataSet = list;
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PopupAttribute))]
    internal sealed class PopupDrawer : PropertyDrawer
    {
        List<string> displayNames = new List<string>();
        int selected;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PopupAttribute pop = (PopupAttribute)attribute;
            displayNames.Clear();
            for (int i = 0; i < pop.dataSet.Length; i++)
            {
                object data = pop.dataSet[i];
                displayNames.Add(data.ToString());
                if (pop.dataType == typeof(int) && property.intValue == (int)data)
                {
                    selected = i;
                }
                else
                {
                    //....
                }
            }

            EditorGUI.BeginChangeCheck();
            selected = EditorGUI.Popup(position, label.text, selected, displayNames.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                if (pop.dataType == typeof(int))
                {
                    property.intValue = (int)pop.dataSet[selected];
                }
                else
                {
                    //....
                }
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}
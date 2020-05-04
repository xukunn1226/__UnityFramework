using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.MeshParticleSystem.Editor
{
    [CustomEditor(typeof(FX_Root), true)]
    public class FX_RootInspector : UnityEditor.Editor
    {
        private SerializedProperty m_LifeTimeProp;
        private SerializedProperty m_RecyclingTypeProp;
        private SerializedProperty m_LRUPoolProp;

        private void OnEnable()
        {
            m_LifeTimeProp = serializedObject.FindProperty("m_LifeTime");
            m_RecyclingTypeProp = serializedObject.FindProperty("m_RecyclingType");
            m_LRUPoolProp = serializedObject.FindProperty("m_LRUPool");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_LifeTimeProp.floatValue = EditorGUILayout.DelayedFloatField("Life Time", m_LifeTimeProp.floatValue);

            m_RecyclingTypeProp.enumValueIndex = (int)(FX_Root.RecyclingType)EditorGUILayout.EnumPopup("Recycling Type", (FX_Root.RecyclingType)m_RecyclingTypeProp.enumValueIndex);

            EditorGUI.BeginDisabledGroup(m_RecyclingTypeProp.enumValueIndex != 2);

            m_LRUPoolProp.objectReferenceValue = EditorGUILayout.ObjectField("LRU Pool", m_LRUPoolProp.objectReferenceValue, typeof(SoftObject), false);

            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
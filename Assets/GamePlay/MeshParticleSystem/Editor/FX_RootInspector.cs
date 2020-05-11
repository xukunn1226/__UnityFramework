using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem.Editor
{
    [CustomEditor(typeof(FX_Root), true)]
    public class FX_RootInspector : UnityEditor.Editor
    {
        private SerializedProperty m_LifeTimeProp;
        private SerializedProperty m_RecyclingTypeProp;

        private void OnEnable()
        {
            m_LifeTimeProp = serializedObject.FindProperty("m_LifeTime");
            m_RecyclingTypeProp = serializedObject.FindProperty("m_RecyclingType");

            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update += UnityEditor.EditorApplication.QueuePlayerLoopUpdate;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update -= UnityEditor.EditorApplication.QueuePlayerLoopUpdate;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_LifeTimeProp.floatValue = EditorGUILayout.DelayedFloatField("Life Time", m_LifeTimeProp.floatValue);

            m_RecyclingTypeProp.enumValueIndex = (int)(FX_Root.RecyclingType)EditorGUILayout.EnumPopup("Recycling Type", (FX_Root.RecyclingType)m_RecyclingTypeProp.enumValueIndex);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            //Handles.BeginGUI();

            //Handles.ConeHandleCap(0, ((FX_Root)target).transform.position, Quaternion.identity, 1, EventType.Repaint);

            //Handles.EndGUI();
        }
    }
}
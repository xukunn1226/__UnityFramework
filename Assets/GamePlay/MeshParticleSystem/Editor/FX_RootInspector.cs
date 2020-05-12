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

        private float m_Time;

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

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            float speed = EditorGUILayout.DelayedFloatField("Speed", ((FX_Root)target).speed);
            if (EditorGUI.EndChangeCheck())
            {
                ((FX_Root)target).speed = Mathf.Max(0, speed);
            }

            float time = ((FX_Root)target).ElapsedLifeTime;
            EditorGUILayout.TextField("Time", time.ToString());

            if (((FX_Root)target).isPaused || ((FX_Root)target).isStoped)
            {
                if (GUILayout.Button("Play"))
                {
                    ((FX_Root)target).Play();
                }
            }
            else
            {
                if (GUILayout.Button("Pause"))
                {
                    ((FX_Root)target).Pause();
                }
            }
            if (GUILayout.Button("Stop"))
            {
                ((FX_Root)target).Stop();
            }
            if (GUILayout.Button("Restart"))
            {
                ((FX_Root)target).Restart();
            }

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
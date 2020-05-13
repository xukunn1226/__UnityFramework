using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem.Editor
{
    [CustomEditor(typeof(FX_Root), true)]
    public class FX_RootInspector : UnityEditor.Editor
    {
        private SerializedProperty m_SpeedProp;
        private SerializedProperty m_LifeTimeProp;
        private SerializedProperty m_RecyclingTypeProp;

        private float m_Time;
        private FX_Root m_FX;

        private void OnEnable()
        {
            m_SpeedProp = serializedObject.FindProperty("m_Speed");
            m_LifeTimeProp = serializedObject.FindProperty("m_LifeTime");
            m_RecyclingTypeProp = serializedObject.FindProperty("m_RecyclingType");
            m_FX = (FX_Root)target;

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

            m_SpeedProp.floatValue = EditorGUILayout.DelayedFloatField("Speed", m_SpeedProp.floatValue);

            m_LifeTimeProp.floatValue = EditorGUILayout.DelayedFloatField("Life Time", m_LifeTimeProp.floatValue);

            m_RecyclingTypeProp.enumValueIndex = (int)(FX_Root.RecyclingType)EditorGUILayout.EnumPopup("Recycling Type", (FX_Root.RecyclingType)m_RecyclingTypeProp.enumValueIndex);

            //DrawPreview();

            serializedObject.ApplyModifiedProperties();
        }

        private Rect windowRect = new Rect(0, 0, 230, 100);

        private void OnSceneGUI()
        {
            windowRect.x = SceneView.lastActiveSceneView.position.width - 240;
            windowRect.y = SceneView.lastActiveSceneView.position.height - windowRect.height - 220;
            windowRect = GUI.Window("ParticleController".GetHashCode(), windowRect, DrawWindowContents, "Mesh ParticleSystem");
        }

        void DrawWindowContents(int windowId)
        {
            if (!m_FX)
            {
                GUILayout.Label("No Particle System found");
                return;
            }
            DrawPreview();

            Event currentEvent = Event.current;
            if(currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
            {
                currentEvent.Use();
            }
        }

        private void DrawPreview()
        {
            GUILayout.BeginHorizontal();
            if (m_FX.isPaused || m_FX.isStoped)
            {
                if (GUILayout.Button("Play"))
                {
                    m_FX.Play();
                }
            }
            else
            {
                if (GUILayout.Button("Pause"))
                {
                    m_FX.Pause();
                }
            }
            if (GUILayout.Button("Restart"))
            {
                m_FX.Restart();
            }
            if (GUILayout.Button("Stop"))
            {
                m_FX.Stop();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            float speed = EditorGUILayout.FloatField("Speed", m_FX.speed);
            if (EditorGUI.EndChangeCheck())
            {
                m_FX.speed = Mathf.Max(0, speed);
            }

            float time = m_FX.ElapsedLifeTime;
            EditorGUILayout.TextField("Time", time.ToString());
        }
    }
}
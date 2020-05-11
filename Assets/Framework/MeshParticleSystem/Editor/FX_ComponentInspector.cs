using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.MeshParticleSystem.Editor
{
    [CustomEditor(typeof(FX_Component), true)]
    public class FX_ComponentInspector : UnityEditor.Editor
    {
        protected virtual void OnEnable()
        {
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update += UnityEditor.EditorApplication.QueuePlayerLoopUpdate;
        }

        protected virtual void OnDisable()
        {
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update -= UnityEditor.EditorApplication.QueuePlayerLoopUpdate;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Play"))
            {
                ((FX_Component)target).Play();
            }
            if (GUILayout.Button("Pause"))
            {
                ((FX_Component)target).Pause();
            }
            if (GUILayout.Button("Stop"))
            {
                ((FX_Component)target).Stop();
            }
            if (GUILayout.Button("Restart"))
            {
                ((FX_Component)target).Restart();
            }

            EditorGUI.BeginDisabledGroup(!((FX_Component)target).isStoped);
            if(GUILayout.Button("Apply"))
            {
                ((FX_Component)target).RecordInit();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem.Editor
{
    [CustomEditor(typeof(FX_Component), true)]
    public class FX_ComponentEditor : UnityEditor.Editor
    {
        private float m_Time;

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

        //public override void OnInspectorGUI()
        //{
        //    base.OnInspectorGUI();

        //    EditorGUILayout.Separator();
        //    EditorGUILayout.Separator();

        //    EditorGUI.BeginChangeCheck();
        //    float speed = EditorGUILayout.DelayedFloatField("Speed", ((FX_Component)target).speed);
        //    if(EditorGUI.EndChangeCheck())
        //    {
        //        ((FX_Component)target).speed = Mathf.Max(0, speed);
        //    }

        //    EditorGUILayout.TextField("Time", ((FX_Component)target).elapsedTime.ToString());

        //    if (GUILayout.Button("Play"))
        //    {
        //        ((FX_Component)target).Play();
        //    }
        //    if (GUILayout.Button("Pause"))
        //    {
        //        ((FX_Component)target).Pause();
        //    }
        //    if (GUILayout.Button("Stop"))
        //    {
        //        ((FX_Component)target).Stop();
        //    }
        //    if (GUILayout.Button("Restart"))
        //    {
        //        ((FX_Component)target).Restart();
        //    }

        //    EditorGUI.BeginDisabledGroup(!((FX_Component)target).isStoped);
        //    if (GUILayout.Button("Apply"))
        //    {
        //        ((FX_Component)target).RecordInit();
        //    }
        //    EditorGUI.EndDisabledGroup();
        //}
    }
}
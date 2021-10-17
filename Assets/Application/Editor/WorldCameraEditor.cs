using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Application.Runtime;
using Framework.Core.Editor;

namespace Application.Editor
{
    [CustomEditor(typeof(WorldCamera))]
    public class WorldCameraEditor : UnityEditor.Editor
    {
        private CameraEffectInfo            m_EffectInfo;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Separator();
            EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
            {
                GUILayout.Label("编辑相机震屏", EGUIStyles.TitleTextStyle);

                ////// CameraEffectProfile
                m_EffectInfo = (CameraEffectInfo)EditorGUILayout.ObjectField("CameraEffectInfo", m_EffectInfo, typeof(CameraEffectInfo), true, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("资源路径：       " + m_EffectInfo != null ? AssetDatabase.GetAssetPath(m_EffectInfo) : "", GUILayout.ExpandWidth(true));
                
                ////// GO
                EditorGUILayout.Separator();
                if (GUILayout.Button("GO", GUILayout.Height(30)))
                {
                    ((WorldCamera)target).PlayCameraEffect(m_EffectInfo);
                }

                GUILayout.Space(10);
                GUILayout.Label("当前相机震屏参数", EGUIStyles.Label2);
                if (m_EffectInfo != null)
                {
                    UnityEditor.Editor editor = CreateEditor(m_EffectInfo);
                    editor.OnInspectorGUI();
                }
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
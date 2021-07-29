using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AnimationInstancingModule.Runtime;

namespace AnimationInstancingModule.Editor
{
    [CustomEditor(typeof(AnimationInstancing))]
    [DisallowMultipleComponent]
    public class AnimationInstancingEditor : UnityEditor.Editor
    {
        private AnimationInstancing     m_Target;
        private string                  m_Output = "";
        private int                     m_FPS = 15;
        private bool                    m_ExposeAttachments;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUIStyle boldStyle = EditorStyles.boldLabel;
            boldStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Generation Setting", boldStyle);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Button("1111");
            }
            GUILayout.EndVertical();

            GUILayout.Button("Generate");
        }
    }
}
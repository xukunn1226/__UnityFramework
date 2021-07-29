using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AnimationInstancing.Runtime;

namespace AnimationInstancing.Editor
{
    [CustomEditor(typeof(AnimationInstancing.Runtime.AnimationInstancing))]
    [DisallowMultipleComponent]
    public class AnimationInstancingEditor : UnityEditor.Editor
    {
        private string m_Output = "";
        private int m_FPS = 15;
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
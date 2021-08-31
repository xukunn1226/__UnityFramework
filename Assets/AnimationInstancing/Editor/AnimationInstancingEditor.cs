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
        private AnimationInstancing m_Target;
        private bool m_isPersistent;
        private int m_SelectedIndex;

        private void OnEnable()
        {
            m_Target = (AnimationInstancing)target;
            m_isPersistent = EditorUtility.IsPersistent(m_Target);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginDisabledGroup(m_isPersistent || !UnityEngine.Application.isPlaying);

            EditorGUILayout.LabelField("Preview");
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            
            string[] names = GetAnimationNames();
            int[] option = new int[names.Length];
            for(int i = 0; i < option.Length; ++i)
            {
                option[i] = i;
            }
            m_SelectedIndex = EditorGUILayout.IntPopup(m_SelectedIndex, names, option);
            
            if(GUILayout.Button("Play"))
            {
                m_Target.PlayAnimation(names[m_SelectedIndex]);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();

            EditorGUI.EndDisabledGroup();
        }

        private string[] GetAnimationNames()
        {
            return m_Target.prototype.EditorLoadAnimationInfo().ToArray();
        }
    }
}
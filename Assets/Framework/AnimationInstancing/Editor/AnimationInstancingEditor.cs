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
            m_SelectedIndex = -1;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginDisabledGroup(m_isPersistent || !UnityEngine.Application.isPlaying);
            {
                EditorGUILayout.LabelField("Preview");
                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    string[] names = m_Target.prototype.EditorLoadAnimationInfo().ToArray();
                    int[] option = new int[names.Length];
                    for (int i = 0; i < option.Length; ++i)
                    {
                        option[i] = i;
                    }
                    int index = EditorGUILayout.IntPopup(m_SelectedIndex, names, option);
                    if (index != m_SelectedIndex)
                    {
                        m_SelectedIndex = index;
                        m_Target.PlayAnimation(names[m_SelectedIndex]);
                    }
                }
                GUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
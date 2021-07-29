using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AnimationInstancingModule.Runtime;

namespace AnimationInstancingModule.Editor
{
    /// <summary>
    /// 
    /// <summary>
    [CustomEditor(typeof(AnimationInstancingGenerator))]
    [DisallowMultipleComponent]
    public class AnimationInstancingGeneratorEditor : UnityEditor.Editor
    {
        class AnimationBakeInfo
        {
            public SkinnedMeshRenderer[]                            m_Smrs;
            public Animator                                         m_Animator;
            public int                                              m_WorkingFrame;
            public float                                            m_Length;
            public int                                              m_Layer;
            public AnimationInstancingModule.Runtime.AnimationInfo  m_AnimationInfo;
        }

        private AnimationInstancingGenerator    m_Target;
        private string                          m_Output = "";
        private Transform[]                     m_BoneTransform;
        private List<Matrix4x4>                 m_BindPose          = new List<Matrix4x4>(150);
        private Animator                        m_Animator;
        private List<AnimationBakeInfo>         m_GeneratedBakeInfo = new List<AnimationBakeInfo>();

        private void OnEnable()
        {
            m_Target = (AnimationInstancingGenerator)target;
            m_Animator = m_Target.GetComponentInChildren<Animator>();
        }

        public override void OnInspectorGUI()
        {
            // GUIStyle boldStyle = EditorStyles.boldLabel;
            // boldStyle.alignment = TextAnchor.MiddleLeft;
            // EditorGUILayout.LabelField("Generation Setting", boldStyle);

            EditorGUI.BeginChangeCheck();

            m_Target.fps = EditorGUILayout.IntSlider("FPS", m_Target.fps, 1, 120);

            m_Target.exposeAttachments = EditorGUILayout.Toggle("Enable Attachments", m_Target.exposeAttachments);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Button("1111");
                if(m_BoneTransform == null)
                {
                    SkinnedMeshRenderer[] smrs = m_Target.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    m_BoneTransform = AnimationInstancingModule.Runtime.AnimationUtility.MergeBone(smrs, m_BindPose);
                }
            }
            GUILayout.EndVertical();

            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_Target);
            }

            GUILayout.Button("Generate");
        }
    }
}
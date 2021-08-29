using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AnimationInstancingModule.Runtime;
using System.Linq;
using UnityEditor.Animations;

namespace AnimationInstancingModule.Editor
{
    /// <summary>
    /// 
    /// <summary>
    [CustomEditor(typeof(AnimationInstancingGenerator))]
    [DisallowMultipleComponent]
    public class AnimationInstancingGeneratorEditor : UnityEditor.Editor
    {
        private AnimationInstancingGenerator    m_Target;
        private List<Matrix4x4>                 m_BindPose          = new List<Matrix4x4>(150);
        private Vector2                         m_ScrollPosition;
        private Vector2                         m_ScrollPosition2;
        private Dictionary<string, bool>        m_Temp              = new Dictionary<string, bool>();
        private List<Transform>                 m_LODs;
        private bool                            m_InitExtraBone;

        private void OnEnable()
        {
            m_Target = (AnimationInstancingGenerator)target;
            m_LODs = m_Target.GetLODs();
        }

        public override void OnInspectorGUI()
        {
            if(m_LODs.Count == 0)
            {
                GUIStyle boldStyle = EditorStyles.boldLabel;
                boldStyle.alignment = TextAnchor.MiddleLeft;
                Color backup = boldStyle.normal.textColor;
                boldStyle.normal.textColor = Color.red;

                EditorGUILayout.LabelField(@"Error: The first child must be ""LOD0"".", boldStyle);
                boldStyle.normal.textColor = backup;
                return;
            }

            EditorGUI.BeginChangeCheck();
            m_Target.enableReference = EditorGUILayout.Toggle("Enable reference", m_Target.enableReference);
            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_Target);
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.BeginChangeCheck();
            {                
                EditorGUI.BeginDisabledGroup(!m_Target.enableReference);
                {
                    m_Target.forceRebuildReference = EditorGUILayout.Toggle("Rebuild reference", m_Target.forceRebuildReference);
                    m_Target.referenceTo = EditorGUILayout.ObjectField("Reference",
                                                                       m_Target.referenceTo,
                                                                       typeof(AnimationInstancingGenerator), 
                                                                       true) as AnimationInstancingGenerator;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(m_Target.enableReference);
                {
                    // draw "FPS"
                    m_Target.fps = EditorGUILayout.IntSlider("FPS", m_Target.fps, 1, 120);

                    // draw "Attachment"
                    DrawAttachment();

                    // draw "AnimationClip" list
                    DrawAnimationClips();
                }
                EditorGUI.EndDisabledGroup();
            }
            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_Target);
            }
            GUILayout.EndVertical();
            
            if(GUILayout.Button("Refresh Extra Bone"))
            {
                RefreshAttachment();
            }

            bool canGen = true;
            if(m_Target.enableReference && m_Target.referenceTo == null)
            {
                canGen = false;
            }
            EditorGUI.BeginDisabledGroup(!canGen);
            if(GUILayout.Button("Generate"))
            {
                BakeWithAnimator();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void BakeWithAnimator()
        {
            m_Target.Bake();
        }

        private void DrawAttachment()
        {
            EditorGUILayout.PrefixLabel("Extra bones");
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                if(!m_InitExtraBone)
                {
                    m_InitExtraBone = true;
                    UpdateSelectExtraBone();
                }

                if (m_Target.m_SelectExtraBone.Count > 0)
                {
                    m_Temp.Clear();
                    foreach (var obj in m_Target.m_SelectExtraBone)
                    {
                        m_Temp[obj.Key] = obj.Value;
                    }
                    m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                    foreach (var obj in m_Temp)
                    {
                        bool value = EditorGUILayout.Toggle(string.Format("   {0}", obj.Key), obj.Value);
                        m_Target.m_SelectExtraBone[obj.Key] = value;
                    }
                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndVertical();
        }

        private void RefreshAttachment()
        {
            UpdateSelectExtraBone();
            EditorUtility.SetDirty(m_Target);
        }

        // find all extra bones
        private void UpdateSelectExtraBone()
        {
            SkinnedMeshRenderer[] meshRender = m_LODs[0].GetComponentsInChildren<SkinnedMeshRenderer>();
            List<Transform> boneTransform = new List<Transform>();
            AnimationInstancingModule.Runtime.AnimationUtility.MergeBone(meshRender, ref m_BindPose, ref boneTransform);

            // 筛选出除骨骼节点之外的节点
            var allTrans = m_LODs[0].GetComponentsInChildren<Transform>().ToList();
            allTrans.RemoveAll(q => boneTransform.Contains(q));

            Dictionary<string, bool> selectExtraBone = new Dictionary<string, bool>(m_Target.m_SelectExtraBone);

            m_Target.m_SelectExtraBone.Clear();
            for (int i = 0; i != allTrans.Count; ++i)
            {
                if (selectExtraBone.ContainsKey(allTrans[i].name))
                {
                    m_Target.m_SelectExtraBone.Add(allTrans[i].name, selectExtraBone[allTrans[i].name]);
                }
                else
                {
                    m_Target.m_SelectExtraBone.Add(allTrans[i].name, false);
                }
            }
        }

        private void DrawAnimationClips()
        {
            Animator animator = m_Target.GetComponent<Animator>();
            if (animator == null)
            {
                GUIStyle boldStyle = EditorStyles.boldLabel;
                boldStyle.alignment = TextAnchor.MiddleLeft;
                Color backup = boldStyle.normal.textColor;
                boldStyle.normal.textColor = Color.red;

                EditorGUILayout.LabelField("Error: The prefab should have a Animator Component.", boldStyle);
                boldStyle.normal.textColor = backup;
                return;
            }
            if (animator.runtimeAnimatorController == null)
            {
                GUIStyle boldStyle = EditorStyles.boldLabel;
                boldStyle.alignment = TextAnchor.MiddleLeft;
                Color backup = boldStyle.normal.textColor;
                boldStyle.normal.textColor = m_Target.enableReference ? Color.white : Color.red;

                string info = m_Target.enableReference ? "Info: you can check included animation clips in reference object" : "Error: The prefab's Animator should have a Animator Controller.";

                EditorGUILayout.LabelField(info, boldStyle);
                boldStyle.normal.textColor = backup;
                return;
            }

            var clips = GetClips(animator);
            var distinctClips = clips.Select(q => (AnimationClip)q).Distinct().ToList();        // make unique

            // 增加
            bool bModified = false;
            for (int i = 0; i < distinctClips.Count; i++)
            {
                if (distinctClips[i] && !m_Target.m_GenerateAnims.ContainsKey(distinctClips[i].name))
                {
                    m_Target.m_GenerateAnims.Add(distinctClips[i].name, true);
                    bModified = true;
                }
            }

            // 删除
            List<string> tmp = new List<string>();
            Dictionary<string, bool>.Enumerator e = m_Target.m_GenerateAnims.GetEnumerator();
            while(e.MoveNext())
            {
                if(!distinctClips.Find(item => item.name == e.Current.Key))
                {
                    tmp.Add(e.Current.Key);
                }
            }
            foreach(var str in tmp)
            {
                m_Target.m_GenerateAnims.Remove(str);
                bModified = true;
            }

            if(bModified)
            {
                EditorUtility.SetDirty(m_Target);
            }

            string[] clipNames = m_Target.m_GenerateAnims.Keys.ToArray();
            int totalFrames = 0;
            List<int> frames = new List<int>();
            foreach (var clipName in clipNames)
            {
                if (!m_Target.m_GenerateAnims[clipName])
                    continue;

                AnimationClip clip = clips.Find(delegate (AnimationClip c)
                {
                    if (c != null)
                        return c.name == clipName;
                    return false;
                });
                int framesToBake = clip ? m_Target.CalculateTotalFrames(clip.length, m_Target.fps) : 1;
                framesToBake = Mathf.Clamp(framesToBake, 1, framesToBake);
                totalFrames += framesToBake;
                frames.Add(framesToBake);
            }
            
            List<Transform> boneTransform = new List<Transform>();
            m_Target.GetSkinnedBoneInfo(m_LODs[0], ref m_BindPose, ref boneTransform);

            int textureWidth, textureHeight;
            m_Target.CalculateTextureSize(frames, boneTransform, out textureWidth, out textureHeight);
            EditorGUILayout.LabelField(string.Format($"Animation Texture will be one {textureWidth} X {textureHeight} texture"));

            m_ScrollPosition2 = GUILayout.BeginScrollView(m_ScrollPosition2);
            foreach (var clipName in clipNames)
            {
                AnimationClip clip = clips.Find(delegate (AnimationClip c)
                {
                    if (c != null)
                        return c.name == clipName;
                    return false;
                });

                int framesToBake = clip ? m_Target.CalculateTotalFrames(clip.length, m_Target.fps) : 1;
                framesToBake = Mathf.Clamp(framesToBake, 1, framesToBake);
                GUILayout.BeginHorizontal();
                {
                    m_Target.m_GenerateAnims[clipName] = EditorGUILayout.Toggle(string.Format("({0}) {1} ", framesToBake, clipName), m_Target.m_GenerateAnims[clipName]);
                }
                GUILayout.EndHorizontal();
                if (framesToBake > 5000)
                {
                    GUI.skin.label.richText = true;
                    EditorGUILayout.LabelField("<color=red>Long animations degrade performance, consider using a higher frame skip value.</color>", GUI.skin.label);
                }
            }
            GUILayout.EndScrollView();
        }        

        private List<AnimationClip> GetClips(Animator animator)
        {
            UnityEditor.Animations.AnimatorController controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            return GetClipsFromStatemachine(controller.layers[0].stateMachine);
        }

        private List<AnimationClip> GetClipsFromStatemachine(UnityEditor.Animations.AnimatorStateMachine stateMachine)
        {
            List<AnimationClip> list = new List<AnimationClip>();
            for (int i = 0; i != stateMachine.states.Length; ++i)
            {
                UnityEditor.Animations.ChildAnimatorState state = stateMachine.states[i];
				if (state.state.motion is UnityEditor.Animations.BlendTree)
				{
					UnityEditor.Animations.BlendTree blendTree = state.state.motion as UnityEditor.Animations.BlendTree;
					ChildMotion[] childMotion = blendTree.children;
					for(int j = 0; j != childMotion.Length; ++j) 
					{
						list.Add(childMotion[j].motion as AnimationClip);
					}
				}
				else if (state.state.motion != null)
                	list.Add(state.state.motion as AnimationClip);
            }
            for (int i = 0; i != stateMachine.stateMachines.Length; ++i)
            {
                list.AddRange(GetClipsFromStatemachine(stateMachine.stateMachines[i].stateMachine));
            }            
            return list;
        }
    }
}
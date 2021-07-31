using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AnimationInstancingModule.Runtime;
using System.Linq;
using UnityEditor.Animations;
using Framework.Core;

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
        private string                          m_Output = "";
        private List<Matrix4x4>                 m_BindPose          = new List<Matrix4x4>(150);
        private Vector2                         m_ScrollPosition;
        private Vector2                         m_ScrollPosition2;
        private Dictionary<string, bool>        m_Temp              = new Dictionary<string, bool>();

        private void OnEnable()
        {
            m_Target = (AnimationInstancingGenerator)target;
        }

        public override void OnInspectorGUI()
        {            
            EditorGUI.BeginChangeCheck();
            {
                // draw "FPS"
                m_Target.fps = EditorGUILayout.IntSlider("FPS", m_Target.fps, 1, 120);

                // draw "Attachment"
                DrawAttachment();

                // draw "AnimationClip" list
                DrawAnimationClips();
            }
            if(EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_Target);
            }
            
            if(GUILayout.Button("Refresh Extra Bone"))
            {
                RefreshAttachment();
            }
            if(GUILayout.Button("Generate"))
            {
                BakeWithAnimator();
            }
        }

        private void BakeWithAnimator()
        {

        }

        private void DrawAttachment()
        {
            m_Target.exposeAttachments = EditorGUILayout.Toggle("Enable Attachments", m_Target.exposeAttachments);            
            EditorGUI.BeginDisabledGroup(!m_Target.exposeAttachments);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                // EditorGUI.BeginChangeCheck();

                // GameObject fbx = EditorGUILayout.ObjectField("FBX refrenced by Prefab:", m_Target.fbx, typeof(GameObject), false) as GameObject;

                // if(EditorGUI.EndChangeCheck())
                // {
                //     if(fbx == null)
                //     {
                //         m_Target.m_SelectExtraBone.Clear();
                //     }
                //     else if(m_Target.fbx != fbx)
                //     {
                //         SkinnedMeshRenderer[] meshRender = m_Target.GetComponentsInChildren<SkinnedMeshRenderer>();
                //         Transform[] boneTransform = AnimationInstancingModule.Runtime.AnimationUtility.MergeBone(meshRender, ref m_BindPose);

                //         // 筛选出除骨骼节点之外的节点
                //         var allTrans = m_Target.GetComponentsInChildren<Transform>().ToList();
                //         allTrans.RemoveAll(q => boneTransform.Contains(q));

                //         m_Target.m_SelectExtraBone.Clear();
                //         for (int i = 0; i != allTrans.Count; ++i)
                //         {
                //             m_Target.m_SelectExtraBone.Add(allTrans[i].name, false);
                //         }
                //     }
                //     m_Target.fbx = fbx;                    
                //     EditorUtility.SetDirty(m_Target);
                // }

                // find all extra bones
                if (m_Target.m_SelectExtraBone.Count == 0)
                {
                    SkinnedMeshRenderer[] meshRender = m_Target.GetComponentsInChildren<SkinnedMeshRenderer>();
                    List<Transform> boneTransform = new List<Transform>();
                    AnimationInstancingModule.Runtime.AnimationUtility.MergeBone(meshRender, ref m_BindPose, ref boneTransform);

                    // 筛选出除骨骼节点之外的节点
                    var allTrans = m_Target.GetComponentsInChildren<Transform>().ToList();
                    allTrans.RemoveAll(q => boneTransform.Contains(q));

                    m_Target.m_SelectExtraBone.Clear();
                    for(int i = 0; i != allTrans.Count; ++i)
                    {
                        m_Target.m_SelectExtraBone.Add(allTrans[i].name, false);
                    }
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
            EditorGUI.EndDisabledGroup();            
        }

        private void RefreshAttachment()
        {
            m_Target.m_SelectExtraBone.Clear();
        }

        private void DrawAnimationClips()
        {
            Animator animator = m_Target.GetComponent<Animator>();
            if (animator == null)
            {
                GUIStyle boldStyle = EditorStyles.boldLabel;
                boldStyle.alignment = TextAnchor.MiddleLeft;
                boldStyle.normal.textColor = Color.red;

                EditorGUILayout.LabelField("Error: The prefab should have a Animator Component.", boldStyle);
                return;
            }
            if (animator.runtimeAnimatorController == null)
            {
                GUIStyle boldStyle = EditorStyles.boldLabel;
                boldStyle.alignment = TextAnchor.MiddleLeft;
                boldStyle.normal.textColor = Color.red;

                EditorGUILayout.LabelField("Error: The prefab's Animator should have a Animator Controller.", boldStyle);
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
                int framesToBake = clip ? (int)(clip.length * m_Target.fps / 1.0f) : 1;
                framesToBake = Mathf.Clamp(framesToBake, 1, framesToBake);
                totalFrames += framesToBake;
                frames.Add(framesToBake);
            }
            
            // List<Transform> boneTransform = new List<Transform>();
            // m_Target.GetFinalBonePose(ref m_BindPose, ref boneTransform);

            // int textureWidth, textureHeight;
            // CalculateTextureSize(frames, boneTransform, out textureWidth, out textureHeight);

            m_ScrollPosition2 = GUILayout.BeginScrollView(m_ScrollPosition2);
            foreach (var clipName in clipNames)
            {
                AnimationClip clip = clips.Find(delegate (AnimationClip c)
                {
                    if (c != null)
                        return c.name == clipName;
                    return false;
                });

                int framesToBake = clip ? (int)(clip.length * m_Target.fps / 1.0f) : 1;
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

        // 计算动画数据占用的贴图大小
        // 每根骨骼4个像素（一个像素记录4个值，4个像素一个矩阵）
        private void CalculateTextureSize(List<int> frames, Transform[] bone, out int textureWidth, out int textureHeight)
        {
            frames.QuickSort();
            int totalFrames = frames.Count<int>();
            int averageFrame = totalFrames / frames.Count;

            textureWidth = totalFrames * 4;
            textureHeight = 0;

            int blockWidth = 4;
            int blockHeight = bone.Length;

            int pixels = bone.Length * totalFrames * 4;
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
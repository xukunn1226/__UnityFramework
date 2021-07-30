using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInstancingGenerator : MonoBehaviour, ISerializationCallbackReceiver
    {
        public int                          fps;
        public bool                         exposeAttachments;
        public GameObject                   fbx;

        [SerializeField]
        private List<string>                m_ExtraBoneNames            = new List<string>();                   // 所有绑点骨骼名称列表
        [SerializeField]
        private List<bool>                  m_ExtraBoneSelectables      = new List<bool>();                     // 绑点骨骼的选中状态
        public Dictionary<string, bool>     m_SelectExtraBone           = new Dictionary<string, bool>();
        [NonSerialized]
        public ExtraBoneInfo                m_ExtraBoneInfo             = new ExtraBoneInfo();

        public Dictionary<string, bool>     m_GenerateAnims             = new Dictionary<string, bool>();       // 所有解析出的动画
        [NonSerialized]
        private List<string>                m_GenerateAnimNames         = new List<string>();
        [NonSerialized]
        private List<bool>                  m_GenerateAnimSelectables   = new List<bool>();

        public void OnBeforeSerialize()
        {
            m_ExtraBoneNames.Clear();
            m_ExtraBoneSelectables.Clear();
            foreach (var kvp in m_SelectExtraBone)
            {
                m_ExtraBoneNames.Add(kvp.Key);
                m_ExtraBoneSelectables.Add(kvp.Value);
            }

            m_GenerateAnimNames.Clear();
            m_GenerateAnimSelectables.Clear();
            foreach (var kvp in m_GenerateAnims)
            {
                m_GenerateAnimNames.Add(kvp.Key);
                m_GenerateAnimSelectables.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            m_SelectExtraBone = new Dictionary<string, bool>();
            int count = Math.Min(m_ExtraBoneNames.Count, m_ExtraBoneSelectables.Count);
            for (int i = 0; i != count; ++i)
            {
                m_SelectExtraBone.Add(m_ExtraBoneNames[i], m_ExtraBoneSelectables[i]);
            }

            m_GenerateAnims = new Dictionary<string, bool>();
            count = Math.Min(m_GenerateAnimNames.Count, m_GenerateAnimSelectables.Count);
            for (int i = 0; i != count; ++i)
            {
                m_GenerateAnims.Add(m_GenerateAnimNames[i], m_GenerateAnimSelectables[i]);
            }
        }
    }
}
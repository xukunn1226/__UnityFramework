﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[MenuItem("Tools/Apply PrefabInstance")]
//static private void TestPrefab()
//{
//    GameObject parent = PrefabUtility.GetOutermostPrefabInstanceRoot(Selection.activeObject);
//    if (parent != null)
//        Selection.activeObject = parent;

//    //PrefabUtility.ApplyPrefabInstance(Selection.activeGameObject, InteractionMode.UserAction);
//}

namespace Framework.MeshParticleSystem
{
    public class FX_Root : MonoBehaviour
    {
#if UNITY_2019_1_OR_NEWER
        [Min(0)]
#endif
        public float            LifeTime;
        private float           m_LifeTime;

        private FX_Component[]  m_Components;
        private bool            m_bInit;

        private FX_Component[] Components
        {
            get
            {
                if (!m_bInit)
                {
                    m_Components = GetComponentsInChildren<FX_Component>(true);
                    m_bInit = true;
                }
                return m_Components;
            }
        }

        private void OnEnable()
        {
            m_LifeTime = LifeTime;
        }

        void Update()
        {
            if (LifeTime <= 0) { return; }

            m_LifeTime -= Time.deltaTime;

            if (m_LifeTime <= 0)
            {
                GameObject.Destroy(gameObject);
            }
        }

        /// <summary>
        /// 特效被回收重新使用时的接口
        /// 重置特效所有状态(FX_Component, ParticleSystem, TrailRenderer)
        /// </summary>
        public void Replay()
        {
            FX_Component[] comps = Components;
            if (comps == null)
                return;

            foreach(var comp in comps)
            {
                IReplay rp = comp as IReplay;
                if(rp != null)
                {
                    rp.Replay();
                }
            }
        }
    }
}
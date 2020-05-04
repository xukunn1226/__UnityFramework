using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.MeshParticleSystem;
using Framework.Core;

namespace Tests
{
    public class TestFX_Root_LRU : MonoBehaviour
    {
        public LoaderType m_Type;

        [SoftObject]
        public SoftObject m_FX1;

        [SoftObject]
        public SoftObject m_FX2;

        [SoftObject]
        public SoftObject m_FX3;

        private void Awake()
        {
            AssetManager.Init(m_Type);
        }

        void OnDestroy()
        {
            AssetManager.Uninit();
        }


        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 120, 60), "Load FX1"))
            {
                StartTask(m_FX1);
            }

            if (GUI.Button(new Rect(240, 100, 120, 60), "Return To Pool"))
            {
                ReturnToPool();
            }

            if (GUI.Button(new Rect(100, 200, 120, 60), "Load FX2"))
            {
                StartTask(m_FX2);
            }

            if (GUI.Button(new Rect(240, 200, 120, 60), "Return To Pool"))
            {
                ReturnToPool();
            }

            if (GUI.Button(new Rect(100, 300, 120, 60), "Load FX3"))
            {
                StartTask(m_FX3);
            }

            if (GUI.Button(new Rect(240, 300, 120, 60), "Return To Pool"))
            {
                ReturnToPool();
            }

            //if (GUI.Button(new Rect(100, 300, 180, 80), "Unload"))
            //{
            //    EndTask(m_FX1);
            //}
        }

        private void StartTask(SoftObject so)
        {
            if (!SoftObject.IsValid(so))
                return;

            FX_Root obj = (FX_Root)so.SpawnFromPrefabedPool();
            if (obj == null)
                return;

            obj.transform.position = Random.insideUnitSphere * 3;
        }

        private void ReturnToPool()
        {

        }

        private void EndTask(SoftObject so)
        {
            so.DestroyPrefabedPool();
        }
    }
}
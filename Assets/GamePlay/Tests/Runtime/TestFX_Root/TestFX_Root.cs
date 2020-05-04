using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.MeshParticleSystem;
using Framework.Core;

namespace Tests
{
    public class TestFX_Root : MonoBehaviour
    {
        public LoaderType m_Type;

        [SoftObject]
        public SoftObject m_FXPoolAsset;

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
            if (GUI.Button(new Rect(100, 100, 180, 80), "Load"))
            {
                StartTask();
            }

            //if (GUI.Button(new Rect(100, 200, 180, 80), "Return To Pool"))
            //{
            //    ReturnToPool();
            //}

            if (GUI.Button(new Rect(100, 300, 180, 80), "Unload"))
            {
                EndTask();
            }
        }

        private void StartTask()
        {
            if (!SoftObject.IsValid(m_FXPoolAsset))
                return;

            FX_Root obj = (FX_Root)m_FXPoolAsset.SpawnFromPrefabedPool();
            if (obj == null)
                return;

            obj.transform.position = Random.insideUnitSphere * 3;
        }

        //private void ReturnToPool()
        //{

        //}

        private void EndTask()
        {
            m_FXPoolAsset.DestroyPrefabedPool();
        }
    }
}
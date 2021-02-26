using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.Core;
using MeshParticleSystem;

namespace Application.Runtime.Tests
{
    public class TestFX_Root_LRU : MonoBehaviour
    {
        public LoaderType m_Type;

        [SoftObject]
        public SoftObject m_FXAsset1;
        private FX_Root m_FX1;

        [SoftObject]
        public SoftObject m_FXAsset2;
        private FX_Root m_FX2;

        [SoftObject]
        public SoftObject m_FXAsset3;
        private FX_Root m_FX3;

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
                m_FX1 = StartTask(m_FXAsset1);
            }

            if (GUI.Button(new Rect(240, 100, 120, 60), "Return To Pool"))
            {
                ReturnToPool(m_FX1);
            }

            if (GUI.Button(new Rect(380, 100, 120, 60), "Unload FX1"))
            {
                EndTask(m_FXAsset1);
            }

            if (GUI.Button(new Rect(100, 200, 120, 60), "Load FX2"))
            {
                m_FX2 = StartTask(m_FXAsset2);
            }

            if (GUI.Button(new Rect(240, 200, 120, 60), "Return To Pool"))
            {
                ReturnToPool(m_FX2);
            }

            if (GUI.Button(new Rect(380, 200, 120, 60), "Unload FX2"))
            {
                EndTask(m_FXAsset2);
            }

            if (GUI.Button(new Rect(100, 300, 120, 60), "Load FX3"))
            {
                m_FX3 = StartTask(m_FXAsset3);
            }

            if (GUI.Button(new Rect(240, 300, 120, 60), "Return To Pool"))
            {
                ReturnToPool(m_FX3);
            }

            if (GUI.Button(new Rect(380, 300, 120, 60), "Unload FX3"))
            {
                EndTask(m_FXAsset3);
            }
        }

        private FX_Root StartTask(SoftObject so)
        {
            if (!SoftObject.IsValid(so))
                return null;

            FX_Root obj = (FX_Root)so.SpawnFromLRUPool();
            if (obj == null)
                return null;

            obj.transform.position = Random.insideUnitSphere * 3;
            return obj;
        }

        private void ReturnToPool(FX_Root fx)
        {
            fx.ReturnToPool();
        }

        private void EndTask(SoftObject so)
        {
            so.DestroyLRUedPool();
        }
    }
}
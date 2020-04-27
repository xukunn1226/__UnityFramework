using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.Core;
using Framework.Cache;

namespace Tests
{
    public class TestPrefabSoftObjectUsePool : MonoBehaviour
    {
        public LoaderType type;

        GameObject inst;
        string info;

        [SoftObject]
        public SoftObject m_SoftObject;

        private MonoPoolBase m_Pool;

        private void Awake()
        {
            AssetManager.Init(type);
        }

        void OnDestroy()
        {
            AssetManager.Uninit();
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 200, 80), "Load"))
            {
                StartTask();
            }

            if (GUI.Button(new Rect(100, 280, 200, 80), "Unload"))
            {
                EndTask();
            }

            if (!string.IsNullOrEmpty(info))
            {
                GUI.Label(new Rect(100, 600, 500, 100), info);
            }
        }

        void StartTask()
        {
            if (m_SoftObject == null)
                return;

            //inst = m_SoftObject.Instantiate();

            TestPooledObject obj = (TestPooledObject)m_SoftObject.SpawnFromPool<TestPooledObject, PrefabObjectPool>();
            obj.transform.position = Random.insideUnitSphere * 3;
            inst = obj.gameObject;
            
            info = inst != null ? "sucess to load: " : "fail to load: ";
            info += m_SoftObject.assetPath;
        }

        void EndTask()
        {
            if (inst != null)
            {
                m_SoftObject.DestroyPool<PrefabObjectPool>();
                inst = null;
            }
            info = null;
        }
    }
}
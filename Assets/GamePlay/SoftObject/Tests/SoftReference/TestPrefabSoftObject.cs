using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.Core;

namespace Tests
{
    public class TestPrefabSoftObject : MonoBehaviour
    {
        public LoaderType type;

        GameObject inst;
        string info;

        [SoftObject]
        public SoftObject m_SoftObject;

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
                //StartTask();
                StartCoroutine(StartTaskAsync());
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

            inst = m_SoftObject.Instantiate();

            info = inst != null ? "sucess to load: " : "fail to load: ";
            info += m_SoftObject.assetPath;
        }

        IEnumerator StartTaskAsync()
        {
            if (m_SoftObject == null)
                yield break;

            yield return m_SoftObject.InstantiateAsync((go) =>
            {
                inst = go;
            });
            info = inst != null ? "sucess to load: " : "fail to load: ";
            info += m_SoftObject.assetPath;
        }

        void EndTask()
        {
            if (inst != null)
            {
                m_SoftObject.ReleaseInst(inst);
                inst = null;
            }
            info = null;
        }
    }
}
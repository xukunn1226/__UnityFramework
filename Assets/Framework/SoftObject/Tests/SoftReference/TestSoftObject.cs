﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.Core;

namespace Tests
{
    public class TestSoftObject : MonoBehaviour
    {
        GameObject inst;
        string info;

        [SoftObject]
        public SoftObject m_SoftObject;

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
            info += m_SoftObject.assetName;
        }

        IEnumerator StartTaskAsync()
        {
            if (m_SoftObject == null)
                yield break;

            Debug.Log($"{Time.time}");
            yield return YieldConsts.GetWaitForSeconds_03(0.84f);
            Debug.Log($"{Time.time}------");

            Debug.Log($"------ {Time.frameCount}");
            GameObjectLoaderAsync loaderAsync = m_SoftObject.InstantiateAsync();
            yield return loaderAsync;
            Debug.Log($"{Time.frameCount}   ------");
            inst = loaderAsync.asset;
            
            info = inst != null ? "sucess to load: " : "fail to load: ";
            info += m_SoftObject.assetName;
        }

        void EndTask()
        {
            if (inst != null)
            {
                m_SoftObject.ReleaseInst();
                inst = null;
            }
            info = null;
        }
    }
}
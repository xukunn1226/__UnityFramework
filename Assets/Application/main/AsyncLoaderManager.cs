using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using System;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    public class AsyncLoaderManager : SingletonMono<AsyncLoaderManager>
    {
        private Dictionary<string, Action<GameObject>> m_Requests = new Dictionary<string, Action<GameObject>>();
        private Coroutine m_Loop;

        void Start()
        {
            m_Loop = StartCoroutine(DoLoad());
        }

        protected override void OnDestroy()
        {
            StopCoroutine(m_Loop);
            base.OnDestroy();
        }

        IEnumerator DoLoad()
        {
            while(true)
            {
                foreach(var req in m_Requests)
                {
                    PrefabLoaderAsync loader = AssetManager.InstantiatePrefabAsync("assets/cube.prefab");

                }

                yield return StartCoroutine(AssetManager.InstantiatePrefabAsync("assets/cube.prefab"));
            }
        }

        private IEnumerator Load(string assetPath)
        {
            PrefabLoaderAsync loader = AssetManager.InstantiatePrefabAsync(assetPath);
            while(!loader.IsDone())
            {
                yield return null;
            }

            // finish
        }

        public void AsyncLoad(string assetPath, Action<GameObject> cb)
        {
            Debug.Assert(Instance != null);
            Debug.Assert(cb != null);

            Action<GameObject> actions;
            m_Requests.TryGetValue(assetPath, out actions);
            Delegate[] delegates = actions?.GetInvocationList();
            if(delegates == null)
            {
                actions = cb;                
            }
            else
            {
                actions += cb;
            }
            m_Requests[assetPath] = actions;
        }
    }
}
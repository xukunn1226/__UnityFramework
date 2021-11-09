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
        private Dictionary<string, Action<GameObject>> m_Dict = new Dictionary<string, Action<GameObject>>();

        void Update()
        {

        }

        IEnumerator Foo()
        {
            while(true)
            {
                yield return StartCoroutine(AssetManager.InstantiatePrefabAsync("assets/cube.prefab"));
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Framework.Core;

namespace Application.Runtime
{
    /// <summary>
    /// 负责初始核心组件，然后加载游戏场景
    /// <summary>
    public class Core : SingletonMono<Core>
    {
        public string   TheFirstGameSceneName;
        public string   ScenePath;
        public string   BundlePath;

        private List<GameObject> m_CachedPersistentGO = new List<GameObject>();

        IEnumerator Start()
        {
            GlobalConfigManager.Init(ResourceManager.Instance.loaderType == Framework.AssetManagement.Runtime.LoaderType.FromEditor);

            yield return null;

            // 子节点设置到顶层方便查看，记录下来用于销毁
            while(transform.childCount > 0)
            {
                Transform t = transform.GetChild(0);
                t.SetParent(null, false);
                DontDestroyOnLoad(t.gameObject);
                m_CachedPersistentGO.Add(t.gameObject);
            }

            StreamingLevelManager.LevelContext ctx = new StreamingLevelManager.LevelContext();
            ctx.sceneName = TheFirstGameSceneName;
            ctx.scenePath = ScenePath;
            ctx.additive = false;
            ctx.bundlePath = BundlePath;
            StreamingLevelManager.Instance.LoadAsync(ctx);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            GlobalConfigManager.FlushAll();
        }

        protected override void OnDestroy()
        {
            GlobalConfigManager.Uninit();

            foreach(var go in m_CachedPersistentGO)
            {
                Destroy(go);
            }
            m_CachedPersistentGO.Clear();

            base.OnDestroy();
        }
    }
}
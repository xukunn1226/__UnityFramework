using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Framework.Core;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    /// <summary>
    /// 负责初始核心组件，然后加载游戏场景
    /// <summary>
    public class Core : MonoBehaviour
    {
        public string   TheFirstGameSceneName;
        public string   ScenePath;
        public string   BundlePath;

        IEnumerator Start()
        {
            GlobalConfigManager.Init(AssetManager.Instance.loaderType == Framework.AssetManagement.Runtime.LoaderType.FromEditor);

            StreamingLevelManager.LevelContext ctx = new StreamingLevelManager.LevelContext();
            ctx.sceneName = TheFirstGameSceneName;
            ctx.scenePath = ScenePath;
            ctx.additive = false;
            ctx.bundlePath = BundlePath;
            StreamingLevelManager.Instance.LoadAsync(ctx);

            yield break;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            GlobalConfigManager.FlushAll();
        }

        void OnDestroy()
        {
            // SingletonMonoBase.DestroyAll();
        }
    }
}
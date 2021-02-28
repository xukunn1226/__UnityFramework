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

        IEnumerator Start()
        {
            yield return null;

            LevelManager.LevelContext ctx = new LevelManager.LevelContext();
            ctx.sceneName = TheFirstGameSceneName;
            ctx.scenePath = ScenePath;
            ctx.additive = false;
            ctx.bundlePath = BundlePath;
            LevelManager.Instance.LoadAsync(ctx);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    /// <summary>
    /// 重启游戏时过渡使用，负责删除核心组件，再重启launcher
    /// <summary>
    public class Empty : MonoBehaviour
    {
        IEnumerator Start()
        {
            if (Core.Instance == null)
                throw new System.Exception("Empty: Core.Instance == null");

            if (Launcher.Instance == null)
                throw new System.Exception("Empty: Launcher.Instance == null");

            // 删除核心组件
            Destroy(Core.Instance.gameObject);

            yield return null;      // 等待core.OnDestroy()的调用

            Resources.UnloadUnusedAssets();

            // 重启launcher流程
            Launcher.Instance.Restart();
        }
    }
}
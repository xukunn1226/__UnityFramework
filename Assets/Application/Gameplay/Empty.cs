using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Application.Runtime
{
    /// <summary>
    /// 重启游戏时过渡使用，负责删除核心组件，再重启launcher
    /// <summary>
    public class Empty : MonoBehaviour
    {
        IEnumerator Start()
        {
            // if (Core.Instance == null)
            //     throw new System.Exception("Empty: Core.Instance == null");

            if (Launcher.Instance == null)
                throw new System.Exception("Empty: Launcher.Instance == null");

            // 删除核心组件
            // Destroy(Core.Instance.gameObject);


            // 等待所有单件的删除
            yield return null;
            yield return null;
            yield return null;

            Resources.UnloadUnusedAssets();
            System.GC.Collect();

            // 重启launcher流程
            Launcher.Instance.Restart();
        }
    }
}
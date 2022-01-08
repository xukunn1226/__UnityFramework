using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;

namespace Application.Runtime
{
    /// <summary>
    /// 重启游戏时过渡使用，再重启launcher
    /// <summary>
    public class Empty : MonoBehaviour
    {
        IEnumerator Start()
        {
            if (Launcher.Instance == null)
                throw new System.Exception("Empty: Launcher.Instance == null");

            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            
            // 重启前删除所有单件
            SingletonMonoBase.DestroyAll();

            yield return null;
            yield return null;
            yield return null;

            SingletonMonoBase.Work();

            // 重启launcher流程
            Launcher.Instance.Restart();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Empty : MonoBehaviour
{
    void Start()
    {
        if(Core.Instance == null)
            throw new System.Exception("Empty: Core.Instance == null");

        if(Launcher.Instance == null)
            throw new System.Exception("Empty: Launcher.Instance == null");

        // 删除核心组件
        Destroy(Core.Instance.gameObject);

        // 重启launcher流程
        Launcher.Instance.Restart();
    }
}

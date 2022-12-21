using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

public class AM_Startup : MonoBehaviour
{
    AssetOperationHandle m_Op1;
    AssetOperationHandle m_Op2;

    IEnumerator Start()
    {
        yield return AssetManagerEx.Initialize();
        Debug.Log("Init succeed!");
    }

    private void OnDestroy()
    {
        AssetManagerEx.Destroy();
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(100, 100, 120, 60), "TestCase1"))
        {
            TestCase1_LoadAsset();
        }
        if(GUI.Button(new Rect(300, 100, 120, 60), "Release"))
        {
            TestCase1_Release();
        }


        if (GUI.Button(new Rect(100, 200, 120, 60), "TestCase2"))
        {
            TestCase2_LoadAssetAsync();
        }
        if (GUI.Button(new Rect(300, 200, 120, 60), "Release"))
        {
            TestCase2_Release();
        }
    }

    /// /////////////////////////////////////// 测试同步、异步加载
    private void TestCase1_LoadAsset()
    {
        m_Op1 = AssetManagerEx.LoadAsset<Texture2D>("Assets/Res/Checker.png");
        m_Op1.Completed += OnCompleted_TestCase1;
    }

    private void TestCase1_Release()
    {
        m_Op1.Release();
        m_Op1.Release();
    }

    private void OnCompleted_TestCase1(AssetOperationHandle op)
    {
        if(op.assetObject != null)
        {
            Debug.Log($"Succeed to load {op.assetObject.name}");
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetObject.name}");
        }
    }



    /// /////////////////////////////////////// 测试回调中创建或销毁句柄
    private void TestCase2_LoadAssetAsync()
    {
        m_Op2 = AssetManagerEx.LoadAssetAsync<Texture2D>("Assets/Res/M_Building_Bar_01_01.prefab");
        m_Op2.Completed += OnCompleted_TestCase2;
    }

    private void TestCase2_Release()
    {
        m_Op2.Release();
        m_Op2.Release();
    }

    private void OnCompleted_TestCase2(AssetOperationHandle op)
    {
        // 回调中加载自身资源，EXCEPTION
        AssetManagerEx.LoadAssetAsync<Texture2D>("Assets/Res/M_Building_Bar_01_01.prefab");

        // 回调中加载其他资源，PASS
        //AssetManagerEx.LoadAssetAsync<Texture2D>("Assets/Res/T_Building_Bar_01_01_D.tga");


        if (op.assetObject != null)
        {
            Debug.Log($"Succeed to load {op.assetObject.name}");
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }
}

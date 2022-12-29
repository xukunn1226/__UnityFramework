using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Framework.AssetManagement.Runtime;

public class AM_Startup : MonoBehaviour
{
    AssetOperationHandle m_Op1;
    AssetOperationHandle m_Op2;
    AssetOperationHandle m_Op31;
    AssetOperationHandle m_Op32;
    AssetOperationHandle m_Op4;
    AssetOperationHandle m_Op5;
    AssetOperationHandle m_Op61;
    AssetOperationHandle m_Op62;
    AssetOperationHandle m_Op71;
    AssetOperationHandle m_Op72;
    GameObject m_Instance1;
    GameObject m_Instance2;

    private EPlayMode GetPlayMode()
    {
#if UNITY_EDITOR
        Application.Runtime.LauncherMode mode = Application.Runtime.EditorLauncherMode.Mode();
        if (mode == Application.Runtime.LauncherMode.FromEditor)
            return EPlayMode.FromEditor;
        else if (mode == Application.Runtime.LauncherMode.FromStreamingAssets)
            return EPlayMode.FromStreaming;
        else
            return EPlayMode.FromHost;
#else
        return EPlayMode.FromStreaming;
#endif
    }

    IEnumerator Start()
    {
        InitializeParameters initializeParameters = new InitializeParameters()
        {
            PlayMode = GetPlayMode(),
            LocationToLower = false,
            AssetLoadingMaxNumber = int.MaxValue
        };
        yield return AssetManagerEx.Initialize(initializeParameters);
    }

    private void OnDestroy()
    {
        AssetManagerEx.Destroy();
    }

    private void OnGUI()
    {
        // test case1
        float y = 100;
        if(GUI.Button(new Rect(100, y, 120, 60), "TestCase1"))
        {
            TestCase1_LoadAssetAsync();
        }
        if(GUI.Button(new Rect(300, y, 120, 60), "Release"))
        {
            TestCase1_Release();
        }

        // test case2
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase2"))
        {
            TestCase2_InvokeException();
        }
        if (GUI.Button(new Rect(300, y, 120, 60), "Release"))
        {
            TestCase2_Release();
        }

        // test case3
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase3"))
        {
            TestCase3_SyncAndAsyncOneFrame();
        }
        if (GUI.Button(new Rect(300, y, 120, 60), "Release"))
        {
            TestCase3_Release();
        }

        // test case4
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase4"))
        {
            TestCase4_FailedToLoad();
        }
        if (GUI.Button(new Rect(300, y, 120, 60), "Release"))
        {
            TestCase4_Release();
        }

        // test case5
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase5"))
        {
            TestCase5_ReleaseImmediately();
        }
        if (GUI.Button(new Rect(300, y, 120, 60), "Release"))
        {
            TestCase5_Release();
        }

        // test case6
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase6_1"))
        {
            TestCase6_MultiLoad_1();
        }
        if (GUI.Button(new Rect(300, y, 120, 60), "Release_1"))
        {
            TestCase6_Release_1();
        }

        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase6_2"))
        {
            TestCase6_MultiLoad_2();
        }
        if (GUI.Button(new Rect(300, y, 120, 60), "Release_2"))
        {
            TestCase6_Release_2();
        }

        // test case7
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase7"))
        {
            TestCase7_LoadTheSameBundle();
        }
        if (GUI.Button(new Rect(300, y, 120, 60), "Release"))
        {
            TestCase7_Release();
        }
    }

    /// /////////////////////////////////////// 测试同步、异步加载
    private void TestCase1_LoadAssetAsync()
    {
        m_Op1 = AssetManagerEx.LoadAssetAsync<Texture2D>("assets/res/checker.png");
        //m_Op1.WaitForAsyncComplete();   // 测试异步变同步
        m_Op1.Completed += OnCompleted_TestCase1;
    }

    private void TestCase1_Release()
    {
        m_Op1.Release();
        m_Op1.Release();    // 测试重复释放
    }

    private void OnCompleted_TestCase1(AssetOperationHandle op)
    {
        if(op.assetObject != null)
        {
            Debug.Log($"Succeed to load {op.assetObject.name}");
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }



    /// /////////////////////////////////////// 测试回调中创建或销毁句柄
    private void TestCase2_InvokeException()
    {
        m_Op2 = AssetManagerEx.LoadAssetAsync<Texture2D>("assets/res/m_building_bar_01_01.prefab");
        m_Op2.Completed += OnCompleted_TestCase2;
    }

    private void TestCase2_Release()
    {
        m_Op2.Release();
        m_Op2.Release();
    }

    private void OnCompleted_TestCase2(AssetOperationHandle op)
    {
        // 回调中再次加载自己，PASS
        AssetManagerEx.LoadAssetAsync<Texture2D>("assets/res/m_building_bar_01_01.prefab");

        // 回调中释放自己，PASS
        //op.Release();

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




    /// /////////////////////////////////////// 测试同一帧同时同步及异步加载资源
    private void TestCase3_SyncAndAsyncOneFrame()
    {
        m_Op31 = AssetManagerEx.LoadAssetAsync<GameObject>("assets/res/m_building_bar_01_01.prefab");
        m_Op31.Completed += OnCompleted_TestCase3;

        m_Op32 = AssetManagerEx.LoadAsset<GameObject>("assets/res/m_building_bar_01_01.prefab");
        m_Op32.Completed += OnCompleted_TestCase3;
    }

    private void TestCase3_Release()
    {
        m_Op31.Release();
        m_Op32.Release();
    }

    private void OnCompleted_TestCase3(AssetOperationHandle op)
    {
        if (op.assetObject != null)
        {
            Debug.Log($"Succeed to load {op.assetObject.name}");
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }



    /// /////////////////////////////////////// 测试加载失败
    private void TestCase4_FailedToLoad()
    {
        m_Op4 = AssetManagerEx.LoadAssetAsync<Texture2D>("Assets/Res/Checker111111.png");
        m_Op4.Completed += OnCompleted_TestCase4;
    }

    private void TestCase4_Release()
    {
        m_Op4.Release();
    }

    private void OnCompleted_TestCase4(AssetOperationHandle op)
    {
        if (op.assetObject != null)
        {
            Debug.Log($"Succeed to load {op.assetObject.name}");
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }



    /// /////////////////////////////////////// 测试异步加载后立即释放
    private void TestCase5_ReleaseImmediately()
    {
        m_Op5 = AssetManagerEx.LoadAssetAsync<Texture2D>("assets/res/checker.png");
        m_Op5.Completed += OnCompleted_TestCase5;

        // 尚未加载完就释放，实际上会等资源加载完后再释放，且注册的回调已释放了，不会触发
        m_Op5.Release();
    }

    private void TestCase5_Release()
    {
        m_Op5.Release();
    }

    private void OnCompleted_TestCase5(AssetOperationHandle op)
    {
        if (op.assetObject != null)
        {
            Debug.Log($"Succeed to load {op.assetObject.name}");
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }



    /// /////////////////////////////////////// 测试：先后加载两个资源，他们有部分共同依赖的bundle
    private void TestCase6_MultiLoad_1()
    {
        m_Op61 = AssetManagerEx.LoadAssetAsync<GameObject>("assets/res/111/cube.prefab");
        m_Op61.Completed += OnCompleted_TestCase61;
    }

    private void TestCase6_Release_1()
    {
        Destroy(m_Instance1);
        m_Op61.Release();
    }

    private void OnCompleted_TestCase61(AssetOperationHandle op)
    {
        if (op.assetObject != null)
        {
            Debug.Log($"Succeed to load {op.assetObject.name}");
            m_Instance1 = op.Instantiate(Vector3.one, Quaternion.identity);
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }

    private void TestCase6_MultiLoad_2()
    {
        m_Op62 = AssetManagerEx.LoadAssetAsync<GameObject>("assets/res/222/sphere.prefab");
        m_Op62.Completed += OnCompleted_TestCase62;
    }

    private void TestCase6_Release_2()
    {
        Destroy(m_Instance2);
        m_Op62.Release();
    }

    private void OnCompleted_TestCase62(AssetOperationHandle op)
    {
        if (op.assetObject != null)
        {
            Debug.Log($"Succeed to load {op.assetObject.name}");
            m_Instance2 = op.Instantiate();
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }



    /// /////////////////////////////////////// 测试同一帧对同一个Bundle加载
    private void TestCase7_LoadTheSameBundle()
    {
        m_Op71 = AssetManagerEx.LoadAssetAsync<GameObject>("assets/res/222/sphere.prefab");
        m_Op71.Completed += OnCompleted_TestCase71;

        m_Op72 = AssetManagerEx.LoadAsset<GameObject>("assets/res/111/cube.prefab");
        m_Op72.Completed += OnCompleted_TestCase72;
    }

    private void TestCase7_Release()
    {
        Destroy(m_Instance1);
        Destroy(m_Instance2);
        m_Op71.Release();
        m_Op72.Release();
    }

    private void OnCompleted_TestCase71(AssetOperationHandle op)
    {
        if (op.assetObject != null)
        {
            Debug.Log($"Succeed to load {op.assetObject.name}");
            m_Instance1 = op.Instantiate(Vector3.one, Quaternion.identity);
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }

    private void OnCompleted_TestCase72(AssetOperationHandle op)
    {
        if (op.assetObject != null)
        {
            Debug.Log($"Succeed to load {op.assetObject.name}");
            m_Instance2 = op.Instantiate();
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }
}

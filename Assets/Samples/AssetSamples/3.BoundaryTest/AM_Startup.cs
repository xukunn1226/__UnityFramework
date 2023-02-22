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

    AssetOperationHandle m_Op8;
    GameObject m_Instance8;

    IEnumerator Start()
    {
        InitializeParameters initializeParameters = new InitializeParameters()
        {
            PlayMode = Application.Runtime.Launcher.GetPlayMode(),
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

        // test case8
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase8"))
        {
            StartCoroutine(TestCase8_LoadPrefabAsync());
        }
        if (GUI.Button(new Rect(300, y, 120, 60), "Release"))
        {
            TestCase8_Release();
        }
    }

    /// /////////////////////////////////////// ����ͬ�����첽����
    private void TestCase1_LoadAssetAsync()
    {
        m_Op1 = AssetManagerEx.LoadAssetAsync<Texture2D>("assets/res/checker.png");
        //m_Op1.WaitForAsyncComplete();   // �����첽��ͬ��
        m_Op1.Completed += OnCompleted_TestCase1;
    }

    private void TestCase1_Release()
    {
        m_Op1.Release();
        m_Op1.Release();    // �����ظ��ͷ�
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



    /// /////////////////////////////////////// ���Իص��д��������پ��
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
        // �ص����ٴμ����Լ���PASS
        AssetManagerEx.LoadAssetAsync<Texture2D>("assets/res/m_building_bar_01_01.prefab");

        // �ص����ͷ��Լ���PASS
        //op.Release();

        // �ص��м���������Դ��PASS
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




    /// /////////////////////////////////////// ����ͬһ֡ͬʱͬ�����첽������Դ
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



    /// /////////////////////////////////////// ���Լ���ʧ��
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



    /// /////////////////////////////////////// �����첽���غ������ͷ�
    private void TestCase5_ReleaseImmediately()
    {
        m_Op5 = AssetManagerEx.LoadAssetAsync<Texture2D>("assets/res/checker.png");
        m_Op5.Completed += OnCompleted_TestCase5;

        // ��δ��������ͷţ�ʵ���ϻ����Դ����������ͷţ���ע��Ļص����ͷ��ˣ����ᴥ��
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



    /// /////////////////////////////////////// ���ԣ��Ⱥ����������Դ�������в��ֹ�ͬ������bundle
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



    /// /////////////////////////////////////// ����ͬһ֡��ͬһ��Bundle����
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

    IEnumerator TestCase8_LoadPrefabAsync()
    {
        m_Op8 = AssetManagerEx.LoadAssetAsync<GameObject>("assets/res/m_building_bar_01_01.prefab");
        yield return m_Op8;
        
        // InstantiateOperation会自动回收，不需要持有
        var instOp = m_Op8.InstantiateAsync();
        yield return instOp;
        m_Instance8 = instOp.Result;
    }

    private void TestCase8_Release()
    {
        Destroy(m_Instance8);

        if(m_Op8 != null)
        {
            m_Op8.Release();
        }
    }
}

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

    IEnumerator Start()
    {
        yield return AssetManagerEx.Initialize();
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
    }

    /// /////////////////////////////////////// ����ͬ�����첽����
    private void TestCase1_LoadAssetAsync()
    {
        m_Op1 = AssetManagerEx.LoadAssetAsync<Texture2D>("assets/res/checker.png");
        m_Op1.WaitForAsyncComplete();   // �����첽��ͬ��
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
        // �ص����ٴμ����Լ���EXCEPTION
        //AssetManagerEx.LoadAssetAsync<Texture2D>("Assets/Res/M_Building_Bar_01_01.prefab");

        // �ص����ͷ��Լ���EXCEPTION
        op.Release();

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
        m_Op31 = AssetManagerEx.LoadAssetAsync<GameObject>("Assets/Res/M_Building_Bar_01_01.prefab");
        m_Op31.Completed += OnCompleted_TestCase3;

        m_Op32 = AssetManagerEx.LoadAsset<GameObject>("Assets/Res/M_Building_Bar_01_01.prefab");
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



    /// /////////////////////////////////////// ���������첽���غ������ͷ�
    private void TestCase5_ReleaseImmediately()
    {
        m_Op5 = AssetManagerEx.LoadAssetAsync<Texture2D>("Assets/Res/Checker.png");        
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



    /// /////////////////////////////////////// ����ͬһ֡��ͬһ��Bundle����
}

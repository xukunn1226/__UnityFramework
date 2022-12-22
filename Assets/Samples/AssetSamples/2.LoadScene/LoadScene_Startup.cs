using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Framework.AssetManagement.Runtime;

public class LoadScene_Startup : MonoBehaviour
{
    private SceneOperationHandle m_Op1;
    private SceneOperationHandle m_Op2;
    private UnloadSceneOperation m_UnloadOp1;
    private UnloadSceneOperation m_UnloadOp2;

    IEnumerator Start()
    {
        Object.DontDestroyOnLoad(gameObject);
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
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase1"))
        {
            TestCase1_LoadSceneAsync();
        }
        if (GUI.Button(new Rect(300, y, 120, 60), "Release"))
        {
            StartCoroutine(TestCase1_Release());
        }

        // test case2
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase2"))
        {
            TestCase2_LoadSceneAsync();
        }
        if (GUI.Button(new Rect(300, y, 120, 60), "Release"))
        {
            StartCoroutine(TestCase2_Release());
        }

    }

    /// /////////////////////////////////////// ≤‚ ‘≥°æ∞µƒº”‘ÿ°¢–∂‘ÿ(Single)
    private void TestCase1_LoadSceneAsync()
    {
        m_Op1 = AssetManagerEx.LoadSceneAsync("Assets/Samples/AssetSamples/2.LoadScene/TestScene1.unity", LoadSceneMode.Single, true);
        m_Op1.Completed += OnCompleted_TestCase1;
    }

    private IEnumerator TestCase1_Release()
    {
        m_UnloadOp1 = m_Op1.UnloadAsync();
        yield return m_UnloadOp1;
        Debug.Log($"Unload scene completed....");
    }

    private void OnCompleted_TestCase1(SceneOperationHandle op)
    {
        if (op.status == EOperationStatus.Succeed)
        {
            Debug.Log($"Succeed to load {op.sceneObject.name}");
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }

    /// /////////////////////////////////////// ≤‚ ‘≥°æ∞µƒº”‘ÿ°¢–∂‘ÿ(Additive)
    private void TestCase2_LoadSceneAsync()
    {
        m_Op2 = AssetManagerEx.LoadSceneAsync("Assets/Samples/AssetSamples/2.LoadScene/TestScene2.unity", LoadSceneMode.Additive, true);
        m_Op2.Completed += OnCompleted_TestCase2;
    }

    private IEnumerator TestCase2_Release()
    {
        m_UnloadOp2 = m_Op2.UnloadAsync();
        yield return m_UnloadOp2;
        Debug.Log($"Unload scene completed....");
    }

    private void OnCompleted_TestCase2(SceneOperationHandle op)
    {
        if (op.status == EOperationStatus.Succeed)
        {
            Debug.Log($"Succeed to load {op.sceneObject.name}");
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }
}

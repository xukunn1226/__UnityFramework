using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

public class Regular_Startup : MonoBehaviour
{    
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
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase1"))
        {
            TestCase1_LoadAssetAsync();
        }
        
        // test case2
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase2"))
        {
            StartCoroutine(TestCase2_LoadAssetAsync());
        }

        // test case3
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase3"))
        {
            StartCoroutine(TestCase3_InstancePrefabAsync());
        }

        // test case4
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase4"))
        {
            TestCase4_InstancePrefabAsync();
        }

        // test case5
        y += 100;
        if (GUI.Button(new Rect(100, y, 120, 60), "TestCase5"))
        {
            TestCase5_LoadSubAssetsAsync();
        }
    }

    // 委托方式
    private void TestCase1_LoadAssetAsync()
    {
        var op = AssetManagerEx.LoadAssetAsync<Texture2D>("assets/res/checker.png");
        op.Completed += OnCompleted_TestCase1;

        //op.Release();
    }

    private void OnCompleted_TestCase1(AssetOperationHandle op)
    {
        if (op.status == EOperationStatus.Succeed)
        {
            Texture2D tex = op.assetObject as Texture2D;
            Debug.Log($"Succeed to load {tex.name}");
        }
        else
        {
            Debug.LogError($"Failed to load {op.assetInfo.assetPath}");
        }
    }

    // 协程方式
    private IEnumerator TestCase2_LoadAssetAsync()
    {
        var op = AssetManagerEx.LoadAssetAsync<Texture2D>("assets/res/checker.png");
        yield return op;
        Texture2D tex = op.assetObject as Texture2D;

        //op.Release();
    }

    // 加载Prefab
    private IEnumerator TestCase3_InstancePrefabAsync()
    {
        var op = AssetManagerEx.LoadAssetAsync<GameObject>("assets/res/m_building_bar_01_01.prefab");
        yield return op;
        GameObject go = op.Instantiate(Vector3.zero, Quaternion.identity, null);


        //Object.Destroy(go);
        //op.Release();
    }

    // Task加载方式
    async void TestCase4_InstancePrefabAsync()
    {
        var op = AssetManagerEx.LoadAssetAsync<GameObject>("assets/res/m_building_bar_01_01.prefab");
        await op.Task;
        GameObject go = op.Instantiate(Vector3.zero, Quaternion.identity, null);


        //Object.Destroy(go);
        //op.Release();
    }

    async void TestCase5_LoadSubAssetsAsync()
    {
        var op = AssetManagerEx.LoadSubAssetsAsync<Sprite>("assets/res/ui/atlases/icon0/sprites/ui_banner_wood.png");
        await op.Task;
        Sprite s = op.GetSubAssetObject<Sprite>("UI_Banner_Wood");
        Debug.Log($"{s.name}");


        //Object.Destroy(go);
        //op.Release();
    }
}

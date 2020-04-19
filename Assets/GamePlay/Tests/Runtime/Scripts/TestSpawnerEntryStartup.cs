using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Framework.AssetManagement.Runtime;

public class TestSpawnerEntryStartup : MonoBehaviour
{
    private void Awake()
    {
        ResourceManager.Init(LoaderType.FromAB);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        ResourceManager.Uninit();
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(100, 100, 120, 80), "Load Scene"))
        {
            //ResourceManager.LoadAssetBundle("assets/res/testspawnerring.ab");
            AssetBundleLoader abLoader = ResourceManager.LoadAssetBundle("assets/res/scene1.ab");
            abLoader.assetBundle.GetAllScenePaths();

            UnityEngine.SceneManagement.SceneManager.LoadScene("TestSpawnerRing");
        }
    }
}

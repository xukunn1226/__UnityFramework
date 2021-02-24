using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Framework.AssetManagement.Runtime;

public class Core : MonoBehaviour
{
    public string               SceneBundlePath;
    public string               TheFirstGameSceneName;
    private SceneLoaderAsync    m_SceneLoader;

    static public Core Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if (FindObjectsOfType<Core>().Length > 1)
        {
            DestroyImmediate(this);
            throw new Exception("Core has already exist...");
        }

        gameObject.name = "[Core]";

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    IEnumerator Start()
    {
        yield return null;

        // m_SceneLoader = ResourceManager.LoadSceneAsync(SceneBundlePath, TheFirstGameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}

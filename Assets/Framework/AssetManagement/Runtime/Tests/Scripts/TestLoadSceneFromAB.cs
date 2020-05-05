using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime.Tests
{
    /// <summary>
    /// “动态场景”加载（同步、异步）测试用例        
    /// 1、无需加入Build Settings
    /// 2、调用LoadScene之前必须把场景所在AB及依赖AB先载入
    /// 3、调用方式：LoadScene(sceneName)  OR  LoadScene(scenePath)，前者不带后缀名，后者带后缀名
    /// 4、sceneName or scenePath大小写敏感
    /// </summary>
    public class TestLoadSceneFromAB : MonoBehaviour
    {
        private const string        m_SceneBundlePath = "assets/framework/assetmanagement/runtime/tests/res/scenes.ab";
        private SceneLoader         m_SceneLoader1;
        private SceneLoader         m_SceneLoader2;

        private SceneLoaderAsync    m_SceneLoaderAsync1;
        private SceneLoaderAsync    m_SceneLoaderAsync2;

        private SceneLoaderAsync    m_LoaderAsync;

        private void Start()
        {
            // 场景的加载、卸载完成都已回调为准
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            DontDestroyOnLoad(gameObject);
        }

        void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            Debug.Log($"OnActiveSceneChanged: [{Time.frameCount}]    oldScene [{oldScene.name}]    newScene [{newScene.name}]");
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"OnSceneLoaded: [{Time.frameCount}]    Scene [{scene.name}]   Mode [{mode}]");
        }

        void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"OnSceneUnloaded: [{Time.frameCount}]    Scene [{scene.name}]");
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 150, 60), "Load Scene1"))
            {
                m_SceneLoader1 = LoadScene("Assets/Framework/AssetManagement/Runtime/Tests/Res/Scenes/TestScene1", LoadSceneMode.Additive);
            }

            if (GUI.Button(new Rect(300, 100, 150, 60), "Unload Scene1"))
            {
                StartCoroutine(UnloadSceneAsync(m_SceneLoader1));
            }

            if (GUI.Button(new Rect(100, 200, 150, 60), "Load Scene2"))
            {
                m_SceneLoader2 = LoadScene("TestScene2", LoadSceneMode.Additive);
            }

            if (GUI.Button(new Rect(300, 200, 150, 60), "Unload Scene2"))
            {
                StartCoroutine(UnloadSceneAsync(m_SceneLoader2));
            }

            ////////////////// Async Load Scene

            if (GUI.Button(new Rect(100, 300, 150, 60), "Load Scene1 Async"))
            {
                m_SceneLoaderAsync1 = LoadSceneAsync("TestScene1", LoadSceneMode.Additive);
                StartCoroutine(m_SceneLoaderAsync1);
            }

            if (GUI.Button(new Rect(300, 300, 150, 60), "Unload Scene1"))
            {
                StartCoroutine(UnloadSceneAsync(m_SceneLoaderAsync1));
            }

            if (GUI.Button(new Rect(100, 400, 150, 60), "Load Scene2 Async"))
            {
                m_SceneLoaderAsync2 = LoadSceneAsync("TestScene2", LoadSceneMode.Single);
                StartCoroutine(m_SceneLoaderAsync2);
            }

            if (GUI.Button(new Rect(300, 400, 150, 60), "Unload Scene2"))
            {
                StartCoroutine(UnloadSceneAsync(m_SceneLoaderAsync2));
            }

            //////////////////// Async Load Scene allow actived
            if (GUI.Button(new Rect(100, 500, 220, 60), "Load Scene1 Allow Activation"))
            {
                m_LoaderAsync = LoadSceneAsync("TestScene1", LoadSceneMode.Additive, false);
                StartCoroutine(m_LoaderAsync);
            }

            if(GUI.Button(new Rect(350, 500, 100, 60), "Continue"))
            {
                m_LoaderAsync.loadAsyncOp.allowSceneActivation = true;
            }
        }        

        SceneLoaderAsync LoadSceneAsync(string sceneName, LoadSceneMode mode, bool allowSceneActivation = true)
        {
            Debug.Log($"----Begin LoadSceneAsync: [{Time.frameCount}]");
            
            SceneLoaderAsync loader = AssetManager.LoadSceneAsync(m_SceneBundlePath, sceneName, mode, allowSceneActivation);
            
            Debug.Log($"----End LoadSceneAsync: [{Time.frameCount}]");

            return loader;
        }

        /// <summary>
        /// 下一帧加载完成，即触发回调OnSceneLoaded
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        SceneLoader LoadScene(string sceneName, LoadSceneMode mode)
        {
            Debug.Log($"----Begin LoadScene: [{Time.frameCount}]");
            SceneLoader loader = AssetManager.LoadScene(m_SceneBundlePath, sceneName, mode);
            Debug.Log($"----End LoadScene: [{Time.frameCount}]");

            return loader;
        }

        IEnumerator UnloadSceneAsync(SceneLoader loader)
        {
            Debug.Log($"----Begin UnloadSceneAsync: [{Time.frameCount}]");
            yield return AssetManager.UnloadSceneAsync(loader);
            Debug.Log($"----End UnloadSceneAsync: [{Time.frameCount}]");
        }

        IEnumerator UnloadSceneAsync(SceneLoaderAsync loader)
        {
            Debug.Log($"----Begin UnloadSceneAsync: [{Time.frameCount}]");
            yield return AssetManager.UnloadSceneAsync(loader);
            Debug.Log($"----End UnloadSceneAsync: [{Time.frameCount}]");
        }
    }
}
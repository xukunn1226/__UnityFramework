using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Framework.AssetManagement.Runtime.Tests
{
    /// <summary>
    /// “静态场景”加载（同步、异步）测试用例        
    /// 1、场景必须加入Build settings
    /// 2、不可热更
    /// 3、sceneName不带后缀名，scenePath带后缀名
    /// 4、sceneName, scenePath大小写不敏感
    /// 5、allowSceneActivation
    /// </summary>
    public class TestLoadScene : MonoBehaviour
    {
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
                m_SceneLoader1 = LoadScene("testscene1", LoadSceneMode.Additive);
            }

            if (GUI.Button(new Rect(300, 100, 150, 60), "Unload Scene1"))
            {
                StartCoroutine(UnloadSceneAsync(m_SceneLoader1));
            }

            if (GUI.Button(new Rect(100, 200, 150, 60), "Load Scene2"))
            {
                m_SceneLoader2 = LoadScene("testscene2", LoadSceneMode.Additive);
            }

            if (GUI.Button(new Rect(300, 200, 150, 60), "Unload Scene2"))
            {
                StartCoroutine(UnloadSceneAsync(m_SceneLoader2));
            }

            ////////////////// Async Load Scene

            if (GUI.Button(new Rect(100, 300, 150, 60), "Load Scene1 Async"))
            {
                m_SceneLoaderAsync1 = LoadSceneAsync("testscene1", LoadSceneMode.Additive);
                StartCoroutine(m_SceneLoaderAsync1);
            }

            if (GUI.Button(new Rect(300, 300, 150, 60), "Unload Scene1"))
            {
                StartCoroutine(UnloadSceneAsync(m_SceneLoaderAsync1));
            }

            if (GUI.Button(new Rect(100, 400, 150, 60), "Load Scene2 Async"))
            {
                m_SceneLoaderAsync2 = LoadSceneAsync("testscene2", LoadSceneMode.Additive);
                StartCoroutine(m_SceneLoaderAsync2);
            }

            if (GUI.Button(new Rect(300, 400, 150, 60), "Unload Scene2"))
            {
                StartCoroutine(UnloadSceneAsync(m_SceneLoaderAsync2));
            }

            //////////////////// Async Load Scene allow actived
            if (GUI.Button(new Rect(100, 500, 220, 60), "Load Scene1 Allow Activation"))
            {
                m_LoaderAsync = LoadSceneAsync("testscene1", LoadSceneMode.Additive, false);
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
            
            SceneLoaderAsync loader = AssetManager.LoadSceneAsync(sceneName, mode, allowSceneActivation);
            
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
            SceneLoader loader = AssetManager.LoadScene(sceneName, mode);
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
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
    public class TestLoadSceneFromAB : MonoBehaviour
    {
        private bool m_bContinue;

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
                LoadScene("TestScene1", LoadSceneMode.Additive);
            }

            if (GUI.Button(new Rect(300, 100, 150, 60), "Unload Scene1"))
            {
                StartCoroutine(UnloadSceneAsync("TestScene1"));
            }

            if (GUI.Button(new Rect(100, 200, 150, 60), "Load Scene2"))
            {
                LoadScene("TestScene2", LoadSceneMode.Additive);
            }

            if (GUI.Button(new Rect(300, 200, 150, 60), "Unload Scene2"))
            {
                StartCoroutine(UnloadSceneAsync("TestScene2"));
            }

            ////////////////// Async Load Scene

            if (GUI.Button(new Rect(100, 300, 150, 60), "Load Scene1 Async"))
            {
                StartCoroutine(LoadSceneAsync("TestScene1", LoadSceneMode.Additive));
            }

            if (GUI.Button(new Rect(300, 300, 150, 60), "Unload Scene1"))
            {
                StartCoroutine(UnloadSceneAsync("TestScene1"));
            }

            if (GUI.Button(new Rect(100, 400, 150, 60), "Load Scene2 Async"))
            {
                StartCoroutine(LoadSceneAsync("TestScene2", LoadSceneMode.Additive));
            }

            if (GUI.Button(new Rect(300, 400, 150, 60), "Unload Scene2"))
            {
                StartCoroutine(UnloadSceneAsync("TestScene2"));
            }

            //////////////////// Async Load Scene allow actived
            if (GUI.Button(new Rect(100, 500, 220, 60), "Load Scene1 Allow Activation"))
            {
                StartCoroutine(LoadSceneAsync_AllowActivation("TestScene1"));
            }

            if (GUI.Button(new Rect(350, 500, 100, 60), "Continue"))
            {
                m_bContinue = true;
            }
        }

        IEnumerator LoadSceneAsync_AllowActivation(string sceneName)
        {
            yield return null;

            //Begin to load the Scene you specify
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            //Don't let the Scene activate until you allow it to
            asyncOperation.allowSceneActivation = false;
            Debug.Log("Pro :" + asyncOperation.progress);
            //When the load is still in progress, output the Text and progress bar
            while (!asyncOperation.isDone)
            {
                //Output the current progress
                Debug.Log("Loading progress: " + (asyncOperation.progress * 100) + "%");

                // Check if the load has finished
                if (asyncOperation.progress >= 0.9f)
                {
                    //Wait to you press the space key to activate the Scene
                    if (m_bContinue)
                    {
                        //Activate the Scene
                        asyncOperation.allowSceneActivation = true;
                        m_bContinue = false;
                    }
                }

                yield return null;
            }
        }

        IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode mode)
        {
            Debug.Log($"----Begin LoadSceneAsync: [{Time.frameCount}]");
            //yield return AssetManager.LoadSceneAsync(sceneName, mode);

            AsyncOperation op = AssetManager.LoadSceneAsync(sceneName, mode);
            while (!op.isDone)
            {
                Debug.Log($"[{Time.frameCount}]  {op.progress}");
                yield return null;
            }

            Debug.Log($"----End LoadSceneAsync: [{Time.frameCount}]");
        }

        /// <summary>
        /// 下一帧加载完成，即触发回调OnSceneLoaded
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        void LoadScene(string sceneName, LoadSceneMode mode)
        {
            Debug.Log($"----Begin LoadScene: [{Time.frameCount}]");
            //AssetManager.LoadScene(sceneName, mode);
            AssetManager.LoadSceneFromBundle("assets/framework/assetmanagement/runtime/tests/res/scenes.ab", sceneName, mode);
            Debug.Log($"----End LoadScene: [{Time.frameCount}]");
        }

        IEnumerator UnloadSceneAsync(string sceneName)
        {
            Debug.Log($"----Begin UnloadSceneAsync: [{Time.frameCount}]");
            yield return AssetManager.UnloadSceneAsync(sceneName);
            Debug.Log($"----End UnloadSceneAsync: [{Time.frameCount}]");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime.Tests
{
    public class LoadSceneFromAB : MonoBehaviour
    {
        public LoaderType type = LoaderType.FromAB;

        AssetBundleLoader loader;
        AssetLoader<GameObject> cubeLoader;

        string info = "";

        private void Awake()
        {
            AssetManager.Init(type);
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnUnloadSceneLoaded;
        }

        void OnDestroy()
        {
            AssetManager.Uninit();
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 200, 80), "Load -- LoadSceneFromAB"))
            {
                StartCoroutine(StartTask());
            }

            if (GUI.Button(new Rect(100, 280, 200, 80), "Unload"))
            {
                EndTask();
            }

            if (!string.IsNullOrEmpty(info))
            {
                GUI.Label(new Rect(100, 600, 500, 100), info);
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("load scene: " + scene.name);
        }

        void OnUnloadSceneLoaded(Scene scene)
        {
            Debug.Log("-----Unload scene: " + scene.name);
        }
        IEnumerator StartTask()
        {
            //loader = AssetManager.LoadAssetBundle("scene.ab");
            ////AssetManager.LoadAssetBundle("texture.ab");
            ////AssetManager.LoadAssetBundle("material.ab");
            //string[] sceneNames = loader.assetBundle.GetAllScenePaths();
            //string sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneNames[0]);

            //cubeLoader = AssetManager.LoadAsset<GameObject>("texture/cube1.prefab");

            //SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            yield break;


            //AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneNames[0], LoadSceneMode.Additive);
            //op.allowSceneActivation = true;
            //yield return op;
        }

        void EndTask()
        {

        }
    }
}
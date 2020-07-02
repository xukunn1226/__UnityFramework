using System.Collections;
using UnityEngine;

namespace Framework.AssetManagement.Runtime.Tests
{
    public class LoadPrefabAsync : MonoBehaviour
    {
        public LoaderType type;

        public string assetPath;

        GameObject inst;
        string info;

        GameObjectLoaderAsync loaderAsync;

        private void Awake()
        {
            AssetManager.Init(type);
        }

        //IEnumerator Start()
        //{
        //    yield return StartCoroutine(StartTask());
        //}

        void OnDestroy()
        {
            AssetManager.Uninit();
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 200, 80), "Load"))
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

        IEnumerator StartTask()
        {
            // yield return AssetManager.InstantiatePrefabAsync(assetPath, (go) =>
            // {
            //     inst = go;
            // });

            Debug.Log($"{Time.frameCount}   Start..............");
            loaderAsync = AssetManager.InstantiatePrefabAsyncEx(assetPath);
            yield return loaderAsync;
            Debug.Log($"{Time.frameCount}   End..............");

            info = inst != null ? "sucess to load: " : "fail to load: ";
            info += assetPath;
        }

        void EndTask()
        {
            if(loaderAsync != null)
            {
                AssetManager.ReleaseGameObject(loaderAsync);
                loaderAsync = null;
            }

            if (inst != null)
            {
                Destroy(inst);
                inst = null;
            }
            info = null;
        }
    }
}
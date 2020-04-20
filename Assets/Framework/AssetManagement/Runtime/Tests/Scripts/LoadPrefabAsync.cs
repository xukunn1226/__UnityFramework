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

        private void Awake()
        {
            AssetManager.Init(type);
        }

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
            yield return AssetManager.InstantiatePrefabAsync(assetPath, (go) =>
            {
                inst = go;
            });

            info = inst != null ? "sucess to load: " : "fail to load: ";
            info += assetPath;
        }

        void EndTask()
        {
            if (inst != null)
            {
                Destroy(inst);
                inst = null;
            }
            info = null;
        }
    }
}
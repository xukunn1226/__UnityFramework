using System.Collections;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace MeshParticleSystem.Editor.Tests
{
    public class TestMeshParticle : MonoBehaviour
    {
        public LoaderType type;

        //public string           assetPath;

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
            if (GUI.Button(new Rect(100, 100, 200, 80), "LoadBillboard"))
            {
                StartTask("assets/res/ps/billboard.prefab");
            }

            if (GUI.Button(new Rect(100, 280, 200, 80), "LoadBillboard_PS"))
            {
                StartTask("assets/res/ps/billboard_ps.prefab");
            }

            if (!string.IsNullOrEmpty(info))
            {
                GUI.Label(new Rect(100, 600, 500, 100), info);
            }
        }

        void StartTask(string assetPath)
        {
            UnityEngine.Profiling.Profiler.BeginSample("11111111111");
            inst = AssetManager.InstantiatePrefab(assetPath);
            UnityEngine.Profiling.Profiler.EndSample();

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

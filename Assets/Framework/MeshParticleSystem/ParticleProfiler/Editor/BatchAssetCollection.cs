using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace MeshParticleSystem.Profiler
{
    // [CreateAssetMenuAttribute(menuName="Create Particle Batcher")]
    public class BatchAssetCollection : ScriptableObject
    {
        [SerializeField]
        public List<AssetProfilerData>      assetProfilerDataList       = new List<AssetProfilerData>();

        [SerializeField]
        public List<DirectoryProfilerData>  directoryProfilerDataList   = new List<DirectoryProfilerData>();

        public void Refresh()
        {
            for(int i = assetProfilerDataList.Count - 1; i >= 0; --i)
            {
                AssetProfilerData data = assetProfilerDataList[i];
                if(AssetDatabase.LoadAssetAtPath<GameObject>(data.assetPath) == null)
                {
                    assetProfilerDataList.RemoveAt(i);
                }
            }

            for(int i = directoryProfilerDataList.Count - 1; i >= 0; --i)
            {
                DirectoryProfilerData directoryData = directoryProfilerDataList[i];
                if(!AssetDatabase.IsValidFolder(directoryData.directoryPath))
                {
                    directoryProfilerDataList.RemoveAt(i);
                }
                else
                {
                    directoryProfilerDataList[i] = new DirectoryProfilerData(directoryData.directoryPath);
                }
            }
        }

        public DirectoryProfilerData GetInDirectoryList(string directoryPath)
        {
            int index = directoryProfilerDataList.FindIndex((item) => { return item.directoryPath == directoryPath.ToLower(); });
            return index == -1 ? null : directoryProfilerDataList[index];
        }

        private int FindInFileList(string assetPath)
        {
            return assetProfilerDataList.FindIndex((item) => { return item.assetPath == assetPath.ToLower(); });
        }

        private int FindInDirectoryList(string assetPath)
        {
            foreach(var directory in directoryProfilerDataList)
            {
                int index = directory.FindAsset(assetPath);
                if(index != -1)
                    return index;
            }
            return -1;
        }

        public void AddFile(string assetPath)
        {
            if(FindInFileList(assetPath) != -1 || FindInDirectoryList(assetPath) != -1)
                return;

            assetProfilerDataList.Add(new AssetProfilerData(assetPath));
        }

        public void RemoveFile(string assetPath)
        {
            int index = FindInFileList(assetPath);
            if(index != -1)
                assetProfilerDataList.RemoveAt(index);
        }

        public void AddDirectory(string directoryPath)
        {
            if(directoryProfilerDataList.FindIndex((item) => { return item.directoryPath == directoryPath.ToLower(); }) != -1)
                return;
            directoryProfilerDataList.Add(new DirectoryProfilerData(directoryPath));
        }

        public void RemoveDirectory(string directoryPath)
        {
            int index = directoryProfilerDataList.FindIndex((item) => { return item.directoryPath == directoryPath.ToLower(); });
            if(index != -1)
                directoryProfilerDataList.RemoveAt(index);
        }
    }

    [Serializable]
    public class AssetProfilerData
    {
        public string assetPath;
        public string filename;

        [HideInInspector]
        public bool pendingProfiling;
        [NonSerialized]
        public GameObject profilingGameObject;

        protected AssetProfilerData()
        {}

        public AssetProfilerData(string assetPath)
        {
            this.assetPath = assetPath.ToLower();
            filename = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        }

        public ParticleProfiler.ProfilerData    profilerData = new ParticleProfiler.ProfilerData();
        public ShowOverdraw.OverdrawData        overdrawData = new ShowOverdraw.OverdrawData();
    }

    [Serializable]
    public class DirectoryProfilerData
    {
        public string directoryPath;

        public List<AssetProfilerData> assetProfilerDataList = new List<AssetProfilerData>();

        protected DirectoryProfilerData()
        {}

        public DirectoryProfilerData(string directoryPath)
        {
            this.directoryPath = directoryPath.ToLower();

            if(!AssetDatabase.IsValidFolder(directoryPath))
            {
                Debug.LogError($"{directoryPath} is not valid folder");
                return;
            }
            
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { directoryPath });
            if(guids.Length == 0)
            {
                Debug.LogWarning($"Empty directory: {directoryPath}");
                return;
            }

            foreach(var guid in guids)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                if(asset != null && ValidParticle(asset))
                    assetProfilerDataList.Add(new AssetProfilerData(filePath));
            }
        }

        public int FindAsset(string assetPath)
        {
            return assetProfilerDataList.FindIndex(0, (item) => { return assetPath.ToLower() == item.assetPath; });
        }

        private bool ValidParticle(GameObject prefab)
        {
            if(prefab.GetComponentsInChildren<ParticleSystem>(true).Length == 0 &&
               prefab.GetComponentsInChildren<FX_Component>(true).Length == 0)
            {
                return false;
            }
            return true;
        }
    }
}
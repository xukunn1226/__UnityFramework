using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MeshParticleSystem.Profiler
{
    // [CreateAssetMenuAttribute(menuName="Create Particle Batcher")]
    public class BatchAssetCollection : ScriptableObject
    {
        public List<string>             assetPaths          = new List<string>();
        public List<string>             directoryPaths      = new List<string>();

        [SerializeField]
        public List<AssetPathsInDir>    dirToAssetsDic      = new List<AssetPathsInDir>();          // 记录directoryPaths下的所有资源路径

        [SerializeField]
        public List<AssetProfilerData>  profilerDataList    = new List<AssetProfilerData>();

#if UNITY_EDITOR
        public void AddDirectory(string directory)
        {
            if(!AssetDatabase.IsValidFolder(directory))
            {
                Debug.LogError($"{directory} is not valid folder");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { directory });
            if(guids.Length == 0)
            {
                Debug.LogWarning($"Empty directory: {directory}");
                return;
            }

            string dirLower = directory.ToLower();
            if(directoryPaths.IndexOf(dirLower) != -1)
                return;

            directoryPaths.Add(dirLower);

            List<string> paths = new List<string>();
            foreach(var guid in guids)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            dirToAssetsDic.Add(new AssetPathsInDir(dirLower, paths));
        }

        public void RemoveDirectory(string directory)
        {
            if(!AssetDatabase.IsValidFolder(directory))
            {
                Debug.LogError($"{directory} is not valid folder");
                return;
            }

            string dirLower = directory.ToLower();
            int index = directoryPaths.IndexOf(dirLower);
            if(index == -1)
                return;

            directoryPaths.RemoveAt(index);
            dirToAssetsDic.RemoveAt(index);
        }

        public void RefreshDirectory(string directory)
        {
            RemoveDirectory(directory);
            AddDirectory(directory);            
        }
#endif        
    }

    [Serializable]
    public class AssetPathsInDir
    {
        public string directory;
        public List<string> assetPaths;

        protected AssetPathsInDir() {}

        public AssetPathsInDir(string directory, List<string> assetPaths)
        {
            this.directory = directory;
            this.assetPaths = assetPaths;
        }
    }

    [Serializable]
    public class AssetProfilerData
    {
        public string assetPath;

        [NonSerialized]
        public ParticleProfiler.ProfilerData profilerData;
        [NonSerialized]
        public ShowOverdraw.OverdrawData overdrawData;
    }
}
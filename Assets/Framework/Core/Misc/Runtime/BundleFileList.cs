using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace Framework.Core
{
    public enum BundleFileState
    {
        NoExtracted,            // 未提取
        Extracting,             // 提取中
        ExtractingDone,         // 提取完成
    }

    [Serializable]
    public class BundleFileInfo
    {
        public string           BundleName;
        public string           AssetPath;                      // asset path relative to Assets/StreamingAssets
        public string           FileHash;
        public string           ContentHash;

        [NonSerialized]
        public FileStream       FileStream;

        [NonSerialized]
        public BundleFileState  State;
    }

    [Serializable]
    public class BundleFileList
    {
        public int Count;

        // public Dictionary<string, BundleFileInfo> FileList = new Dictionary<string, BundleFileInfo>();      // key: bundleName
        public List<BundleFileInfo> FileList = new List<BundleFileInfo>();

        public void Add(BundleFileInfo fileInfo)
        {
            FileList.Add(fileInfo);
            ++Count;
        }

        static public string SerializeToJson(BundleFileList data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }

        static public BundleFileList DeserializeFromJson(string json)
        {
            return JsonConvert.DeserializeObject<BundleFileList>(json);
        }
    }
}
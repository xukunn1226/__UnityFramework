using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Framework.Core
{
    [Serializable]
    public class BundleFileInfo
    {
        public string           BundleName;
        public string           AssetPath;                      // asset path relative to Assets/StreamingAssets
        public string           FileHash;
        public string           ContentHash;
    }

    [Serializable]
    public class BundleFileList
    {
        public int Count;

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
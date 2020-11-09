using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Framework.Core
{
    [Serializable]
    public class BundleFileInfo
    {
        public string Name;
        public string Hash;
    }

    [Serializable]
    public class BundleFileList
    {
        [SerializeField]
        public int Count;

        [SerializeField]
        public Dictionary<string, BundleFileInfo> FileList = new Dictionary<string, BundleFileInfo>();

        public void Add(string key, BundleFileInfo fileInfo)
        {
            FileList.Add(key, fileInfo);
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
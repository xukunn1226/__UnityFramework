using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetChecker
{
    public class AssetChecker
    {
        public class AssetFilterInfo
        {
            public bool enabled;
            public IAssetFilter filter;

            static public AssetFilterInfo Create<T>() where T : IAssetFilter, new()
            {
                AssetFilterInfo info = new AssetFilterInfo();
                info.enabled = true;
                info.filter = new T();
                return info;
            }
        }
        public string               Desc;
        public AssetFilterInfo      PathFilter;
        public AssetFilterInfo      FilenameFilter;
        public IAssetProcessor      Processor;

        public AssetChecker()
        {
            PathFilter = AssetFilterInfo.Create<AssetFilter_Path>();
            FilenameFilter = AssetFilterInfo.Create<AssetFilter_Filename>();
        }

        /// <summary>
        /// ִ�й�����
        /// </summary>
        /// <returns>����ִ�й�������Ľ����Ϣ</returns>
        public List<string> DoFilter()
        {
            List<string> paths = new List<string>();
            if (PathFilter != null && PathFilter.enabled)
            {
                paths = PathFilter.filter.DoFilter();
            }

            if (FilenameFilter != null && FilenameFilter.enabled)
            {
                AssetFilter_Filename filter_Filename = (AssetFilter_Filename)FilenameFilter.filter;
                if (filter_Filename.input != null && filter_Filename.input.Count == 0)
                {
                    // ����ʹ��filter���õ�input����
                    filter_Filename.input = paths;
                }
                paths = filter_Filename.DoFilter();
            }
            return paths;
        }

        /// <summary>
        /// ִ�д�����
        /// </summary>
        /// <returns>����ִ�д����������Ϣ</returns>
        /// <exception cref="Exception"></exception>
        public List<string> DoProcessor()
        {
            if (Processor == null)
                throw new Exception($"EMPTY Asset Processor");

            List<string> paths = DoFilter();

            List<string> results = new List<string>();
            foreach (var path in paths)
            {
                string result = Processor.DoProcess(path);
                if (!string.IsNullOrEmpty(result))
                {
                    results.Add(result);
                }
            }
            return results;
        }

        static public string Serialize(AssetChecker checker)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            return JsonConvert.SerializeObject(checker, Formatting.Indented, settings);
        }

        static public AssetChecker Deserialize(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            return JsonConvert.DeserializeObject<AssetChecker>(json, settings);
        }
    }
}
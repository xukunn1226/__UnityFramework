using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Framework.Core
{
    /// <summary>
    /// 记录历史版本到BaseVersion的所有diff文件hash
    /// </summary>
    public class DiffCollection
    {
        public string                       BaseVersion;
        public Dictionary<string, string>   VersionHashMap = new Dictionary<string, string>();        // key: version; value: diff config file hash

        public string GetDiffFileHash(string version)
        {
            if (version == null)
                return null;
            string hash;
            VersionHashMap.TryGetValue(version, out hash);
            return hash;
        }

        static public void Serialize(string assetPath, DiffCollection diffCollection)
        {
            string json = JsonConvert.SerializeObject(diffCollection, Formatting.Indented);

            System.IO.FileStream fs = new System.IO.FileStream(assetPath, System.IO.FileMode.Create);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
            fs.Write(data, 0, data.Length);
            fs.Close();
            fs.Dispose();
        }

        static public DiffCollection Deserialize(string assetPath)
        {
            if (!System.IO.File.Exists(assetPath))
            {
                Debug.LogError($"{assetPath} is not exists");
                return null;
            }

            System.IO.FileStream fs = new System.IO.FileStream(assetPath, System.IO.FileMode.Open);
            byte[] array = new byte[1024 * 128];
            int size = fs.Read(array, 0, 1024 * 128);
            fs.Close();
            fs.Dispose();

            return JsonConvert.DeserializeObject<DiffCollection>(System.Text.Encoding.UTF8.GetString(array, 0, size));
        }
    }
}
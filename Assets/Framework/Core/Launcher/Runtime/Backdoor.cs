using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Framework.Core
{
    public class Backdoor
    {
        public string                       MinVersion;             // 强更版本号，且不能向下兼容，只能是三位
        public string                       CurVersion;             // 当前版本号，可能是三位或四位
        public Dictionary<string, string>   VersionHistory;         // [version, diffcollection json's hash]

        public string GetDiffCollectionFileHash(string version)
        {
            if (VersionHistory == null)
                return null;

            string hash;
            VersionHistory.TryGetValue(version, out hash);
            return hash;
        }

        static public void Serialize(string assetPath, Backdoor bd)
        {
            string json = JsonConvert.SerializeObject(bd, Formatting.Indented);

            System.IO.FileStream fs = new System.IO.FileStream(assetPath, System.IO.FileMode.Create);
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(json);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
            fs.Dispose();
        }

        static public Backdoor Deserialize(string assetPath)
        {
            if(!System.IO.File.Exists(assetPath))
            {
                Debug.LogError($"{assetPath} is not exists");
                return null;
            }

            System.IO.FileStream fs = new System.IO.FileStream(assetPath, System.IO.FileMode.Open);
            byte[] array = new byte[1024 * 32];
            int size = fs.Read(array, 0, 1024 * 32);
            fs.Close();
            fs.Dispose();
            
            return JsonConvert.DeserializeObject<Backdoor>(System.Text.Encoding.UTF8.GetString(array, 0, size));
        }
    }
}
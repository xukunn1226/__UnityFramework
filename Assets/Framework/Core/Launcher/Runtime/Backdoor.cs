using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Framework.Core
{
    public class Backdoor
    {
        public enum Platform
        {
            PF_Unsupport = -1,
            PF_Win64,
            PF_Android,
            PF_IOS,
            PF_Count
        }
        public string                       MinVersion;             // 强更版本号，且不能向下兼容，只能是三位
        public string                       CurVersion;             // 当前版本号，可能是三位或四位
        public Dictionary<string, string>   VersionHistory_Win64    = new Dictionary<string, string>();         // [version, diffcollection json's hash]
        public Dictionary<string, string>   VersionHistory_Android  = new Dictionary<string, string>();
        public Dictionary<string, string>   VersionHistory_IOS      = new Dictionary<string, string>();

        public Dictionary<string, string> GetVersionHistory()
        {
            Platform platform = GetCurrentPlatform();
            if (platform == Platform.PF_Android)
                return VersionHistory_Android;
            if (platform == Platform.PF_IOS)
                return VersionHistory_IOS;
            if (platform == Platform.PF_Win64)
                return VersionHistory_Win64;
            return null;
        }

        public string GetDiffCollectionFileHash(string version)
        {
            Platform platform = GetCurrentPlatform();
            Debug.Assert(platform != Platform.PF_Unsupport);

            string hash = null;
            switch (platform)
            {
                case Platform.PF_Android:
                    VersionHistory_Android.TryGetValue(version, out hash);
                    break;
                case Platform.PF_IOS:
                    VersionHistory_IOS.TryGetValue(version, out hash);
                    break;
                case Platform.PF_Win64:
                    VersionHistory_Win64.TryGetValue(version, out hash);
                    break;
            }
            return hash;
        }

        private Platform GetCurrentPlatform()
        {
            string name = Utility.GetPlatformName();
            switch(name)
            {
                case "android":
                    return Platform.PF_Android;
                case "ios":
                    return Platform.PF_IOS;
                case "windows":
                    return Platform.PF_Win64;
            }
            return Platform.PF_Unsupport;
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
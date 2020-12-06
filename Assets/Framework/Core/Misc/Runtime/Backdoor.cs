using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Framework.Core
{
    public class Backdoor
    {
        public string       MinVersion;
        public string       CurVersion;
        public List<string> VersionHistory;

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
            byte[] array = new byte[2048];
            int size = fs.Read(array, 0, 2048);
            fs.Close();
            fs.Dispose();
            
            return JsonConvert.DeserializeObject<Backdoor>(System.Text.Encoding.UTF8.GetString(array, 0, size));
        }
    }
}
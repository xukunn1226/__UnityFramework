using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Framework.Core
{
    public class Diff
    {
        public class DiffFileInfo
        {
            public string           BundleName;
            public string           FileHash;
            public long             Size;       // byte
        }

        public string               Desc;
        public List<DiffFileInfo>   DeletedFileList;
        public List<DiffFileInfo>   UpdatedFileList;
        public List<DiffFileInfo>   AddedFileList;

        static public void Serialize(string assetPath, Diff diff)
        {
            string json = JsonConvert.SerializeObject(diff, Formatting.Indented);

            System.IO.FileStream fs = new System.IO.FileStream(assetPath, System.IO.FileMode.Create);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
            fs.Write(data, 0, data.Length);
            fs.Close();
            fs.Dispose();
        }

        static public Diff Deserialize(string assetPath)
        {
            if (!System.IO.File.Exists(assetPath))
            {
                Debug.LogError($"{assetPath} is not exists");
                return null;
            }

            System.IO.FileStream fs = new System.IO.FileStream(assetPath, System.IO.FileMode.Open);
            byte[] array = new byte[2048];
            int size = fs.Read(array, 0, 2048);
            fs.Close();
            fs.Dispose();

            return JsonConvert.DeserializeObject<Diff>(System.Text.Encoding.UTF8.GetString(array, 0, size));
        }
    }
}
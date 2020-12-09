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
        public long                 Size;

        public List<DiffFileInfo>   AddedFileList = new List<DiffFileInfo>();
        public List<DiffFileInfo>   UpdatedFileList = new List<DiffFileInfo>();
        public List<DiffFileInfo>   DeletedFileList = new List<DiffFileInfo>();

        public void PushAddedFile(DiffFileInfo dfi)
        {
            AddedFileList.Add(dfi);
            Size += dfi.Size;
        }

        public void PushUpdatedFile(DiffFileInfo dfi)
        {
            UpdatedFileList.Add(dfi);
            Size += dfi.Size;
        }

        public void PushDeletedFile(DiffFileInfo dfi)
        {
            DeletedFileList.Add(dfi);
        }

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
            byte[] array = new byte[1024 * 32];
            int size = fs.Read(array, 0, 1024 * 32);
            fs.Close();
            fs.Dispose();

            return JsonConvert.DeserializeObject<Diff>(System.Text.Encoding.UTF8.GetString(array, 0, size));
        }
    }
}
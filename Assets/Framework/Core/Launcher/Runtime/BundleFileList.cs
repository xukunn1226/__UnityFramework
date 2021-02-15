using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;

namespace Framework.Core
{
    [Serializable]
    public class BundleFileInfo
    {
        public string           BundleName;
        public string           FileHash;
        public long             Size;
    }

    [Serializable]
    public class BundleFileList
    {
        public int  Count;
        public long TotalSize;

        public List<BundleFileInfo> FileList = new List<BundleFileInfo>();

        public void Add(BundleFileInfo fileInfo)
        {
            FileList.Add(fileInfo);
            ++Count;
            TotalSize += fileInfo.Size;
        }

        static public string SerializeToJson(BundleFileList data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }

        static public BundleFileList DeserializeFromJson(string json)
        {
            return JsonConvert.DeserializeObject<BundleFileList>(json);
        }

#if UNITY_EDITOR
        /// <summary>
        /// ��ָ���ļ�������FileList����������savedFile
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="savedFile"></param>
        /// <returns></returns>
        static public bool BuildBundleFileList(string directory, string savedFile)
        {
            string json = GenerateBundleFileListJson(directory);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError($"failed to generate json of bundle file list");
                return false;
            }

            try
            {
                string savedDirectory = Path.GetDirectoryName(savedFile);
                if (!Directory.Exists(savedDirectory))
                    Directory.CreateDirectory(savedDirectory);
                using (FileStream fs = new FileStream(savedFile, FileMode.Create))
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
                    fs.Write(data, 0, data.Length);
                }
            }
            catch(System.Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// ����ָ���ļ����������ļ���BundleFileInfo Json
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        static private string GenerateBundleFileListJson(string directory)
        {
            directory = directory.Replace('\\', '/').TrimEnd(new char[] { '/' });
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"{directory}");
            }

            BundleFileList fileList = new BundleFileList();
            DirectoryInfo di = new DirectoryInfo(directory);
            FileInfo[] fis = di.GetFiles("*", SearchOption.AllDirectories);
            foreach (var fi in fis)
            {
                if (!string.IsNullOrEmpty(fi.Extension) && string.Compare(fi.Extension, ".meta", true) == 0)
                {
                    continue;
                }

                string bundleName = fi.FullName.Replace('\\', '/').Substring(di.FullName.Replace('\\', '/').Length + 1);
                fileList.Add(
                    new BundleFileInfo() { BundleName = bundleName, FileHash = GetHash(fi), Size = fi.Length }
                    );
            }
            return SerializeToJson(fileList);
        }

        static private string GetHash(FileInfo fi)
        {
            string hash = null;
            try
            {
                FileStream fs = fi.Open(FileMode.Open);
                fs.Position = 0;
                hash = EasyMD5.Hash(fs);
                fs.Close();
                fs.Dispose();
            }
            catch (IOException e)
            {
                Debug.Log($"I/O Exception: {e.Message}");
            }
            catch (UnauthorizedAccessException e)
            {
                Debug.Log($"Access Exception: {e.Message}");
            }
            return hash;
        }
#endif
    }
}
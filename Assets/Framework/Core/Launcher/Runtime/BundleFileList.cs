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
        /// 把文件夹（directory）下数据生成BundleFileList，并保存至savedFile
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="savedFile"></param>
        /// <returns></returns>
        static public bool BuildBundleFileList(string directory, string subFolderName, string savedFile)
        {
            string json = GenerateBundleFileListJson(directory, subFolderName);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"failed to generate json of bundle file list of directory[{directory}]");
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

        // 构建base和raw data数据的FileList，raw data暂时不支持分包功能，默认随base一起发布到母包
        static public bool BuildBaseAndRawDataBundleFileList(string directory, string subFolderName, string savedFile)
        {
            // base data file list
            string json = GenerateBundleFileListJson(directory, subFolderName);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"failed to generate json of bundle file list of directory[{directory}]");
                return false;
            }

            // raw data file list
            string json2 = GenerateRawDataBundleFileListJson(directory);

            // push raw data into FileList
            if(!string.IsNullOrEmpty(json2))
            {
                BundleFileList bf1 = DeserializeFromJson(json);
                BundleFileList bf2 = DeserializeFromJson(json2);
                BundleFileList bf = new BundleFileList();
                foreach(var item in bf1.FileList)
                {
                    bf.Add(item);
                }
                foreach(var item in bf2.FileList)
                {
                    bf.Add(item);
                }
                json = SerializeToJson(bf);
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
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 生成文件夹（parentFolder/subFolderName）下所有数据的BundleFileList，并序列化为json
        /// </summary>
        /// <param name="parentFolder">Assets/StreamingAssets/windows</param>
        /// <param name="subFolderName">base</param>
        /// <returns></returns>
        static private string GenerateBundleFileListJson(string parentFolder, string subFolderName)
        {
            parentFolder = parentFolder.Replace('\\', '/').TrimEnd(new char[] { '/' });
            if (!Directory.Exists(parentFolder))
            {
                return null;
            }

            subFolderName = subFolderName.Replace('\\', '/').TrimEnd(new char[] { '/' });
            if(!Directory.Exists(parentFolder + "/" + subFolderName))
            {
                return null;
            }

            BundleFileList fileList = new BundleFileList();
            DirectoryInfo di = new DirectoryInfo(string.Format($"{parentFolder}/{subFolderName}").Trim('/'));
            FileInfo[] fis = di.GetFiles("*", SearchOption.AllDirectories);
            foreach (var fi in fis)
            {
                if (!string.IsNullOrEmpty(fi.Extension) && string.Compare(fi.Extension, ".meta", true) == 0)
                {
                    continue;
                }

                string bundleName = fi.FullName.Replace('\\', '/').Substring(di.FullName.Replace('\\', '/').Length + 1);
                fileList.Add(
                    new BundleFileInfo() { BundleName = string.Format($"{subFolderName}/{bundleName}").Trim('/'), FileHash = GetHash(fi), Size = fi.Length }
                    );
            }
            return SerializeToJson(fileList);
        }

        /// <summary>
        /// 构建文件夹下数据的FileList，不包括分包数据
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        static private string GenerateRawDataBundleFileListJson(string directory)
        {
            directory = directory.Replace('\\', '/').TrimEnd(new char[] { '/' });
            if (!Directory.Exists(directory))
            {
                return null;
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

                if (!IsRawData(directory, fi.FullName.Replace('\\', '/')))
                    continue;

                string bundleName = fi.FullName.Replace('\\', '/').Substring(di.FullName.Replace('\\', '/').Length + 1);
                fileList.Add(
                    new BundleFileInfo() { BundleName = bundleName, FileHash = GetHash(fi), Size = fi.Length }
                    );
            }

            return SerializeToJson(fileList);
        }

        static private bool IsRawData(string directory, string filePath)
        {
            if(filePath.Contains(directory + "/base", StringComparison.OrdinalIgnoreCase)
                || filePath.Contains(directory + "/extra", StringComparison.OrdinalIgnoreCase)
                || filePath.Contains(directory + "/pkg_", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
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
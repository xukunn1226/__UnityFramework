using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace Framework.AssetManagement.Runtime
{
    [Serializable]
    public class CustomManifest
    {
        /// <summary>
        /// ���з�����Դ����Ϣ������unity bundle��raw asset
        /// </summary>
        [Serializable]
        public class BundleDetail
        {
            public string       bundleName;             // bundle name
            public string       bundlePath;             // ��¼bundle��Ӧ����Դ·����������ʹ��
            public bool         isUnityBundle;          // �Ƿ���unity bundle
            public bool         isStreamingAsset;       // true��in streaming asset; false: in persistent data path
            public string[]     dependencies;           // ������bundle name list
        }

        public class FileDetail
        {
            public string       bundleHash;             // ��Դ������bundle name
            public string       fileName;               // file name
            [NonSerialized]
            public BundleDetail bundleDetail;           // ��Դ������bundle������ʱ��ֵ
        }

        [SerializeField]
        public Dictionary<string, FileDetail>       m_FileDetails   = new Dictionary<string, FileDetail>();     // ��¼��Դ��bundle�Ķ�Ӧ��ϵ
                                                                                                                // key:   identifier������ʹ��assetPath
                                                                                                                // value: FileDetail
        [SerializeField]
        public Dictionary<string, BundleDetail>     m_BundleDetails = new Dictionary<string, BundleDetail>();   // ��¼����bundle������ϸ��Ϣ
                                                                                                                // key:     bundle hash
                                                                                                                // value:   BundleDetail

        public BundleDetail GetBundleDetail(string assetBundleName)
        {
            BundleDetail detail;
            m_BundleDetails.TryGetValue(assetBundleName, out detail);
            return detail;
        }

        public FileDetail GetFileDetail(string assetPath)
        {
            FileDetail fd;
            if(m_FileDetails.TryGetValue(assetPath, out fd))
            {
                if(fd.bundleDetail == null)
                {
                    BundleDetail bd;
                    m_BundleDetails.TryGetValue(fd.bundleHash, out bd);
                    fd.bundleDetail = bd;
                }                
            }
            Debug.Assert(fd != null && fd.bundleDetail != null);
            return fd;
        }

        static public void Serialize(string assetPath, CustomManifest manifest)
        {
            string json = JsonConvert.SerializeObject(manifest, Formatting.Indented);

            System.IO.FileStream fs = new System.IO.FileStream(assetPath, System.IO.FileMode.Create);
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(json);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
            fs.Dispose();
        }

        static public CustomManifest Deserialize(string assetPath)
        {
            if (!System.IO.File.Exists(assetPath))
            {
                Debug.LogError($"{assetPath} is not exists");
                return null;
            }

            System.IO.FileStream fs = new System.IO.FileStream(assetPath, System.IO.FileMode.Open);
            byte[] array = new byte[1024 * 2048];       // 2MB
            int size = fs.Read(array, 0, 1024 * 2048);
            fs.Close();
            fs.Dispose();

            return JsonConvert.DeserializeObject<CustomManifest>(System.Text.Encoding.UTF8.GetString(array, 0, size));
        }
    }
}
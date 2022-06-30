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
            public bool         isUnityBundle;          // �Ƿ���unity bundle
            public bool         isStreamingAsset;       // true��in streaming asset; false: in persistent data path
            public string[]     dependencies;           // ������bundle name list
        }

        public class FileDetail
        {
            public string       bundleName;             // ��Դ������bundle name
            public string       fileName;               // file name
            [NonSerialized]
            public BundleDetail bundleDetail;           // ��Դ������bundle������ʱ��ֵ
        }

        [SerializeField]
        public Dictionary<string, FileDetail>       m_FileDetails   = new Dictionary<string, FileDetail>();     // ��¼��Դ��bundle�Ķ�Ӧ��ϵ
                                                                                                                // key:   identifier������ʹ��assetPath
                                                                                                                // value: bundle name
        [SerializeField]
        public Dictionary<string, BundleDetail>     m_BundleDetails = new Dictionary<string, BundleDetail>();   // ��¼����bundle������ϸ��Ϣ
                                                                                                                // key:     bundle name
                                                                                                                // value:   RawBundleDetail

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
                    m_BundleDetails.TryGetValue(fd.bundleName, out bd);
                    fd.bundleDetail = bd;
                }                
            }
            Debug.Assert(fd != null && fd.bundleDetail != null);
            return fd;
        }

#if UNITY_EDITOR
        static public void Serialize(string assetPath, CustomManifest manifest)
        {
            string json = JsonConvert.SerializeObject(manifest, Formatting.Indented);

            System.IO.FileStream fs = new System.IO.FileStream(assetPath, System.IO.FileMode.Create);
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(json);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
            fs.Dispose();
        }
#endif
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Framework.Core.Editor
{
    public class SoftReferenceDBProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int i = 0; i < importedAssets.Length; ++i)
            {
                UpdateDB(importedAssets[i]);
            }

            for (int i = 0; i < deletedAssets.Length; ++i)
            {
                UpdateDB(deletedAssets[i]);
            }

            for (int i = 0; i < movedAssets.Length; ++i)
            {
                UpdateDB(movedAssets[i]);
            }

            for (int i = 0; i < movedFromAssetPaths.Length; ++i)
            {
                UpdateDB(movedFromAssetPaths[i]);
            }
        }

        static private void UpdateDB(string assetPath)
        {

        }
    }

    [System.Serializable]
    public class SoftReferenceInfo
    {
        public string           m_GUID;                 // 被引用资源的GUID
        public string           m_AssetPath;            // 资源路径（与GUID对应，方便DEBUG）

        public FileStream       m_Stream;

        public class UserInfo
        {
            public string       m_GUID;                 // 使用此资源的GUID
            public long         m_FileID;               // FileID，用于定位SoftObjectPath
        }
        public List<UserInfo>   m_UserInfoList = new List<UserInfo>();

        public void AddUser(string guid, long fileID)
        {
            int index = FindUser(guid, fileID);
            if(index != -1)
            {
                Debug.LogError($"{guid} & {fileID} has already exist.");
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if(string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"{guid} is not valid");
                return;
            }

            m_UserInfoList.Add(new UserInfo() { m_GUID = guid, m_FileID = fileID });
        }

        public int RemoveUser(string guid, long fileID)
        {
            int index = FindUser(guid, fileID);
            if (index == -1)
            {
                Debug.LogError($"{guid} & {fileID} not exist.");
                return m_UserInfoList.Count;
            }

            m_UserInfoList.RemoveAt(index);
            return m_UserInfoList.Count;
        }

        // 当资源发生移动时通知引用对象
        public void UpdateUsers()
        {
            m_AssetPath = AssetDatabase.GUIDToAssetPath(m_GUID);

            // todo: update users
        }

        // 当资源发生删除时通知引用对象
        public void Clear()
        {
            // todo: update users

        }

        private int FindUser(string guid, long fileID)
        {
            return m_UserInfoList.FindIndex((item) => (item.m_GUID == guid && item.m_FileID == fileID));
        }
    }

    [InitializeOnLoad]
    public class SoftReferencesDB
    {
        static private Dictionary<string, SoftReferenceInfo> k_Datas = new Dictionary<string, SoftReferenceInfo>();

        static SoftReferencesDB()
        {
        }

        static public SoftReferenceInfo GetOrCreateData(string guid)
        {
            SoftReferenceInfo data;
            if(!k_Datas.TryGetValue(guid, out data))
            {
                data = new SoftReferenceInfo();
                data.m_GUID = guid;
                data.m_AssetPath = AssetDatabase.GUIDToAssetPath(guid);
                k_Datas.Add(guid, data);
            }

            return data;
        }

        static public void UpdateData_MoveAsset(string guid)
        {
            SoftReferenceInfo data;
            if (!k_Datas.TryGetValue(guid, out data))
            {
                return;
            }


        }
    }
}
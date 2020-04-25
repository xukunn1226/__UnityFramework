using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

namespace Framework.Core.Editor
{
    public class RedirectorDBProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (Application.isBatchMode)
                return;

            for (int i = 0; i < importedAssets.Length; ++i)
            {
                //Debug.Log($"importedAsset: {importedAssets[i]}");
                RedirectorDB.ImportAsset(importedAssets[i]);
            }

            bool bDirty = false;
            for (int i = 0; i < deletedAssets.Length; ++i)
            {
                //Debug.Log($"deletedAssets: {deletedAssets[i]}");
                bDirty |= RedirectorDB.DeleteAsset(deletedAssets[i]);
            }

            for (int i = 0; i < movedAssets.Length; ++i)
            {
                //Debug.Log($"movedAssets: {movedAssets[i]}       movedFromAssetPaths: {movedFromAssetPaths[i]}");
                bDirty |= RedirectorDB.MoveAsset(movedAssets[i]);
            }

            if(bDirty)
            {
                AssetDatabase.SaveAssets();
            }
        }
    }

    /// <summary>
    /// 重定向器，记录资源被引用信息
    /// </summary>
    public class SoftRefRedirector
    {
        public string           m_RefObjectGUID;                // 被引用资源的GUID
        public string           m_RefObjectAssetPath;           // 资源路径（与GUID对应，方便DEBUG）
        
        public class UserInfo
        {
            public string       m_UserObjectGUID;               // 使用此资源的GUID
            public long         m_FileID;                       // FileID，用于定位SoftObjectPath
            public string       m_UserObjectAssetPath;          // 资源路径（与GUID对应，方便DEBUG）
        }
        public SortedList<string, UserInfo> m_UserInfoList = new SortedList<string, UserInfo>();        // key: m_UserObjectGUID | m_FileID

        public void AddOrUpdateUserInfo(string userGUID, long fileID)
        {
            string key = MakeKey(userGUID, fileID);
            if (m_UserInfoList.ContainsKey(key))
            {
                // 多次导入属正常情况
                //Debug.LogError($"{guid} & {fileID} has already exist.");

                UserInfo userInfo;
                if(m_UserInfoList.TryGetValue(key, out userInfo))
                {
                    userInfo.m_UserObjectGUID = userGUID;
                    userInfo.m_FileID = fileID;
                    userInfo.m_UserObjectAssetPath = AssetDatabase.GUIDToAssetPath(userGUID);
                }
                
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(userGUID);
            if(string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"{userGUID} is not valid");
                return;
            }

            m_UserInfoList.Add(key, new UserInfo() { m_UserObjectGUID = userGUID, m_FileID = fileID, m_UserObjectAssetPath = assetPath });
        }

        public string MakeKey(string guid, long fileID)
        {
            return string.Format($"{guid}|{fileID}");
        }
    }

    [InitializeOnLoad]
    public class RedirectorDB
    {
        static private string k_SavedPath = "Assets/RedirectorDB";

        static RedirectorDB()
        {
            Directory.CreateDirectory(k_SavedPath);
        }
        
        /// <summary>
        /// 资源导入时根据SoftObjectPath组件信息更新DB
        /// </summary>
        /// <param name="userAssetPath"></param>
        static public void ImportAsset(string userAssetPath)
        {
            // todo: 目前仅支持GameObject上挂载SoftReferencePath
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(userAssetPath);
            if (asset == null)
                return;

            SoftObjectPath[] sopList = asset.GetComponentsInChildren<SoftObjectPath>(true);
            if (sopList.Length == 0)
                return;

            string userGUID = AssetDatabase.AssetPathToGUID(userAssetPath);
            foreach (var sop in sopList)
            {
                if (string.IsNullOrEmpty(sop.m_GUID))
                    continue;

                string referencedAssetPath = AssetDatabase.GUIDToAssetPath(sop.m_GUID);         // 被引用资源路径
                if (string.IsNullOrEmpty(referencedAssetPath))
                    continue;       // 引用资源被删除可能失效

                // get or create SoftReferenceInfo data
                SoftRefRedirector sri;
                string filePath = string.Format("{0}/{1}.json", k_SavedPath, sop.m_GUID);
                if (File.Exists(filePath))
                {
                    sri = DeserializeSoftReference(sop.m_GUID);
                }
                else
                {
                    sri = new SoftRefRedirector() { m_RefObjectGUID = sop.m_GUID, m_RefObjectAssetPath = referencedAssetPath };
                }

                // get fileID
                string guid;
                long fileID;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sop, out guid, out fileID);
                if (guid != userGUID)
                    throw new System.Exception("guid != userGUID");
                sri.AddOrUpdateUserInfo(userGUID, fileID);

                SerializeSoftReference(sop.m_GUID, sri);
            }
        }

        /// <summary>
        /// 删除资源触发DB更新，把正在引用此资源的SoftObjectPath清空
        /// </summary>
        /// <param name="deletedAssetPath"></param>
        static public bool DeleteAsset(string deletedAssetPath)
        {
            return UpdateSoftRefRedirector(deletedAssetPath, true);
        }

        /// <summary>
        /// 移动、改名触发DB更新
        /// </summary>
        /// <param name="newAssetPath"></param>
        static public bool MoveAsset(string newAssetPath)
        {
            return UpdateSoftRefRedirector(newAssetPath, false);
        }

        static private bool UpdateSoftRefRedirector(string referencedObjectAssetPath, bool bDelete)
        {
            string guid = AssetDatabase.AssetPathToGUID(referencedObjectAssetPath);

            string filePath = string.Format("{0}/{1}.json", k_SavedPath, guid);
            if (!File.Exists(filePath))
                return false;

            // 根据json记录的引用数据更新
            List<string> removeList = new List<string>();
            SoftRefRedirector sri = DeserializeSoftReference(guid);
            sri.m_RefObjectGUID = guid;
            sri.m_RefObjectAssetPath = referencedObjectAssetPath;
            bool bModified = false;
            foreach (var item in sri.m_UserInfoList)
            {
                SoftRefRedirector.UserInfo userInfo = item.Value;
                string userAssetPath = AssetDatabase.GUIDToAssetPath(userInfo.m_UserObjectGUID);
                if (string.IsNullOrEmpty(userAssetPath))        // 被删除资源的GUID仍会返回一个路径，需要加载判断资源是否真实存在
                {
                    removeList.Add(item.Key);
                    continue;
                }

                GameObject userGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(userAssetPath);
                if (userGameObject == null)
                {
                    removeList.Add(item.Key);
                    continue;       // 可能userAssetPath有值，但对应的资源已删除
                }

                // 找到匹配fileID的数据
                bool bFind = false;
                SoftObjectPath[] sopList = userGameObject.GetComponentsInChildren<SoftObjectPath>(true);
                foreach (var sop in sopList)
                {
                    string userGUID;
                    long fileID;
                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sop, out userGUID, out fileID))
                        continue;

                    if (fileID != userInfo.m_FileID)
                        continue;

                    bFind = true;

                    sop.m_GUID = bDelete ? null : guid;
                    sop.m_AssetPath = bDelete ? null : referencedObjectAssetPath.ToLower();

                    UnityEditor.EditorUtility.SetDirty(sop.transform);
                    bModified = true;

                    break;
                }

                if (!bFind)
                {
                    removeList.Add(item.Key);
                }
            }

            // 移除已失效的记录
            foreach (var r in removeList)
            {
                sri.m_UserInfoList.Remove(r);
            }

            if (bDelete)
            {
                // 保留记录不删除，以备debug
                //AssetDatabase.DeleteAsset(filePath)
                //AssetDatabase.Refresh();

                SerializeSoftReference(guid, sri);
            }
            else
            {
                SerializeSoftReference(guid, sri);
            }
            return bModified;
        }

        private static SoftRefRedirector DeserializeSoftReference(string filename)
        {
            string filePath = string.Format("{0}/{1}.json", k_SavedPath, filename);

            FileStream reader = new FileStream(filePath, FileMode.Open);
            byte[] bs_reader = new byte[reader.Length];
            reader.Read(bs_reader, 0, bs_reader.Length);
            reader.Close();
            string json_reader = System.Text.Encoding.UTF8.GetString(bs_reader);

            return JsonConvert.DeserializeObject<SoftRefRedirector>(json_reader);
        }

        private static void SerializeSoftReference(string filename, SoftRefRedirector data)
        {
            string filePath = string.Format("{0}/{1}.json", k_SavedPath, filename);

            string json_writer = JsonConvert.SerializeObject(data, Formatting.Indented);
            byte[] bs_writer = System.Text.Encoding.UTF8.GetBytes(json_writer);

            FileStream writer = new FileStream(filePath, FileMode.Create);
            writer.Write(bs_writer, 0, bs_writer.Length);
            writer.Close();

            AssetDatabase.ImportAsset(filePath);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;

namespace Framework.Core.Editor
{
    public class SoftReferenceDBProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (Application.isBatchMode)
                return;

            // 触发DB存储，OnWillSaveAssets在资源导入时不触发，所以不适合触发DB存储
            for (int i = 0; i < importedAssets.Length; ++i)
            {
                Debug.Log($"importedAsset: {importedAssets[i]}");
                SoftReferencesDB.ImportAsset(importedAssets[i]);
            }

            for (int i = 0; i < deletedAssets.Length; ++i)
            {
                Debug.Log($"deletedAssets: {deletedAssets[i]}");
                SoftReferencesDB.DeleteAsset(deletedAssets[i]);

                if(i == deletedAssets.Length - 1)
                {
                    AssetDatabase.SaveAssets();
                }
            }

            for (int i = 0; i < movedAssets.Length; ++i)
            {
                Debug.Log($"movedAssets: {movedAssets[i]}       movedFromAssetPaths: {movedFromAssetPaths[i]}");
                SoftReferencesDB.MoveAsset(movedFromAssetPaths[i], movedAssets[i]);
            }
        }
    }

    //public class SoftReferenceDBModificationProcessor : UnityEditor.AssetModificationProcessor
    //{
    //    static void OnWillCreateAsset(string assetName)
    //    {
    //        if (Application.isBatchMode)
    //            return;

    //        Debug.Log("OnWillCreateAsset - is being called with the following asset: " + assetName + ".");
    //    }

    //    public static AssetDeleteResult OnWillDeleteAsset(string AssetPath, RemoveAssetOptions rao)
    //    {
    //        if (Application.isBatchMode)
    //            return AssetDeleteResult.DidNotDelete;

    //        Debug.Log("OnWillDeleteAsset - unity callback: " + AssetPath);

    //        return AssetDeleteResult.DidNotDelete;
    //    }

    //    private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
    //    {
    //        if (Application.isBatchMode)
    //            return AssetMoveResult.DidMove;

    //        Debug.Log("OnWillMoveAsset - Source path: " + sourcePath + ". Destination path: " + destinationPath + ".");
    //        AssetMoveResult assetMoveResult = AssetMoveResult.DidMove;

    //        // Perform operations on the asset and set the value of 'assetMoveResult' accordingly.

    //        return assetMoveResult;
    //    }

    //    static string[] OnWillSaveAssets(string[] paths)
    //    {
    //        if (Application.isBatchMode)
    //            return paths;

    //        Debug.Log("OnWillSaveAssets: " + paths.Length);
    //        foreach (string path in paths)
    //            Debug.Log("---OnWillSaveAssets:" + path);
    //        return paths;
    //    }
    //}

    [System.Serializable]
    public class SoftReferenceInfo
    {
        public string           m_GUID;                 // 被引用资源的GUID
        public string           m_AssetPath;            // 资源路径（与GUID对应，方便DEBUG）
        
        public class UserInfo
        {
            public string       m_GUID;                 // 使用此资源的GUID
            public long         m_FileID;               // FileID，用于定位SoftObjectPath
            public string       m_AssetPath;            // 资源路径（与GUID对应，方便DEBUG）
        }
        public SortedList<string, UserInfo> m_UserInfoList = new SortedList<string, UserInfo>();

        public void AddOrUpdateUser(string guid, long fileID)
        {
            if(m_UserInfoList.ContainsKey(guid))
            {
                // 多次导入属正常情况
                //Debug.LogError($"{guid} & {fileID} has already exist.");

                UserInfo userInfo;
                if(m_UserInfoList.TryGetValue(guid, out userInfo))
                {
                    userInfo.m_GUID = guid;
                    userInfo.m_FileID = fileID;
                    userInfo.m_AssetPath = AssetDatabase.GUIDToAssetPath(guid);
                }
                
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if(string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"{guid} is not valid");
                return;
            }

            m_UserInfoList.Add(guid, new UserInfo() { m_GUID = guid, m_FileID = fileID, m_AssetPath = AssetDatabase.GUIDToAssetPath(guid) });
        }

        public int RemoveUser(string guid, long fileID)
        {
            if (!m_UserInfoList.ContainsKey(guid))
            {
                Debug.LogError($"{guid} & {fileID} not exist.");
                return m_UserInfoList.Count;
            }

            m_UserInfoList.Remove(guid);
            return m_UserInfoList.Count;
        }
    }

    [InitializeOnLoad]
    public class SoftReferencesDB
    {
        static private string k_SavedPath = "Assets/SoftReferenceDB";

        static private Dictionary<string, SoftReferenceInfo> k_Datas = new Dictionary<string, SoftReferenceInfo>();

        static SoftReferencesDB()
        {
            Directory.CreateDirectory(k_SavedPath);

            ReloadDB();
        }

        static void ReloadDB()
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

        static public void UpdateData_DeleteAsset(string guid)
        {

        }

        /// <summary>
        /// 手动操作触发数据更新
        /// </summary>
        /// <param name="oldGUID">被替换掉的引用资源GUID</param>
        /// <param name="newGUID">新的引用资源GUID</param>
        /// <param name="userGUID">使用对象的GUID</param>
        /// <param name="userFileID">使用对象组件FileID</param>
        static public void UpdateDataManually(string oldGUID, string newGUID, string userGUID, long userFileID)
        {
            // delete old data
            if(!string.IsNullOrEmpty(oldGUID))
            {
                SoftReferenceInfo oldData;
                if(!k_Datas.TryGetValue(oldGUID, out oldData))
                {
                    throw new System.Exception("{oldGUID} not record");
                }

                oldData.RemoveUser(userGUID, userFileID);
            }

            // add new data
            if (string.IsNullOrEmpty(newGUID))
                throw new System.ArgumentNullException("newGUID");

            SoftReferenceInfo newData = GetOrCreateData(newGUID);
            newData.AddOrUpdateUser(userGUID, userFileID);
        }

        static public void Flush(string guid)
        {
            SoftReferenceInfo data;
            if(!k_Datas.TryGetValue(guid, out data))
            {
                throw new System.Exception($"{guid} not found in k_Datas!");
            }

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(json);

            FileStream fs = new FileStream(string.Format("{0}/{1}.json", k_SavedPath, guid), FileMode.Create);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
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
                string referencedAssetPath = AssetDatabase.GUIDToAssetPath(sop.m_GUID);         // 被引用资源路径
                if (string.IsNullOrEmpty(referencedAssetPath))
                    continue;       // 引用资源被删除可能失效

                // get or create SoftReferenceInfo data
                SoftReferenceInfo sri;
                string filePath = string.Format("{0}/{1}.json", k_SavedPath, sop.m_GUID);
                if (File.Exists(filePath))
                {
                    sri = DeserializeSoftReference(sop.m_GUID);
                }
                else
                {
                    sri = new SoftReferenceInfo() { m_GUID = sop.m_GUID, m_AssetPath = referencedAssetPath };
                }

                // get fileID
                string guid;
                long fileID;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sop, out guid, out fileID);
                if (guid != userGUID)
                    throw new System.Exception("guid != userGUID");
                sri.AddOrUpdateUser(userGUID, fileID);

                SerializeSoftReference(sop.m_GUID, sri);
            }
        }

        /// <summary>
        /// 删除资源触发DB更新，把正在引用此资源的SoftObjectPath清空
        /// </summary>
        /// <param name="deletedAssetPath"></param>
        static public void DeleteAsset(string deletedAssetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(deletedAssetPath);

            string filePath = string.Format("{0}/{1}.json", k_SavedPath, guid);
            if (!File.Exists(filePath))
                return;

            // 根据json记录的引用数据更新
            SoftReferenceInfo sri = DeserializeSoftReference(guid);
            foreach(var item in sri.m_UserInfoList)
            {
                SoftReferenceInfo.UserInfo userInfo = item.Value;
                string userAssetPath = AssetDatabase.GUIDToAssetPath(userInfo.m_GUID);
                if (string.IsNullOrEmpty(userAssetPath))        // 被删除资源的GUID仍会返回一个路径
                    continue;

                GameObject userGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(userAssetPath);
                if (userGameObject == null)
                    continue;       // 可能userAssetPath有值，但对应的资源已删除

                SoftObjectPath[] sopList = userGameObject.GetComponentsInChildren<SoftObjectPath>(true);
                foreach(var sop in sopList)
                {
                    string userGUID;
                    long fileID;
                    if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sop, out userGUID, out fileID))
                        continue;

                    if (fileID != userInfo.m_FileID)
                        continue;

                    sop.m_GUID = null;
                    sop.m_AssetPath = null;

                    UnityEditor.EditorUtility.SetDirty(sop.transform.root.gameObject);
                }
            }
        }

        /// <summary>
        /// 移动、改名触发DB更新
        /// </summary>
        /// <param name="oldAssetPath"></param>
        /// <param name="newAssetPath"></param>
        static public void MoveAsset(string oldAssetPath, string newAssetPath)
        {

        }

        private static SoftReferenceInfo DeserializeSoftReference(string filename)
        {
            string filePath = string.Format("{0}/{1}.json", k_SavedPath, filename);

            FileStream reader = new FileStream(filePath, FileMode.Open);
            byte[] bs_reader = new byte[reader.Length];
            reader.Read(bs_reader, 0, bs_reader.Length);
            reader.Close();
            string json_reader = System.Text.Encoding.UTF8.GetString(bs_reader);

            return JsonConvert.DeserializeObject<SoftReferenceInfo>(json_reader);
        }

        private static void SerializeSoftReference(string filename, SoftReferenceInfo data)
        {
            string filePath = string.Format("{0}/{1}.json", k_SavedPath, filename);

            string json_writer = JsonConvert.SerializeObject(data, Formatting.Indented);
            byte[] bs_writer = System.Text.Encoding.UTF8.GetBytes(json_writer);

            FileStream writer = new FileStream(filePath, FileMode.Create);
            writer.Write(bs_writer, 0, bs_writer.Length);
            writer.Close();
        }
    }
}
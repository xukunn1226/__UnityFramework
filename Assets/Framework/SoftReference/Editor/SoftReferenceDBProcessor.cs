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
                //SoftReferencesDB.ImportAsset(importedAssets[i]);
            }

            for (int i = 0; i < deletedAssets.Length; ++i)
            {
                Debug.Log($"deletedAssets: {deletedAssets[i]}");
            }

            for (int i = 0; i < movedAssets.Length; ++i)
            {
                Debug.Log($"movedAssets: {movedAssets[i]}");
            }

            for (int i = 0; i < movedFromAssetPaths.Length; ++i)
            {
                Debug.Log($"movedFromAssetPaths: {movedFromAssetPaths[i]}");
            }
        }
    }

    public class SoftReferenceDBModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        static void OnWillCreateAsset(string assetName)
        {
            if (Application.isBatchMode)
                return;

            Debug.Log("OnWillCreateAsset - is being called with the following asset: " + assetName + ".");
        }

        public static AssetDeleteResult OnWillDeleteAsset(string AssetPath, RemoveAssetOptions rao)
        {
            if (Application.isBatchMode)
                return AssetDeleteResult.DidNotDelete;

            Debug.Log("OnWillDeleteAsset - unity callback: " + AssetPath);

            return AssetDeleteResult.DidNotDelete;
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (Application.isBatchMode)
                return AssetMoveResult.DidMove;

            Debug.Log("OnWillMoveAsset - Source path: " + sourcePath + ". Destination path: " + destinationPath + ".");
            AssetMoveResult assetMoveResult = AssetMoveResult.DidMove;

            // Perform operations on the asset and set the value of 'assetMoveResult' accordingly.

            return assetMoveResult;
        }

        static string[] OnWillSaveAssets(string[] paths)
        {
            if (Application.isBatchMode)
                return paths;

            Debug.Log("OnWillSaveAssets: " + paths.Length);
            foreach (string path in paths)
                Debug.Log("---OnWillSaveAssets:" + path);
            return paths;
        }
    }

    [System.Serializable]
    public class SoftReferenceInfo
    {
        public string           m_GUID;                 // 被引用资源的GUID
        public string           m_AssetPath;            // 资源路径（与GUID对应，方便DEBUG）
        
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
            newData.AddUser(userGUID, userFileID);
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
        /// <param name="assetPath"></param>
        static public void ImportAsset(string assetPath)
        {
            // todo: 目前仅支持GameObject上挂载SoftReferencePath
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null)
                return;

            SoftObjectPath[] sopList = asset.GetComponentsInChildren<SoftObjectPath>(true);
            if (sopList.Length == 0)
                return;

            string userGUID = AssetDatabase.AssetPathToGUID(assetPath);
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
                    //FileStream reader = new FileStream(filePath, FileMode.Open);
                    //byte[] bs_reader = new byte[reader.Length];
                    //reader.Read(bs_reader, 0, bs_reader.Length);
                    //reader.Close();
                    //string json_reader = System.Text.Encoding.UTF8.GetString(bs_reader);

                    //sri = JsonConvert.DeserializeObject<SoftReferenceInfo>(json_reader);

                    sri = DeserializeSoftReference(sop.m_GUID);
                }
                else
                {
                    sri = new SoftReferenceInfo() { m_GUID = sop.m_GUID, m_AssetPath = assetPath };
                }

                // get fileID
                string guid;
                long fileID;
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sop, out guid, out fileID);
                if (guid != userGUID)
                    throw new System.Exception("guid != userGUID");
                sri.AddUser(userGUID, fileID);

                // save to disk
                //string json_writer = JsonConvert.SerializeObject(sri, Formatting.Indented);
                //byte[] bs_writer = System.Text.Encoding.UTF8.GetBytes(json_writer);

                //FileStream writer = new FileStream(filePath, FileMode.Open);
                //writer.Write(bs_writer, 0, bs_writer.Length);
                //writer.Close();

                SerializeSoftReference(sop.m_GUID, sri);
            }
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

        public static long GetObjectLocalIdInFile(Object _object)
        {
            SerializedObject serialize = new SerializedObject(_object);

            PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            if (inspectorModeInfo != null)
                inspectorModeInfo.SetValue(serialize, InspectorMode.Debug, null);

            return serialize.FindProperty("m_LocalIdentfierInFile")?.longValue ?? 0;
        }

        static public void MoveAsset(string oldAssetPath, string newAssetPath)
        {

        }
    }
}
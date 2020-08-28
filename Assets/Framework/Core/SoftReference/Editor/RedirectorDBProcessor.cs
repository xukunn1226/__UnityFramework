using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEditor.SceneManagement;
using Unity.EditorCoroutines.Editor;

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
                // Debug.Log($"importedAsset: {importedAssets[i]}");
                RedirectorDB.ImportAsset(importedAssets[i]);
            }

            bool bDirty = false;
            for (int i = 0; i < deletedAssets.Length; ++i)
            {
                // Debug.Log($"deletedAssets: {deletedAssets[i]}");
                bDirty |= RedirectorDB.DeleteAsset(deletedAssets[i]);
            }

            for (int i = 0; i < movedAssets.Length; ++i)
            {
                // Debug.Log($"movedAssets: {movedAssets[i]}       movedFromAssetPaths: {movedFromAssetPaths[i]}");
                bDirty |= RedirectorDB.MoveAsset(movedAssets[i]);
            }

            if (bDirty)
            {
                AssetDatabase.SaveAssets();
            }
        }
    }

    [InitializeOnLoad]
    public class RedirectorDB
    {
        static private string s_RefObjectDBPath = "Assets/RedirectorCache/RefObject_DB";           // 记录资源被引用的信息
        static private string s_UserObjectDBPath = "Assets/RedirectorCache/UserObject_DB";         // 记录userObj引用的资源

        static RedirectorDB()
        {
            if(!Directory.Exists(s_RefObjectDBPath))
                Directory.CreateDirectory(s_RefObjectDBPath);
            
            if(!Directory.Exists(s_UserObjectDBPath))
                Directory.CreateDirectory(s_UserObjectDBPath);
        }

        /// <summary>
        /// 记录资源被引用信息
        /// </summary>
        public class RefObjectInfo
        {
            public string       m_RefObjectGUID;                // 被引用资源的GUID
            public string       m_RefObjectAssetPath;           // 资源路径（与GUID对应，方便DEBUG）

            public class UserInfo
            {
                public string   m_UserObjectGUID;               // 使用此资源的GUID(*.unity, *.prefab)
                public long     m_FileID;                       // FileID，用于资源内部定位SoftObjectPath
                public string   m_UserObjectAssetPath;          // 资源路径（与GUID对应，方便DEBUG）
            }
            public SortedList<string, UserInfo> m_UserInfoList = new SortedList<string, UserInfo>();        // key: m_UserObjectGUID | m_FileID

            public void AddOrUpdateUserInfo(string userGUID, long fileID)
            {
                string key = MakeKey(userGUID, fileID);
                if (m_UserInfoList.ContainsKey(key))
                {
                    UserInfo userInfo;
                    if (m_UserInfoList.TryGetValue(key, out userInfo))
                    {
                        userInfo.m_UserObjectGUID = userGUID;
                        userInfo.m_FileID = fileID;
                        userInfo.m_UserObjectAssetPath = AssetDatabase.GUIDToAssetPath(userGUID);
                    }

                    return;
                }

                string assetPath = AssetDatabase.GUIDToAssetPath(userGUID);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogError($"{userGUID} is not valid");
                    return;
                }

                m_UserInfoList.Add(key, new UserInfo() { m_UserObjectGUID = userGUID, m_FileID = fileID, m_UserObjectAssetPath = assetPath });
            }

            public void Remove(string userGUID, long fileID)
            {
                m_UserInfoList.Remove(MakeKey(userGUID, fileID));
            }

            private string MakeKey(string guid, long fileID)
            {
                return string.Format($"{guid}|{fileID}");
            }
        }

        /// <summary>
        /// 记录*.prefab, *.unity引用的资源信息
        /// </summary>
        public class UserObjectInfo
        {
            public string m_UserObjectGUID;
            public string m_UserObjectAssetPath;

            public class RefInfo
            {
                public long m_FileID;           // FileID means that which component of UserObject references the m_RefObjectGUID
                public string m_RefObjectGUID;
                public string m_RefObjectAssetPath;
            }
            public SortedList<long, RefInfo> m_RefInfoList = new SortedList<long, RefInfo>();               // 记录引用的资源信息（SoftObjectPath、SoftObject）

            public void AddOrUpdateRefInfo(long fileID, string refGUID)
            {
                if (m_RefInfoList.ContainsKey(fileID))
                {
                    RefInfo refInfo;
                    if (m_RefInfoList.TryGetValue(fileID, out refInfo))
                    {
                        refInfo.m_FileID = fileID;
                        refInfo.m_RefObjectGUID = refGUID;                        
                        refInfo.m_RefObjectAssetPath = AssetDatabase.GUIDToAssetPath(refGUID);
                    }

                    return;
                }

                string assetPath = AssetDatabase.GUIDToAssetPath(refGUID);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogError($"{refGUID} is not valid");
                    return;
                }

                m_RefInfoList.Add(fileID, new RefInfo() { m_FileID = fileID, m_RefObjectGUID = refGUID, m_RefObjectAssetPath = assetPath });
            }

            public void Remove(long fileID)
            {
                m_RefInfoList.Remove(fileID);
            }

            public void Clear()
            {
                m_RefInfoList.Clear();
            }
        }

        /// <summary>
        /// 根据SoftObjectPath组件信息更新DB
        /// </summary>
        /// <param name="importerAssetPath"></param>
        static public void ImportAsset(string importerAssetPath)
        {
            if (importerAssetPath.EndsWith(".prefab"))
            {
                InternalImportAsset(importerAssetPath);
            }
        }

        /// <summary>
        /// 删除资源触发DB更新，把正在引用此资源的SoftObjectPath清空
        /// </summary>
        /// <param name="deletedAssetPath"></param>
        static public bool DeleteAsset(string deletedAssetPath)
        {
            return InternalDeleteAsset(deletedAssetPath);
        }

        /// <summary>
        /// 移动、改名触发DB更新
        /// </summary>
        /// <param name="movedAssetPath"></param>
        static public bool MoveAsset(string movedAssetPath)
        {
            return InternalMoveAsset(movedAssetPath);
        }

        /// <summary>
        /// 资源导入、保存时把其所有SoftObjectPath数据更新至redirector
        /// 遍历asset中的所有SoftObjectPath，更新其GUID,FILEID
        /// </summary>
        static private void InternalImportAsset(string importedAssetPath)
        {
            //////////////////////////// importedAssetPath: 更新UserObject_DB
            // step 1. 更新UserObj之前指向的Ref的数据
            string userGUID = AssetDatabase.AssetPathToGUID(importedAssetPath);
            string userFilePath = string.Format("{0}/{1}.json", s_UserObjectDBPath, userGUID);
            UserObjectInfo uoi;
            if(File.Exists(userFilePath))
            {
                uoi = DeserializeUserInfo(userGUID);
                foreach(var ri in uoi.m_RefInfoList)
                {
                    string refFilePath = string.Format("{0}/{1}.json", s_RefObjectDBPath , ri.Value.m_RefObjectGUID);
                    if(File.Exists(refFilePath))
                    {
                        RefObjectInfo roi = DeserializeSoftReference(ri.Value.m_RefObjectGUID);
                        roi.Remove(userGUID, ri.Value.m_FileID);
                        SerializeSoftReference(ri.Value.m_RefObjectGUID, roi);
                    }
                }
            }

            // step 2. 更新UserObj当前指向的Ref数据
            GameObject userObj = AssetDatabase.LoadAssetAtPath<GameObject>(importedAssetPath);
            SoftObjectPath[] sopList = userObj.GetComponentsInChildren<SoftObjectPath>(true);
            foreach (var sop in sopList)
            {
                if (string.IsNullOrEmpty(sop.m_GUID))
                    continue;

                string referencedAssetPath = AssetDatabase.GUIDToAssetPath(sop.m_GUID);         // 被引用资源路径
                if (string.IsNullOrEmpty(referencedAssetPath))
                    continue;       // 引用资源被删除可能失效

                long id = GetLocalID(sop);
                if(id == 0)
                    continue;       // 场景对象时可能为0，表示组件是prefab的一部分，而不是场景的一部分

                // get or create SoftReferenceInfo data
                RefObjectInfo sri;
                string filePath = string.Format("{0}/{1}.json", s_RefObjectDBPath, sop.m_GUID);
                if (File.Exists(filePath))
                {
                    sri = DeserializeSoftReference(sop.m_GUID);
                }
                else
                {
                    sri = new RefObjectInfo() { m_RefObjectGUID = sop.m_GUID, m_RefObjectAssetPath = referencedAssetPath };
                    // Debug.Log($"--- New RefObjectInfo: {referencedAssetPath}");
                }

                sri.AddOrUpdateUserInfo(userGUID, id);

                SerializeSoftReference(sop.m_GUID, sri);
            }

            // step 3. create or update UserObj DB
            if(sopList.Length != 0)
            {
                if(File.Exists(userFilePath))
                {
                    uoi = DeserializeUserInfo(userGUID);
                }
                else
                {
                    uoi = new UserObjectInfo() { m_UserObjectGUID = userGUID, m_UserObjectAssetPath = AssetDatabase.GUIDToAssetPath(userGUID) };
                    Debug.Log($"--- New UserObjectInfo: {uoi.m_UserObjectAssetPath}");
                }

                uoi.Clear();
                foreach(var sop in sopList)
                {
                    if (string.IsNullOrEmpty(sop.m_GUID))
                        continue;

                    string referencedAssetPath = AssetDatabase.GUIDToAssetPath(sop.m_GUID);         // 被引用资源路径
                    if (string.IsNullOrEmpty(referencedAssetPath))
                        continue;       // 引用资源被删除可能失效

                    long id = GetLocalID(sop);
                    if(id == 0)
                        continue;

                    uoi.AddOrUpdateRefInfo(id, sop.m_GUID);
                }
                SerializeUserInfo(userGUID, uoi);
            }
            else
            {
                if(File.Exists(userFilePath))
                {
                    File.Delete(userFilePath);
                    File.Delete(userFilePath + ".meta");
                }
            }
        }

        /// <summary>
        /// 被引用资源删除时触发引用资源更新数据
        /// </summary>
        /// <param name="deletedAssetPath"></param>
        /// <returns></returns>
        static private bool InternalDeleteAsset(string deletedAssetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(deletedAssetPath);
            bool isDirty = false;

            // step 1. update RefObject DB
            string userFilePath = string.Format("{0}/{1}.json", s_UserObjectDBPath, guid);
            string refFilePath;
            if(File.Exists(userFilePath))
            {
                UserObjectInfo uoi = DeserializeUserInfo(guid);
                
                foreach(var ri in uoi.m_RefInfoList)
                {
                    refFilePath = string.Format("{0}/{1}.json", s_RefObjectDBPath, ri.Value.m_RefObjectGUID);
                    if(File.Exists(refFilePath))
                    {
                        RefObjectInfo roi = DeserializeSoftReference(refFilePath);
                        roi.Remove(guid, ri.Value.m_FileID);
                        SerializeSoftReference(refFilePath, roi);
                    }
                }
                File.Delete(userFilePath);
                File.Delete(userFilePath + ".meta");
            }

            // step 2. update UserObject DB
            refFilePath = string.Format("{0}/{1}.json", s_RefObjectDBPath, guid);
            if (File.Exists(refFilePath))
            {
                RefObjectInfo roi = DeserializeSoftReference(guid);

                foreach(var ui in roi.m_UserInfoList)
                {
                    userFilePath = string.Format("{0}/{1}.json", s_UserObjectDBPath, guid);
                    if(File.Exists(userFilePath))
                    {
                        UserObjectInfo uoi = DeserializeUserInfo(userFilePath);
                        uoi.Remove(ui.Value.m_FileID);
                        SerializeUserInfo(userFilePath, uoi);
                    }

                    string userAssetPath = AssetDatabase.GUIDToAssetPath(ui.Value.m_UserObjectGUID);
                    GameObject userObj = AssetDatabase.LoadAssetAtPath<GameObject>(userAssetPath);
                    if(userObj != null)
                    {
                        SoftObjectPath[] sops = userObj.GetComponentsInChildren<SoftObjectPath>(true);
                        foreach(var sop in sops)
                        {
                            if(GetLocalID(sop) != ui.Value.m_FileID)
                                continue;
                            sop.m_GUID = string.Empty;
                            sop.assetPath = string.Empty;

                            UnityEditor.EditorUtility.SetDirty(userObj);
                            isDirty = true;
                            break;
                        }
                    }
                }

                File.Delete(refFilePath);
                File.Delete(refFilePath + ".meta");
            }
            return isDirty;
        }

        static private bool InternalMoveAsset(string movedAssetPath)
        {
            //////////////////////////// movedAssetPath: 更新RefObject_DB            
            string refGUID = AssetDatabase.AssetPathToGUID(movedAssetPath);
            string refFilePath = string.Format("{0}/{1}.json", s_RefObjectDBPath, refGUID);
            
            if(!File.Exists(refFilePath))
                return false;

            RefObjectInfo roi;
            roi = DeserializeSoftReference(refGUID);
            roi.m_RefObjectGUID = refGUID;
            roi.m_RefObjectAssetPath = movedAssetPath;
            SerializeSoftReference(refGUID, roi);

            bool isDirty = false;
            foreach(var ui in roi.m_UserInfoList)
            {
                string userFilePath = string.Format("{0}/{1}.json", s_UserObjectDBPath , ui.Value.m_UserObjectGUID);
                if(!File.Exists(userFilePath))
                    continue;

                UserObjectInfo uoi = DeserializeUserInfo(ui.Value.m_UserObjectGUID);
                uoi.AddOrUpdateRefInfo(ui.Value.m_FileID, refGUID);
                SerializeUserInfo(ui.Value.m_UserObjectGUID, uoi);

                // update UserObj
                GameObject userObj = AssetDatabase.LoadAssetAtPath<GameObject>(ui.Value.m_UserObjectAssetPath);
                if(userObj != null)
                {
                    SoftObjectPath[] sops = userObj.GetComponentsInChildren<SoftObjectPath>(true);
                    foreach(var sop in sops)
                    {
                        if(GetLocalID(sop) != ui.Value.m_FileID)
                            continue;
                        sop.m_GUID = refGUID;
                        sop.assetPath = movedAssetPath.ToLower();

                        UnityEditor.EditorUtility.SetDirty(userObj);
                        isDirty = true;
                        break;
                    }
                }
            }
            return isDirty;
        }

        private static RefObjectInfo DeserializeSoftReference(string filename)
        {
            string filePath = string.Format("{0}/{1}.json", s_RefObjectDBPath, filename);

            FileStream reader = new FileStream(filePath, FileMode.Open);
            byte[] bs_reader = new byte[reader.Length];
            reader.Read(bs_reader, 0, bs_reader.Length);
            reader.Close();
            string json_reader = System.Text.Encoding.UTF8.GetString(bs_reader);

            return JsonConvert.DeserializeObject<RefObjectInfo>(json_reader);
        }

        private static void SerializeSoftReference(string filename, RefObjectInfo data)
        {
            string filePath = string.Format("{0}/{1}.json", s_RefObjectDBPath, filename);

            string json_writer = JsonConvert.SerializeObject(data, Formatting.Indented);
            byte[] bs_writer = System.Text.Encoding.UTF8.GetBytes(json_writer);

            FileStream writer = new FileStream(filePath, FileMode.Create);
            writer.Write(bs_writer, 0, bs_writer.Length);
            writer.Close();

            AssetDatabase.ImportAsset(filePath);
        }
        
        private static UserObjectInfo DeserializeUserInfo(string filename)
        {
            string filePath = string.Format("{0}/{1}.json", s_UserObjectDBPath, filename);

            FileStream reader = new FileStream(filePath, FileMode.Open);
            byte[] bs_reader = new byte[reader.Length];
            reader.Read(bs_reader, 0, bs_reader.Length);
            reader.Close();
            string json_reader = System.Text.Encoding.UTF8.GetString(bs_reader);

            return JsonConvert.DeserializeObject<UserObjectInfo>(json_reader);
        }

        private static void SerializeUserInfo(string filename, UserObjectInfo data)
        {
            string filePath = string.Format("{0}/{1}.json", s_UserObjectDBPath, filename);

            string json_writer = JsonConvert.SerializeObject(data, Formatting.Indented);
            byte[] bs_writer = System.Text.Encoding.UTF8.GetBytes(json_writer);

            FileStream writer = new FileStream(filePath, FileMode.Create);
            writer.Write(bs_writer, 0, bs_writer.Length);
            writer.Close();

            AssetDatabase.ImportAsset(filePath);
        }

        public static long GetLocalID(UnityEngine.Object go)
        {
            initDebugMode();
            SerializedObject so = new SerializedObject(go);
            debugModeInspectorThing.SetValue(so, InspectorMode.Debug, null);
            SerializedProperty localIDProp = so.FindProperty("m_LocalIdentfierInFile");
            return localIDProp.longValue;
        }

        static PropertyInfo debugModeInspectorThing;
        static void initDebugMode()
        {
            if (debugModeInspectorThing == null)
            {
                debugModeInspectorThing = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        [MenuItem("Assets/Reimport All Redirectors")]
        static void MenuItem_ReimportAllRedirectors()
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Reimport All Redirectors", "Are you sure, about 2 mins", "OK", "Cancel"))
            {
                // delete all redirectors
                if(Directory.Exists(s_RefObjectDBPath))
                    Directory.Delete(s_RefObjectDBPath, true);

                if(Directory.Exists(s_UserObjectDBPath))
                    Directory.Delete(s_UserObjectDBPath, true);

                Directory.CreateDirectory(s_RefObjectDBPath);
                StreamWriter sw = File.CreateText(string.Format("{0}/{1}", s_RefObjectDBPath, "DONTDELETEME.txt"));
                sw.Close();

                Directory.CreateDirectory(s_UserObjectDBPath);
                sw = File.CreateText(string.Format("{0}/{1}", s_UserObjectDBPath, "DONTDELETEME.txt"));
                sw.Close();

                // reimport
                // 56fa9a21fe1ba864086c2d3328d79985 : SoftObjectPath
                // 5526957ebeb07bd4299f5213397a148b : SoftObject
                m_Coroutine = EditorCoroutineUtility.StartCoroutineOwnerless(Reimport(new string[] { "56fa9a21fe1ba864086c2d3328d79985", "5526957ebeb07bd4299f5213397a148b" }, new string[] { ".prefab", ".unity" }));

                AssetDatabase.Refresh();
            }
        }

        static private EditorCoroutine m_Coroutine;
        static private IEnumerator Reimport(string[] guids, string[] filters)
        {
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                .Where(s => filters.Contains(Path.GetExtension(s).ToLower())).ToArray();

            int startIndex = 0;

            while(startIndex < files.Length)
            {
                string file = files[startIndex];

                bool isCancel = UnityEditor.EditorUtility.DisplayCancelableProgressBar("重新导入...", file, (float)startIndex / (float)files.Length);

                foreach (var guid in guids)
                {
                    if (Regex.IsMatch(File.ReadAllText(file), guid))
                    {
                        string assetPath = GetRelativeAssetsPath(file);
                        AssetDatabase.ImportAsset(assetPath);

                        Debug.Log(file, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath));
                    }
                }

                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    UnityEditor.EditorUtility.ClearProgressBar();
                    Debug.Log("重导结束 in (*.prefab, *.unity)");
                    break;
                }

                yield return null;
            };

            StopCoroutine();
        }

        static private void StopCoroutine()
        {
            if(m_Coroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }
        }

        static private string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        }
    }
}
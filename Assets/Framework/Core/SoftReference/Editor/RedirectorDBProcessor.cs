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
                // RedirectorDB.ImportAsset(importedAssets[i]);
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

            if (bDirty)
            {
                AssetDatabase.SaveAssets();
            }
        }
    }

    [InitializeOnLoad]
    public class RedirectorDB
    {
        static private string s_RefObjectDBPath = "Assets/RedirectorDB/RefObject_DB";           // 记录资源被引用的信息
        static private string s_UserObjectDBPath = "Assets/RedirectorDB/UserObject_DB";         // 记录userObj引用的资源

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
                    // 多次导入属正常情况
                    //Debug.LogError($"{guid} & {fileID} has already exist.");

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

            public string MakeKey(string guid, long fileID)
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
        }

        /// <summary>
        /// 根据SoftObjectPath组件信息更新DB
        /// </summary>
        /// <param name="userAssetPath"></param>
        static public void ImportAsset(string userAssetPath)
        {
            if (userAssetPath.EndsWith(".unity"))
            {
                Scene activeScene = EditorSceneManager.GetActiveScene();

                Scene scene = EditorSceneManager.OpenScene(userAssetPath, OpenSceneMode.Additive);
                if (!scene.IsValid())
                    return;

                GameObject[] gos = scene.GetRootGameObjects();
                foreach (var go in gos)
                {
                    InternalImportAsset(go, AssetDatabase.AssetPathToGUID(userAssetPath));
                }

                if(scene != activeScene)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
            else if (userAssetPath.EndsWith(".prefab"))
            {
                InternalImportAsset(AssetDatabase.LoadAssetAtPath<GameObject>(userAssetPath), AssetDatabase.AssetPathToGUID(userAssetPath));
            }
        }

        /// <summary>
        /// 资源导入、保存时把其所有SoftObjectPath数据更新至redirector
        /// 遍历asset中的所有SoftObjectPath，更新其GUID,FILEID
        /// 更新流程：
        /// step 1. 检查userObject
        /// </summary>
        /// <param name="userObj"></param>
        /// <param name="userGUID"></param>
        static private void InternalImportAsset(GameObject userObj, string userGUID)
        {
            if (userObj == null)
                return;

            SoftObjectPath[] sopList = userObj.GetComponentsInChildren<SoftObjectPath>(true);
            if (sopList.Length == 0)
                return;

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
                }

                sri.AddOrUpdateUserInfo(userGUID, id);

                SerializeSoftReference(sop.m_GUID, sri);
            }
        }

        /// <summary>
        /// 删除资源触发DB更新，把正在引用此资源的SoftObjectPath清空
        /// </summary>
        /// <param name="deletedAssetPath"></param>
        static public bool DeleteAsset(string deletedAssetPath)
        {
            return InternalDeleteOrMoveAsset(deletedAssetPath, true);
        }

        /// <summary>
        /// 移动、改名触发DB更新
        /// </summary>
        /// <param name="newAssetPath"></param>
        static public bool MoveAsset(string newAssetPath)
        {
            return InternalDeleteOrMoveAsset(newAssetPath, false);
        }

        /// <summary>
        /// 被引用资源删除、移动、改名时触发引用资源更新数据(*.prefab, *.unity)
        /// </summary>
        /// <param name="refObjectAssetPath"></param>
        /// <param name="bDelete">true: 删除；false: 更新</param>
        /// <returns></returns>
        static private bool InternalDeleteOrMoveAsset(string refObjectAssetPath, bool bDelete)
        {
            string guid = AssetDatabase.AssetPathToGUID(refObjectAssetPath);

            string filePath = string.Format("{0}/{1}.json", s_RefObjectDBPath, guid);
            if (!File.Exists(filePath))
                return false;

            // deserialize redirector and update
            RefObjectInfo sri = DeserializeSoftReference(guid);
            sri.m_RefObjectGUID = guid;
            sri.m_RefObjectAssetPath = refObjectAssetPath;

            // 根据redirector记录的userInfo，逐个更新user data
            List<string> removeList = new List<string>();
            bool bModified = false;
            foreach (var item in sri.m_UserInfoList)
            {
                RefObjectInfo.UserInfo userInfo = item.Value;

                string userAssetPath = AssetDatabase.GUIDToAssetPath(userInfo.m_UserObjectGUID);
                if (string.IsNullOrEmpty(userAssetPath))
                {
                    removeList.Add(item.Key);
                    continue;
                }

                Object userObject = AssetDatabase.LoadAssetAtPath<Object>(userAssetPath);       // 被删除资源的GUID仍会返回一个路径，需要加载判断资源是否真实存在
                if (userObject == null)
                {
                    removeList.Add(item.Key);
                    continue;       // 可能userAssetPath有值，但对应的资源已删除
                }

                // prefab，unity分开处理
                if (userObject is GameObject)
                { // GameObject
                    bool bFind = false;
                    SoftObjectPath[] sopList = ((GameObject)userObject).GetComponentsInChildren<SoftObjectPath>(true);
                    foreach (var sop in sopList)
                    {
                        if (GetLocalID(sop) != userInfo.m_FileID)
                            continue;

                        sop.m_GUID = bDelete ? null : guid;
                        sop.m_AssetPath = bDelete ? null : refObjectAssetPath.ToLower();

                        UnityEditor.EditorUtility.SetDirty(userObject);
                        bModified = true;
                        bFind = true;

                        break;
                    }

                    if (!bFind)
                    {
                        removeList.Add(item.Key);
                    }
                }
                else if(userObject is SceneAsset)
                { // SceneAsset   
                    Scene activeScene = EditorSceneManager.GetActiveScene();

                    Scene scene = EditorSceneManager.OpenScene(userAssetPath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                    if (!scene.IsValid())
                        continue;
                    
                    bool bFind = false;
                    GameObject[] gos = scene.GetRootGameObjects();
                    foreach (var go in gos)
                    {
                        SoftObjectPath[] sopList = go.GetComponentsInChildren<SoftObjectPath>(true);
                        foreach (var sop in sopList)
                        {
                            if (GetLocalID(sop) != userInfo.m_FileID)
                                continue;

                            sop.m_GUID = bDelete ? null : guid;
                            sop.m_AssetPath = bDelete ? null : refObjectAssetPath.ToLower();

                            bModified = true;
                            bFind = true;

                            break;
                        }
                    }

                    if(bFind)
                    {
                        Debug.Log($"MarkSceneDirty:    {UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene)}");
                        Debug.Log($"SaveScene:    {UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene)}");
                    }

                    if(activeScene != scene)
                    {
                        Debug.Log($"CloseScene: {UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true)}");
                    }

                    // 场景内所有对象找不到再删除
                    if (!bFind)
                    {
                        removeList.Add(item.Key);
                    }
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

        static public void UnloadRefObject(string refObjectGUID, string userGUID, long fileID)
        {
            string filePath = string.Format("{0}/{1}.json", s_RefObjectDBPath, refObjectGUID);
            if (!File.Exists(filePath))
                return;

            // deserialize redirector and update
            RefObjectInfo sri = DeserializeSoftReference(refObjectGUID);
            sri.m_RefObjectGUID = refObjectGUID;
            sri.m_RefObjectAssetPath = AssetDatabase.GUIDToAssetPath(refObjectGUID);

            sri.Remove(userGUID, fileID);

            SerializeSoftReference(refObjectGUID, sri);
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
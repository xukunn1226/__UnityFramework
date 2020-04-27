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
                Debug.Log($"importedAsset: {importedAssets[i]}");
                RedirectorDB.ImportAsset(importedAssets[i]);
            }

            bool bDirty = false;
            for (int i = 0; i < deletedAssets.Length; ++i)
            {
                Debug.Log($"deletedAssets: {deletedAssets[i]}");
                bDirty |= RedirectorDB.DeleteAsset(deletedAssets[i]);
            }

            for (int i = 0; i < movedAssets.Length; ++i)
            {
                Debug.Log($"movedAssets: {movedAssets[i]}       movedFromAssetPaths: {movedFromAssetPaths[i]}");
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
        static private string k_SavedPath = "Assets/RedirectorDB";

        static RedirectorDB()
        {
            Directory.CreateDirectory(k_SavedPath);
        }

        /// <summary>
        /// 重定向器，记录资源被引用信息
        /// </summary>
        public class SoftRefRedirector
        {
            public string       m_RefObjectGUID;                // 被引用资源的GUID
            public string       m_RefObjectAssetPath;           // 资源路径（与GUID对应，方便DEBUG）

            public class UserInfo
            {
                public string   m_UserObjectGUID;               // 使用此资源的GUID
                public long     m_FileID;                       // FileID，用于定位SoftObjectPath
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
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="userGUID"></param>
        static private void InternalImportAsset(GameObject asset, string userGUID)
        {
            if (asset == null)
                return;

            SoftObjectPath[] sopList = asset.GetComponentsInChildren<SoftObjectPath>(true);
            if (sopList.Length == 0)
                return;

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

                sri.AddOrUpdateUserInfo(userGUID, GetLocalID(sop));

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

            string filePath = string.Format("{0}/{1}.json", k_SavedPath, guid);
            if (!File.Exists(filePath))
                return false;

            // deserialize redirector and update
            SoftRefRedirector sri = DeserializeSoftReference(guid);
            sri.m_RefObjectGUID = guid;
            sri.m_RefObjectAssetPath = refObjectAssetPath;

            // 根据redirector记录的userInfo，逐个更新user data
            List<string> removeList = new List<string>();
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

                Object userObject = AssetDatabase.LoadAssetAtPath<Object>(userAssetPath);
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
            string filePath = string.Format("{0}/{1}.json", k_SavedPath, refObjectGUID);
            if (!File.Exists(filePath))
                return;

            // deserialize redirector and update
            SoftRefRedirector sri = DeserializeSoftReference(refObjectGUID);
            sri.m_RefObjectGUID = refObjectGUID;
            sri.m_RefObjectAssetPath = AssetDatabase.GUIDToAssetPath(refObjectGUID);

            sri.Remove(userGUID, fileID);

            SerializeSoftReference(refObjectGUID, sri);
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
                if(Directory.Exists(k_SavedPath))
                    Directory.Delete(k_SavedPath, true);
                Directory.CreateDirectory(k_SavedPath);

                // reimport
                // 56fa9a21fe1ba864086c2d3328d79985 : SoftObjectPath
                // 5526957ebeb07bd4299f5213397a148b : SoftObject
                ReimportAllRedirectors(new string[] { "56fa9a21fe1ba864086c2d3328d79985", "5526957ebeb07bd4299f5213397a148b" }, new string[] { ".prefab", ".unity" });

                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// reimport all assets which reference the asset(guid)
        /// </summary>
        /// <param name="guids"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        static private void ReimportAllRedirectors(string[] guids, string[] filters)
        {
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                .Where(s => filters.Contains(Path.GetExtension(s).ToLower())).ToArray();

            int startIndex = 0;

            EditorApplication.update = delegate ()
            {
                string file = files[startIndex];

                bool isCancel = UnityEditor.EditorUtility.DisplayCancelableProgressBar("资源查找中", file, (float)startIndex / (float)files.Length);

                foreach (var guid in guids)
                {
                    if (Regex.IsMatch(File.ReadAllText(file), guid))
                    {
                        string assetPath = GetRelativeAssetsPath(file);
                        AssetDatabase.ImportAsset(assetPath);

                        //Debug.Log(file, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath));
                    }
                }

                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    UnityEditor.EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    Debug.Log("重导结束 in (*.prefab, *.unity, *.mat, *.asset)");
                }
            };
        }

        static private string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        }
    }
}
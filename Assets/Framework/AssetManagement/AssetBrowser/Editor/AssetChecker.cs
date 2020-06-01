using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.AssetManagement.AssetBrowser
{
    static public class AssetChecker
    {
        /// <summary>
        /// Input: AssetBundle Name
        /// 返回依赖的所有AB包（直接和间接引用）
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/AssetBrowser/Display All AssetBundle Dependencies", false, 30)]
        public static void MenuItem_GetAssetBundleDependencies()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            string assetBundleName = AssetDatabase.GetImplicitAssetBundleName(assetPath);

            string[] dependencies = AssetBrowserUtil.GetAllAssetBundleDependencies(assetBundleName, true);
            if(dependencies == null)
            {
                return;
            }

            Debug.LogFormat("Display All Dependencies of AssetBundle: [{0}]    count:[{1}]", assetBundleName, dependencies.Length);
            foreach (string dependency in dependencies)
            {
                Debug.LogFormat("---- AssetBundle Dependencies: {0}", dependency);
            }
        }

        [MenuItem("Assets/AssetBrowser/Display Direct AssetBundle Dependencies", false, 30)]
        public static void MenuItem_GetDirectAssetBundleDependencies()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            string assetBundleName = AssetDatabase.GetImplicitAssetBundleName(assetPath);

            string[] dependencies = AssetBrowserUtil.GetAllAssetBundleDependencies(assetBundleName, false);
            if (dependencies == null)
            {
                return;
            }

            Debug.LogFormat("Display Direct Dependencies of AssetBundle: [{0}]    count:[{1}]", assetBundleName, dependencies.Length);
            foreach (string dependency in dependencies)
            {
                Debug.LogFormat("---- AssetBundle Dependencies: {0}", dependency);
            }
        }

        /// <summary>
        /// Input: Asset Name
        /// 返回资源依赖的最小AssetBundle dependencies
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/AssetBrowser/Display Minimal AssetBundle Dependencies", false, 31)]
        public static void GetAssetBundleDependenciesFromAssetPath()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            string[] dependencies = AssetBrowserUtil.GetMinimalAllAssetBundleDependencies(assetPath);
            if(dependencies == null)
            {
                Debug.LogError("Plz select a asset");
                return;
            }

            Debug.LogFormat("Display Minimal AssetBundle Dependencies For Asset: {0}       count: [{1}]", assetPath, dependencies.Length);
            foreach (string dependency in dependencies)
            {
                Debug.LogFormat("---- AssetBundle Dependencies: {0}", dependency);
            }
        }

        /// <summary>
        /// 返回依赖的资源列表（直接和间接引用），但不会统计依赖的内置资源
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/AssetBrowser/Display All Dependencies(无法统计引用的内置资源)", false, 32)]
        public static void MenuItem_GetDependencies()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            if(AssetDatabase.IsValidFolder(assetPath))
            {
                Debug.LogError("Plz select a asset");
                return;
            }

            string[] dependencies = AssetBrowserUtil.GetAllDependencies(assetPath);
            Debug.LogFormat("Display All Dependencies of Asset: {0}    count:{1}", assetPath, dependencies.Length);
            foreach (string dependency in dependencies)
            {
                Debug.LogFormat("---- Asset Dependencies: {0}", dependency);
            }
        }

        // 检查当前选中对象的引用资源是否正确
        [MenuItem("Assets/AssetBrowser/Display Dependencies Validity", false, 33)]
        static public void MenuItem_CheckAll()
        {
            List<string> paths = new List<string>();
            for(int i = 0; i < Selection.assetGUIDs.Length; ++i)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]));
            }
            if (paths.Count > 0)
            {
                try
                {
                    UnityEditor.EditorUtility.DisplayProgressBar("Check Dependencies", "", 0);
                    for (int i = 0; i < paths.Count; ++i)
                    {
                        CheckResourceValid(paths[i]);
                        UnityEditor.EditorUtility.DisplayProgressBar("Check Dependencies", i + "/" + paths.Count, i / (float)paths.Count);
                    }
                    UnityEditor.EditorUtility.ClearProgressBar();
                    Debug.Log("Check Done.  Total asset: " + paths.Count);
                }
                catch (System.Exception e)
                {
                    UnityEditor.EditorUtility.ClearProgressBar();
                    Debug.LogError(e.Message);
                }
            }
            else
            {
                Debug.Log("CheckDependencies: No valid asset selected...");
            }
        }

        // 检查引用的资源是否合法（内置资源或被引用的资源没有被打成ab包）
        static public void CheckResourceValid(string assetPath)
        {
            List<string> dependencies = new List<string>();
            List<string> assetNames = new List<string>();
            List<int> builtin = new List<int>();
            List<int> external = new List<int>();

            bool canCheck = AssetBrowserUtil.CheckResource(assetPath, out dependencies, out assetNames, out builtin, out external);
            if(!canCheck)
            {
                Debug.LogError("资源检查失败，可能原因：1、资源不存在；2、资源没有被打AssetBundle");
                return;
            }

            Debug.LogFormat("------ Begin Check Resource: {0}    Count of dependencies: {1}", assetPath, dependencies.Count);
            for(int i = 0; i < dependencies.Count; ++i)
            {
                if(builtin.Contains(i))
                {
                    Debug.LogErrorFormat("引用内置资源: {0}     assetPath: {1}", assetNames[i], dependencies[i]);
                }
                else if(external.Contains(i))
                {
                    Debug.LogErrorFormat("引用外部资源:  {0}     asset path:{1}", assetNames[i], dependencies[i]);
                }
                else
                {
                    Debug.LogFormat("引用的资源：{0}    asset path: {1}", assetNames[i], dependencies[i]);
                }
            }
            Debug.Log("======= End Check Resource");
        }

        [MenuItem("Assets/AssetBrowser/Find Missing References In Assets", false, 20)]
        static public void MenuItem_FindSelectedAssetMissingReferences()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(AssetDatabase.IsValidFolder(assetPath))
            {
                Debug.LogError("Can't work on Folder");
                return;
            }

            string error;
            if(AssetBrowserUtil.FindMissingReference(Selection.activeObject, out error) != 0)
            {
                Debug.LogError(error);
            }
        }

        [MenuItem("Assets/AssetBrowser/Find Missing References In Scene", false, 21)]
        static public void MenuItem_FindMissingReferencesInScene()
        {
            string error;
            if(AssetBrowserUtil.FindMissingReferencesInScene(out error) != 0)
            {
                Debug.LogError(error);
            }
        }

        static private List<string> s_ReferenceFindingCommand = new List<string>();

        [MenuItem("Assets/AssetBrowser/Find References(*.prefab, *.unity, *.mat, *.asset)", false, 1)]
        static private void FindReferences()
        {
            s_ReferenceFindingCommand.Clear();

            UnityEngine.Object[] objs = Selection.objects;
            foreach (UnityEngine.Object obj in objs)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (AssetDatabase.IsValidFolder(path))
                {
                    string[] GUIDs = AssetDatabase.FindAssets("", new string[] {path});
                    foreach(var guid in GUIDs)
                    {
                        s_ReferenceFindingCommand.Add(AssetDatabase.GUIDToAssetPath(guid));
                    }
                }
                else
                {
                    s_ReferenceFindingCommand.Add(path);
                }
            }

            if(s_ReferenceFindingCommand.Count > 0)
                DoFindReference(0, new string[] {".prefab", ".unity", ".mat", ".asset"});
        }

        // 检查当前选中对象的引用资源是否正确
        [MenuItem("Assets/AssetBrowser/Find References(*.*)", false, 2)]
        static public void FindReferences2()
        {
            s_ReferenceFindingCommand.Clear();

            UnityEngine.Object[] objs = Selection.objects;
            foreach (UnityEngine.Object obj in objs)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (AssetDatabase.IsValidFolder(path))
                {
                    string[] GUIDs = AssetDatabase.FindAssets("", new string[] {path});
                    foreach(var guid in GUIDs)
                    {
                        s_ReferenceFindingCommand.Add(AssetDatabase.GUIDToAssetPath(guid));
                    }
                }
                else
                {
                    s_ReferenceFindingCommand.Add(path);
                }
            }

            if(s_ReferenceFindingCommand.Count > 0)
                DoFindReference(0);
        }

        static private void DoFindReference(int index, string[] extensions = null)
        {
            if(index >= s_ReferenceFindingCommand.Count)
                return;

            string path = s_ReferenceFindingCommand[index];

            if(string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
            {
                DoFindReference(index + 1, extensions);
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(path);
            // string[] withExtensions = new string[] { ".prefab", ".unity", ".mat", ".asset" };
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            if(extensions != null)
                files = files.Where(s => extensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            int startIndex = 0;

            Debug.Log($"Begin to find reference: {path}");
            int count = 0;
            EditorApplication.update = delegate ()
            {
                string file = files[startIndex];

                bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);

                if (Regex.IsMatch(File.ReadAllText(file), guid))
                {
                    ++count;
                    Debug.Log(file, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(file)));
                }

                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;

                    if(count > 1)
                        Debug.LogWarning($"查找结束 in (*.prefab, *.unity, *.mat, *.asset)      Count: {count}");
                    else
                        Debug.Log($"查找结束 in (*.prefab, *.unity, *.mat, *.asset)     Count: {count}");

                    DoFindReference(index + 1, extensions);
                }
            };
        }

        static private string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        }
        [MenuItem("Assets/AssetBrowser/Fix Redundant Mesh of ParticleSystemRender", false, 3)]
        static private void FixRedundantMeshOfParticleSystemRender()
        {
            List<string> assetPaths = AssetBrowserUtil.GetSelectedAllPaths(".prefab");
            foreach(string assetPath in assetPaths)
            {
                Debug.Log("Fix ParticleSystemRender: " + assetPath);
                AssetBrowserUtil.FixRedundantMeshOfParticleSystemRender(assetPath);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Fix Redundant Mesh of ParticleSystemRender is Done.  " + assetPaths.Count);
        }
    }
}
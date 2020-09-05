using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

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
                StartFindReference(new string[] {".prefab", ".unity", ".mat", ".asset"});
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
                StartFindReference();
        }

        static private EditorCoroutine m_Coroutine;
        static private void StartFindReference(string[] extensions = null)
        {
            m_Coroutine = EditorCoroutineUtility.StartCoroutineOwnerless(DoFindReference(extensions));
        }

        static private void StopFindReference()
        {
            if(m_Coroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }
        }

        static private IEnumerator DoFindReference(string[] extensions = null)
        {
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            int startIndex = 0;
            while(startIndex < s_ReferenceFindingCommand.Count)
            {
                string path = s_ReferenceFindingCommand[startIndex];
                if(string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
                {
                    ++startIndex;
                    yield return null;
                }
                else
                {
                    string[] filesWithFilter = files;
                    if(extensions != null)
                        filesWithFilter = files.Where(s => extensions.Contains(Path.GetExtension(s).ToLower())).ToArray();

                    Debug.Log($"Begin to find reference: {path}");
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    int index = 0;
                    int count = 0;
                    while(index < filesWithFilter.Length)
                    {
                        bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", filesWithFilter[index], (float)index / (float)filesWithFilter.Length);

                        if (Regex.IsMatch(File.ReadAllText(filesWithFilter[index]), guid))
                        {
                            ++count;
                            Debug.Log(filesWithFilter[index], AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(filesWithFilter[index])));
                        }

                        index++;
                        if (isCancel || index >= filesWithFilter.Length)
                        {
                            EditorUtility.ClearProgressBar();

                            if(count > 1)
                                Debug.LogWarning($"查找结束 in (*.prefab, *.unity, *.mat, *.asset)      被引用次数: {count}");
                            else
                                Debug.Log($"查找结束 in (*.prefab, *.unity, *.mat, *.asset)     被引用次数: {count}");
                            break;
                        }
                    }

                    ++startIndex;
                    yield return null;
                }
            }

            StopFindReference();
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

        [MenuItem("Assets/AssetBrowser/Clean Property on All Materials", false, 4)]
        static private void MemuItem_CleanAllMaterials()
        {
            if(!EditorUtility.DisplayDialog("", "耗时操作，预计2分钟", "OK", "Cancel"))
                return;

            var allMaterials = AssetDatabase.FindAssets("t:Material");
            for (int i = 0; i < allMaterials.Length; ++i)
            {
                var matPath = AssetDatabase.GUIDToAssetPath(allMaterials[i]);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

                if (mat)
                {
                    CleanMaterialProperties(mat);

                    EditorUtility.SetDirty(mat);
                }
            }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/AssetBrowser/Clean Property on Selected Material", false, 5)]
        static private void MemuItem_CleanMaterial()
        {
            List<string> assetPaths = AssetBrowserUtil.GetSelectedAllPaths(".mat");
            foreach(string assetPath in assetPaths)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if(mat != null)
                {
                    CleanMaterialProperties(mat);
                }
            }
            AssetDatabase.SaveAssets();
        }

        private static void CleanMaterialProperties(Material mat)
        {
            SerializedObject source = new SerializedObject(mat);
            SerializedProperty savedProperties = source.FindProperty("m_SavedProperties");
            SerializedProperty texEnvs = savedProperties.FindPropertyRelative("m_TexEnvs");
            SerializedProperty floats = savedProperties.FindPropertyRelative("m_Floats");
            SerializedProperty colors = savedProperties.FindPropertyRelative("m_Colors");

            CleanMaterialsSerializedProperty(texEnvs, mat);
            CleanMaterialsSerializedProperty(floats, mat);
            CleanMaterialsSerializedProperty(colors, mat);
            source.ApplyModifiedProperties();            
        }

        private static void CleanMaterialsSerializedProperty(SerializedProperty property, Material mat)
        {
            for(int i = property.arraySize - 1; i >= 0; --i)
            {
                string name = property.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
                if (!mat.HasProperty(name))
                    property.DeleteArrayElementAtIndex(i);
            }
        }

        static private bool DoCleanMotionVectors(GameObject obj)
        {
            if (obj == null)
                return false;

            bool isDirty = false;
            SkinnedMeshRenderer[] smrs = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var smr in smrs)
            {
                if (smr != null && smr.skinnedMotionVectors)
                {
                    isDirty = true;
                    smr.skinnedMotionVectors = false;
                    EditorUtility.SetDirty(smr);
                }
            }
            return isDirty;
        }

        [MenuItem("Assets/AssetBrowser/Clean MotionVectors On Selected GameObject", false, 6)]
        static private void CleanSelectedMotionVectors()
        {
            bool isDirty = false;
            List<string> assetPaths = AssetBrowserUtil.GetSelectedAllPaths(".prefab");
            foreach(var assetPath in assetPaths)
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                isDirty |= DoCleanMotionVectors(obj);
            }
            if(isDirty)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("Assets/AssetBrowser/Clean MotionVectors On All GameObjects", false, 7)]
        static private void CleanAllMotionVectors()
        {
            bool isDirty = false;
            string[] guids = AssetDatabase.FindAssets("t:prefab");
            foreach(var guid in guids)
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                isDirty |= DoCleanMotionVectors(obj);
            }
            if(isDirty)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}
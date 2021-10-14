using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.IO;

namespace Framework.AssetManagement.AssetBrowser
{
    static internal class AssetBrowserUtil
    {
        /// <summary>
        /// 返回AB包依赖的AB包列表
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public string[] GetAllAssetBundleDependencies(string assetBundleName, bool recursive)
        {
            return AssetDatabase.GetAssetBundleDependencies(assetBundleName, recursive);
        }

        /// <summary>
        /// 返回资源依赖的AB包列表，不支持文件夹
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public string[] GetMinimalAllAssetBundleDependencies(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return null;
            }

            // 获取依赖的资源列表（包含自身）
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);
            if (dependencies == null || dependencies.Length == 0)
                return null;

            HashSet<string> assetBundleNames = new HashSet<string>();

            foreach (var dependency in dependencies)
            {
                if (string.Compare(assetPath, dependency, true) == 0)
                    continue;       // 忽略自身

                string assetBundleName = AssetDatabase.GetImplicitAssetBundleName(dependency);
                if (!string.IsNullOrEmpty(assetBundleName))
                {
                    assetBundleNames.Add(assetBundleName);
                }
            }
            return new List<string>(assetBundleNames).ToArray();
        }

        /// <summary>
        /// 获取直接依赖的资源列表
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public string[] GetDirectDependencies(string assetPath)
        {
            return AssetDatabase.GetDependencies(assetPath, false);
        }

        /// <summary>
        /// 获取所有依赖的资源列表（排除自身）
        /// WARNING: 不会统计依赖的内置资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static public string[] GetAllDependencies(string assetPath)
        {
            List<string> dependencies = new List<string>(AssetDatabase.GetDependencies(assetPath, true));
            dependencies.RemoveAll(x => string.Compare(x, assetPath) == 0);
            return dependencies.ToArray();
        }

        /// <summary>
        /// 检查资源的合法性，可以检测出依赖的内置资源和外部资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="dependencies">所有依赖列表</param>
        /// <param name="builtin">依赖的内置资源列表</param>
        /// <param name="external">依赖的外部资源列表</param>
        /// <returns>true: 资源可以被检查；false：资源不合法，不被检查</returns>
        static public bool CheckResource(string assetPath, out List<string> dependencies, out List<string> assetNames, out List<int> builtin, out List<int> external)
        {
            dependencies = new List<string>();
            assetNames = new List<string>();
            builtin = new List<int>();
            external = new List<int>();

            string assetBundleName = AssetDatabase.GetImplicitAssetBundleName(assetPath);
            if (string.IsNullOrEmpty(assetBundleName))
            {
                return false;  // 没有AB名的资源不做检查
            }

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                return false;
            }

            // 收集所有依赖，包含了脚本、dll和编辑器资源，需要去除
            UnityEngine.Object[] objs = UnityEditor.EditorUtility.CollectDependencies(new UnityEngine.Object[] { asset });
            foreach (UnityEngine.Object o in objs)
            {
                if (o == null)
                    continue;

                string dependentAssetPath = AssetDatabase.GetAssetPath(o);
                dependencies.Add(dependentAssetPath);
                assetNames.Add(o.name);

                AssetImporter subObjectImporter = AssetImporter.GetAtPath(dependentAssetPath);
                if (subObjectImporter == null)
                {
                    builtin.Add(dependencies.Count - 1);
                }
                else
                { // 判断是否是外部资源                    
                    if (subObjectImporter.GetType().Name == "MonoImporter")        // 脚本不处理
                        continue;

                    if (string.IsNullOrEmpty(AssetDatabase.GetImplicitAssetBundleName(subObjectImporter.assetPath)))
                    {
                        external.Add(dependencies.Count - 1);
                    }
                }
            }

            return true;
        }

        private static PropertyInfo objRefValueMethod = typeof(SerializedProperty).GetProperty("objectReferenceStringValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// find missing references in scene objects or assets on disk
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="error"></param>
        /// <returns>0: no missing reference; -1: has missing reference</returns>
        static public int FindMissingReference(UnityEngine.Object obj, out string error)
        {
            if (!obj)
            {
                error = "obj == null";
                return -1;
            }

            error = string.Empty;

            int count = 1;
            GameObject go = obj as GameObject;
            if (go != null)
            {
                Component[] comps = go.GetComponentsInChildren<Component>(true);
                foreach (Component comp in comps)
                {
                    if (!comp)
                    {
                        error += string.Format("[{0}] Maybe missing mono script, plz check it.\n", count++);
                        continue;
                    }

                    SerializedObject so = new SerializedObject(comp);
                    SerializedProperty it = so.GetIterator();
                    while (it.NextVisible(true))
                    {
                        if (it.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            string objectReferenceStringValue = (string)objRefValueMethod.GetGetMethod(true).Invoke(it, new object[] { });

                            if (it.objectReferenceValue == null
                                && (it.objectReferenceInstanceIDValue != 0 || objectReferenceStringValue.StartsWith("Missing")))
                            {
                                error += string.Format("[{0}] Missing Reference in: {1}\nComponent: {2}({3})\nProperty: {4} \n", count++, GetFullPath(go), comp.name, comp.GetType().Name, ObjectNames.NicifyVariableName(it.name));
                            }
                        }
                    }
                }
            }
            else
            {
                SerializedObject so = new SerializedObject(obj);
                SerializedProperty it = so.GetIterator();
                while (it.NextVisible(true))
                {
                    if (it.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        string objectReferenceStringValue = (string)objRefValueMethod.GetGetMethod(true).Invoke(it, new object[] { });

                        if (it.objectReferenceValue == null
                            && (it.objectReferenceInstanceIDValue != 0 || objectReferenceStringValue.StartsWith("Missing")))
                        {
                            error += string.Format("[{0}] Missing Reference in: {1}    Property: {2} \n", count++, GetFullPath(obj), ObjectNames.NicifyVariableName(it.name));
                        }
                    }
                }
            }
            return string.IsNullOrEmpty(error) ? 0 : -1;
        }

        static private string GetFullPath(UnityEngine.Object go)
        {
            return EditorUtility.IsPersistent(go) ? AssetDatabase.GetAssetPath(go) : GetFullPathInScene(go as GameObject);
        }

        static private string GetFullPathInScene(GameObject go)
        {
            return go.transform.parent == null ? go.name : GetFullPathInScene(go.transform.parent.gameObject) + "/" + go.name;
        }

        static private GameObject[] GetSceneObjects()
        {
            return Resources.FindObjectsOfTypeAll<GameObject>().Where(go => !EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)).ToArray();
        }

        static public int FindMissingReferencesInScene(out string error)
        {
            error = string.Empty;

            GameObject[] gos = GetSceneObjects();
            foreach (GameObject go in gos)
            {
                string e;
                FindMissingReference(go, out e);

                error += e;
            }
            return string.IsNullOrEmpty(error) ? 0 : -1;
        }

        /// <summary>
        /// 替换材质shader
        /// </summary>
        /// <param name="assetPath">资产路径</param>
        /// <param name="oldShaderName">材质将被替换的shader name</param>
        /// <param name="newShaderName">new shader name</param>
        /// <returns></returns>
        static public bool ReplaceShader(string assetPath, string oldShaderName, string newShaderName)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null)
            {
                return false;
            }

            if(string.Compare(mat.shader.name.ToLower(), oldShaderName.ToLower()) != 0)
            {
                Debug.LogWarningFormat("ReplaceShader: can't find the shader name with {0}", oldShaderName);
                return false;
            }

            Shader newShader = Shader.Find(newShaderName);
            if (newShader == null)
            {
                Debug.LogError("Can't find Shader[" + newShaderName + "]");
                return false;
            }

            mat.shader = newShader;
            EditorUtility.SetDirty(mat);

            return true;
        }

        /// <summary>
        /// 替换mesh，仅限prefab的meshfilter组件
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="oldMeshName"></param>
        /// <param name="newMesh"></param>
        /// <returns></returns>
        static public bool ReplaceMesh(string assetPath, string oldMeshName, Mesh newMesh)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if(prefab == null)
            {
                return false;
            }

            // mesh filter possible includes mesh
            MeshFilter[] mfs = prefab.GetComponentsInChildren<MeshFilter>(true);
            foreach(var mf in mfs)
            {
                if (mf == null || mf.sharedMesh == null)
                    continue;

                if(string.Compare(mf.sharedMesh.name.ToLower(), oldMeshName.ToLower()) == 0)
                {
                    mf.sharedMesh = newMesh;
                }
            }

            // psr possible includes mesh
            ParticleSystemRenderer[] psrs = prefab.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach(var psr in psrs)
            {
                if (psr == null)
                    continue;

                Mesh[] meshes = new Mesh[psr.meshCount];
                int size = psr.GetMeshes(meshes);
                for(int i = 0; i < size; ++i)
                {
                    if (meshes[i] == null)
                        continue;

                    if(string.Compare(meshes[i].name.ToLower(), oldMeshName.ToLower()) == 0)
                    {
                        meshes[i] = newMesh;
                    }
                }
                psr.SetMeshes(meshes);
            }

            // mesh collider possible includes mesh
            MeshCollider[] mcs = prefab.GetComponentsInChildren<MeshCollider>(true);
            foreach(var mc in mcs)
            {
                if (mc == null || mc.sharedMesh == null)
                    continue;

                if(string.Compare(mc.sharedMesh.name.ToLower(), oldMeshName.ToLower()) == 0)
                {
                    mc.sharedMesh = newMesh;
                }
            }

            EditorUtility.SetDirty(prefab);
            return true;
        }

        /// <summary>
        /// 替换依赖的texture
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="newTexture"></param>
        /// <returns></returns>
        static public bool ReplaceTexture(string assetPath, string oldTextureName, Texture2D newTexture)
        {
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if(asset == null)
            {
                return false;
            }

            var serializedObject = new SerializedObject(asset);

            PropertyInfo inspectorMode = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            inspectorMode.SetValue(serializedObject, InspectorMode.Debug, null);

            var it = serializedObject.GetIterator();
            while (it.NextVisible(true))
            {
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
                {
                    if(string.Compare(it.objectReferenceValue.name.ToLower(), oldTextureName.ToLower()) == 0)
                    {
                        it.objectReferenceValue = newTexture;
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
            return true;
        }

        /// <summary>
        /// 替换prefab的材质
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="oldMaterialName"></param>
        /// <param name="newMaterial"></param>
        /// <returns></returns>        
        static public bool ReplaceMaterial(string assetPath, string oldMaterialName, Material newMaterial)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                return false;
            }

            Renderer[] rdrs = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (var rdr in rdrs)
            {
                if (rdr == null)
                    continue;

#if UNITY_2018_1_OR_NEWER
                UnityEngine.Object parent = PrefabUtility.GetCorrespondingObjectFromSource(rdr);
#else
                UnityEngine.Object parent = PrefabUtility.GetPrefabParent(rdr);
#endif
                if (parent != null && AssetDatabase.GetAssetPath(parent) != assetPath)
                {
                    continue;       // 嵌套其他prefab时不处理
                }

                if (rdr.sharedMaterial != null && string.Compare(rdr.sharedMaterial.name.ToLower(), oldMaterialName.ToLower()) == 0)
                {
                    rdr.sharedMaterial = newMaterial;
                }
            }
            EditorUtility.SetDirty(prefab);

            return true;
        }

        /// <summary>
        /// 遍历所有UILabel组件中的Font引用，进行替换
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="newFont"></param>
        /// <returns></returns>
        static public bool ReplaceFont(string assetPath, Font newFont)
        {
#if USE_NGUI
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if(prefab == null)
            {
                return true;
            }

            UILabel[] labels = prefab.GetComponentsInChildren<UILabel>(true);
            foreach(UILabel label in labels)
            {
                if (label.trueTypeFont == null)
                    continue;
                
                string path = AssetDatabase.GetAssetPath(label.trueTypeFont);
                if (string.Compare(path.ToLower(), ("Library/unity default resources").ToLower()) == 0)
                {
                    label.trueTypeFont = newFont;
                }
            }
            EditorUtility.SetDirty(prefab);
#endif
            return true;
        }

        static public string GetRelativePathToProjectFolder(string fullPath)
        {
            string projectFolder = UnityEngine.Application.dataPath.Replace("Assets", "");
            if (fullPath.StartsWith(projectFolder, System.StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(projectFolder.Length);
            }
            return fullPath;
        }

        // 删除ParticleSystemRender中已不用但仍引用着的mesh
        static public void FixRedundantMeshOfParticleSystemRender(string assetPath, bool makeDirty = true)
        {
            string ext = Path.GetExtension(assetPath);
            if(string.IsNullOrEmpty(ext) || string.Compare(ext, ".prefab") != 0)
                return;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
                return;

            bool bDeal = false;
            ParticleSystemRenderer[] psrs = prefab.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach(var psr in psrs)
            {
                if(psr.renderMode != ParticleSystemRenderMode.Mesh)
                {
                    bDeal = true;
                    psr.SetMeshes(new Mesh[] { });
                }
            }

            if (bDeal && makeDirty)
                EditorUtility.SetDirty(prefab);
        }

        // 根据扩展名筛选文件 e.g. ".fbx", ".prefab", ".asset", "*.*"
        static internal List<string> GetSelectedAllPaths(string extension)
        {
            List<string> paths = new List<string>();
            bool bAll = string.Compare(extension, "*.*", System.StringComparison.OrdinalIgnoreCase) == 0 ? true : false;

            UnityEngine.Object[] objs = Selection.objects;
            foreach (UnityEngine.Object obj in objs)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (AssetDatabase.IsValidFolder(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    FileInfo[] files = di.GetFiles(bAll ? extension : "*" + extension, SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; ++i)
                    {
                        path = files[i].FullName.Replace("\\", "/").Replace(UnityEngine.Application.dataPath, "Assets");
                        if (ValidExtension(path))
                        {
                            paths.Add(path);
                        }
                    }
                }
                else
                {
                    if (bAll || path.IndexOf(extension, System.StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        if (ValidExtension(path))
                        {
                            paths.Add(path);
                        }
                    }
                }
            }

            return paths;
        }

        static private bool ValidExtension(string filePath)
        {
            if (filePath.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase) || filePath.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }
    }
}


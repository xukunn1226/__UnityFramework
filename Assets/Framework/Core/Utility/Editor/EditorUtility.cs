using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

namespace Framework.Core.Editor
{
    public class EditorUtility
    {
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

        [MenuItem("Assets/Misc/List MemoryStat")]
        static private void MenuItem_ListMemoryStat()
        {
            List<string> assetPaths = GetSelectedAllPaths(".prefab");
            foreach(var assetPath in assetPaths)
                ListMemoryStat(AssetDatabase.LoadAssetAtPath<GameObject>(assetPath));
        }

        static private void ListMemoryStat(GameObject asset)
        {
            MeshFilter[] mfs = asset.GetComponentsInChildren<MeshFilter>(true);            
            SkinnedMeshRenderer[] smrs = asset.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            int vertexCount = 0;
            int triangleCount = 0;
            HashSet<int> meshInstanceIDs = new HashSet<int>();
            foreach(var mf in mfs)
            {
                if (mf.sharedMesh == null) continue;

                if (meshInstanceIDs.Contains(mf.sharedMesh.GetInstanceID())) continue;

                meshInstanceIDs.Add(mf.sharedMesh.GetInstanceID());

                vertexCount += mf.sharedMesh.vertexCount;
                triangleCount += mf.sharedMesh.triangles.Length;
                Debug.LogFormat("Mesh: {0}      Vertex: {1}     Triangle: {2}", mf.sharedMesh.name, mf.sharedMesh.vertexCount, mf.sharedMesh.triangles.Length);
            }
            foreach (var smr in smrs)
            {
                if (smr.sharedMesh == null) continue;

                if (meshInstanceIDs.Contains(smr.sharedMesh.GetInstanceID())) continue;

                meshInstanceIDs.Add(smr.sharedMesh.GetInstanceID());

                vertexCount += smr.sharedMesh.vertexCount;
                triangleCount += smr.sharedMesh.triangles.Length;
                Debug.LogFormat("Mesh: {0}      Vertex: {1}     Triangle: {2}", smr.sharedMesh.name, smr.sharedMesh.vertexCount, smr.sharedMesh.triangles.Length);
            }
            Debug.LogFormat("-----------Mesh Total: Vertex: {0}     Triangle: {1}", vertexCount, triangleCount);



            float megaBytes = 0;
            HashSet<int> texInstanceIDs = new HashSet<int>();
            Renderer[] rdrs = asset.GetComponentsInChildren<Renderer>(true);
            foreach(var rdr in rdrs)
            {
                foreach(var mat in rdr.sharedMaterials)
                {
                    if (mat == null) continue;

                    SerializedObject so = new SerializedObject(mat);
                    SerializedProperty it = so.GetIterator();
                    while(it.NextVisible(true))
                    {
                        if(it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
                        {
                            if (it.objectReferenceValue is Texture2D)
                            {
                                if (texInstanceIDs.Contains(it.objectReferenceInstanceIDValue)) continue;
                                texInstanceIDs.Add(it.objectReferenceInstanceIDValue);

                                string assetPath = AssetDatabase.GetAssetPath(it.objectReferenceValue);
                                Texture2D tex = it.objectReferenceValue as Texture2D;
                                TextureImporter ti = TextureImporter.GetAtPath(assetPath) as TextureImporter;
                                
                                float bytes = GetBytes(tex.width, tex.height, ti.DoesSourceTextureHaveAlpha());
                                Debug.LogFormat("Texture: {0}     Width: {1}  Height: {2}     Memory(ETC2): {3}M", assetPath, tex.width, tex.height, bytes);

                                megaBytes += bytes;
                            }
                        }
                    }
                }
            }
            Debug.LogFormat("-----------Texture Total: {0}M", megaBytes);
        }

        static private float GetBytes(int width, int height, bool isRGBA)
        {
            float megaBytes = width * height * (isRGBA ? 32 : 24) * 1.0f / (8 * 1024 * 1024);
            megaBytes /= 8;
            return megaBytes;
        }

        static public T GetOrCreateEditorConfigObject<T>(string directoryPath) where T : UnityEngine.ScriptableObject
        {
            //name of config data object
            string stringName = "com.myproject." + typeof(T).Name;

            //path to Config Object and asset name
            string stringPath = string.Format("{0}{1}{2}{3}", directoryPath.TrimEnd(new char[] { '/' }), "/", typeof(T).Name, ".asset");

            //used to hold config data
            T data = null;

            //if a config data object exists with the same name, return its config data
            if (EditorBuildSettings.TryGetConfigObject<T>(stringName, out data))
                return data;

            //If the asset file already exists, store existing config data
            if (System.IO.File.Exists(stringPath))
                data = AssetDatabase.LoadAssetAtPath<T>(stringPath);

            //if no previous config data exists
            if (data == null)
            {
                //initialise config data object
                data = ScriptableObject.CreateInstance<T>();
                //create new asset from data at specified path
                //asset MUST be saved with the AssetDatabase before adding to EditorBuildSettings
                AssetDatabase.CreateAsset(data, stringPath);
            }

            //add the new or loaded config object to EditorBuildSettings
            EditorBuildSettings.AddConfigObject(stringName, data, true);

            return data;
        }

        // 复制源目录下所有文件至目标目录（深度遍历）
        static public void CopyDirectory(string sourcePath, string destinationPath)
        {
            DirectoryInfo info = new DirectoryInfo(sourcePath);
            Directory.CreateDirectory(destinationPath);
            foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
            {
                string destName = Path.Combine(destinationPath, fsi.Name);

                if (fsi is System.IO.FileInfo)          //如果是文件，复制文件
                {
                    if(!fsi.FullName.EndsWith(".meta"))
                        File.Copy(fsi.FullName, destName);
                }
                else                                    //如果是文件夹，新建文件夹，递归
                {
                    CopyDirectory(fsi.FullName, destName);
                }
            }
        }

        // 复制sourcePath目录下的所有资源至目标目录，并逐文件添加后缀名suffix
        static public void CopyUnityAsset(string sourcePath, string destinationPath, string suffix = null)
        {
            DirectoryInfo srcDir = new DirectoryInfo(sourcePath);
            if(!srcDir.Exists)
                throw new System.InvalidOperationException($"The path ({sourcePath}) not exists");

            Directory.CreateDirectory(destinationPath);

            foreach(FileSystemInfo fsi in srcDir.GetFileSystemInfos())
            {
                if(fsi is System.IO.FileInfo)
                {
                    if(!fsi.FullName.EndsWith(".meta"))
                    {
                        string oldPath = Utility.GetProjectPath(fsi.FullName);
                        string newPath = string.Format($"{Utility.GetProjectPath(destinationPath)}/{fsi.Name + (string.IsNullOrEmpty(suffix) ? "" : suffix)}");
                        AssetDatabase.CopyAsset(oldPath, newPath);
                    }
                }
                else
                {
                    string oldPath = Utility.GetProjectPath(fsi.FullName);
                    string newPath = Utility.GetProjectPath(Path.Combine(destinationPath, fsi.Name));
                    CopyUnityAsset(oldPath, newPath, suffix);
                }
            }
        }

        static public int Exec(string filename, string args)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = args;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            int exit_code = -1;

            try
            {
                process.Start();

                if (process.StartInfo.RedirectStandardOutput && process.StartInfo.RedirectStandardError)
                {
                    process.BeginOutputReadLine();
                    Debug.LogError(process.StandardError.ReadToEnd());
                }
                else if (process.StartInfo.RedirectStandardOutput)
                {
                    string data = process.StandardOutput.ReadToEnd();
                    Debug.Log(data);
                }
                else if (process.StartInfo.RedirectStandardError)
                {
                    string data = process.StandardError.ReadToEnd();
                    Debug.LogError(data);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return -1;
            }

            process.WaitForExit();
            exit_code = process.ExitCode;
            process.Close();
            return exit_code;
        }
    }
}
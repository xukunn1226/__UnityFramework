using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Text;
#endif

namespace Application.Runtime
{
    public class TestPatch : MonoBehaviour
    {
        public static string ZIP_PATCH_FORMAT { get { return UnityEngine.Application.streamingAssetsPath + "/Patch_{0}.zip"; } }
        public static string RUNTIME_PATCH_PATH_FORMAT { get { return UnityEngine.Application.persistentDataPath + "/Patch_{0}"; } }
        
        void OnGUI()
        {
            GUI.Button(new Rect(200, 200, 500, 300), "This is Base Version");

            if(GUI.Button(new Rect(1000, 200, 200, 100), "Load Patch 1"))
            {
                StartCoroutine(PreparePatchAndRestart(1));
            }

            if(GUI.Button(new Rect(1000, 400, 200, 100), "Load Patch 2"))
            {
                // StartCoroutine(PreparePatchAndRestart(2));
            }

            if(GUI.Button(new Rect(1000, 600, 200, 100), "Revert to Base Version"))
            {
                string error = Bootstrap.use_data_dir("", "");
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError("use failed. empty path error:" + error);
                    return;
                }

                if(ClearCache())
                {
                    StartCoroutine(Restart());
                }
            }
        }

        private IEnumerator PreparePatchAndRestart(int version)
        {
            Debug.Log("Step 1......");
            //1. clear files if exist
            string runtimePatchPath = string.Format(RUNTIME_PATCH_PATH_FORMAT, version);
            if (Directory.Exists(runtimePatchPath)) { Directory.Delete(runtimePatchPath, true); }
            Directory.CreateDirectory(runtimePatchPath);

            //2. extract files from zip
            string zipPatchFile = string.Format(ZIP_PATCH_FORMAT, version);
            WWW zipPatchFileReader = new WWW(zipPatchFile);
            while (!zipPatchFileReader.isDone) { yield return null; }
            if (zipPatchFileReader.error != null)
            {
                Debug.LogError($"failed to get zip patch file: {zipPatchFile}");
                yield break;
            }
            byte[] zipContent = zipPatchFileReader.bytes;
            ZipHelper.UnZipBytes(zipContent, runtimePatchPath, "", true);
            // ZipHelper.UnZip(ANDROID_PROJECT_PATH + "/Patch1.zip", ANDROID_PROJECT_PATH, "", true);
            Debug.Log("UnZipBytes...patch");
            yield return new WaitForSeconds(1);

            //3. prepare libil2cpp, unzip with name: libil2cpp.so.new
            string zipLibil2cppPath = runtimePatchPath + "/lib_" + Bootstrap.get_arch_abi() + "_libil2cpp.so.zip";
            if (!File.Exists(zipLibil2cppPath))
            {
                Debug.LogError($"file not found: {zipLibil2cppPath}");
                yield break;
            }
            ZipHelper.UnZip(zipLibil2cppPath, runtimePatchPath, "", true);
            Debug.Log("UnZip...so");
            yield return new WaitForSeconds(1);

            //4. tell libboostrap.so to use the right patch after reboot
            string apkPath = "";
            string error = Bootstrap.use_data_dir(runtimePatchPath, apkPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"use failed. path: {zipLibil2cppPath}   {error}");
                yield break;
            }

            //5. clear unity cache
            if(!ClearCache())
                yield break;

            yield return new WaitForSeconds(2);

            //6. reboot app
            yield return StartCoroutine(Restart());
        }

        private bool ClearCache()
        {
            string cacheDir = UnityEngine.Application.persistentDataPath + "/il2cpp";
            if (Directory.Exists(cacheDir))
            {
                Debug.Log("DeleteDirectory.............");
                DeleteDirectory(cacheDir);
            }
            
            if (Directory.Exists(cacheDir))
            {
                Debug.LogError("failed to delete pre Unity cached file. path:" + cacheDir);
                return false;
            }
            return true;
        }

        private IEnumerator Restart()
        {
            Debug.Log($"The patch is ready. Reboot");
            yield return new WaitForSeconds(3);
            Bootstrap.reboot_app();
        }

        private static void DeleteDirectory(string target_dir)
        {
            try
            {
                string[] files = Directory.GetFiles(target_dir);
                string[] dirs = Directory.GetDirectories(target_dir);

                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string dir in dirs)
                {
                    DeleteDirectory(dir);
                }

                Directory.Delete(target_dir, false);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

#if UNITY_EDITOR
    public class TestPatchEditor
    {
        public static readonly string PROJECT_DIR = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.Length - 7);
        public static readonly string ANDROID_PROJECT_PATH = PROJECT_DIR + "/Deployment";
        public static string SO_DIR_NAME = "lib";
        public static string ZIP_PATH = PROJECT_DIR + "/Assets/Framework/Core/Utility/Editor/zip.exe";

        [MenuItem("TestPatch/Generate Bin Patches", false, 103)]
        public static void GenerateBinPatches()
        {
            // GenerateBinPatches(ANDROID_PROJECT_PATH + "/MyProject", "Patch_0");
            GenerateBinPatchesEx(ANDROID_PROJECT_PATH + "/MyProject", "Patch_1");
        }

        [MenuItem("TestPatch/Unzip Bin Patches", false, 103)]
        public static void UnzipBinPatches()
        {
            ZipHelper.UnZip(ANDROID_PROJECT_PATH + "/Patch_1.zip", ANDROID_PROJECT_PATH, "", true);
        }
        
        public static bool GenerateBinPatches(string projectPath, string patchName)
        {
            projectPath = projectPath.TrimEnd(new char[] {'/', '\\'});
            if(!Directory.Exists(projectPath))
            {
                Debug.LogError($"not found directory: {projectPath}");
                return false;
            }
            
            string parentPath = projectPath.Substring(0, projectPath.LastIndexOf('/'));
            string assetBinDataPath = projectPath + "/assets/bin/Data";
            string patchTopPath = parentPath + "/" + patchName;
            string assertBinDataPatchPath = patchTopPath + "/assets_bin_Data";

            if (Directory.Exists(patchTopPath)) { FileUtil.DeleteFileOrDirectory(patchTopPath); }
            Directory.CreateDirectory(assertBinDataPatchPath);

            string[][] soPatchFile =
            {
                // path_in_android_project, filename inside zip, zip file anme
                new string[3]{ "/"+ SO_DIR_NAME + "/armeabi-v7a/libil2cpp.so", "libil2cpp.so.new", "lib_armeabi-v7a_libil2cpp.so.zip" },
                new string[3]{ "/"+ SO_DIR_NAME + "/arm64-v8a/libil2cpp.so", "libil2cpp.so.new", "lib_arm64-v8a_libil2cpp.so.zip" },
            };

            for (int i = 0; i < soPatchFile.Length; i++)
            {
                string[] specialPaths       = soPatchFile[i];
                string projectRelativePath  = specialPaths[0];
                string pathInZipFile        = specialPaths[1];
                string zipFileName          = specialPaths[2];

                string projectFullPath = projectPath + "/" + projectRelativePath;
                ZipHelper.ZipFile(projectFullPath, pathInZipFile, patchTopPath + "/" + zipFileName, 9);
            }

            string[] allAssetsBinDataFiles = Directory.GetFiles(assetBinDataPath, "*", SearchOption.AllDirectories);
            foreach (string apk_file in allAssetsBinDataFiles)
            {
                string relativePathHeader = "assets/bin/Data";
                int relativePathStart = apk_file.IndexOf(relativePathHeader);
                string filenameInZip = apk_file.Substring(relativePathStart);                                                       //file: assets/bin/Data/xxx/xxx
                string relativePath = apk_file.Substring(relativePathStart + relativePathHeader.Length + 1).Replace('\\', '/');     //file: xxx/xxx
                string zipFileName = relativePath.Replace("/", "__").Replace("\\", "__") + ".bin";                                  //file: xxx__xxx.bin

                ZipHelper.ZipFile(apk_file, filenameInZip, assertBinDataPatchPath + "/" + zipFileName, 8);
            }
            ZipHelper.ZipFileDirectory(patchTopPath, parentPath + "/" + patchName + ".zip");
            return true;
        }

        public static bool GenerateBinPatchesEx(string projectPath, string patchName)
        {
            Debug.Log($"begin generate bin patches: {Time.timeSinceLevelLoad}");

            projectPath = projectPath.TrimEnd(new char[] {'/', '\\'});
            if(!Directory.Exists(projectPath))
            {
                Debug.LogError($"not found directory: {projectPath}");
                return false;
            }
            
            string parentPath = projectPath.Substring(0, projectPath.LastIndexOf('/'));
            string assetBinDataPath = projectPath + "/assets/bin/Data";
            string patchTopPath = parentPath + "/" + patchName;
            string assertBinDataPatchPath = patchTopPath + "/assets_bin_Data";

            if (Directory.Exists(patchTopPath)) { FileUtil.DeleteFileOrDirectory(patchTopPath); }
            Directory.CreateDirectory(assertBinDataPatchPath);

            string[][] soPatchFile =
            {
                // path_in_android_project, filename inside zip, zip file anme
                new string[3]{ "/"+ SO_DIR_NAME + "/armeabi-v7a/libil2cpp.so", "libil2cpp.so.new", "lib_armeabi-v7a_libil2cpp.so.zip" },
                new string[3]{ "/"+ SO_DIR_NAME + "/arm64-v8a/libil2cpp.so", "libil2cpp.so.new", "lib_arm64-v8a_libil2cpp.so.zip" },
            };

            for (int i = 0; i < soPatchFile.Length; i++)
            {
                string[] specialPaths       = soPatchFile[i];
                string projectRelativePath  = specialPaths[0];
                string pathInZipFile        = specialPaths[1];
                string zipFileName          = specialPaths[2];

                string projectFullPath = projectPath + "/" + projectRelativePath;
                ZipHelper.ZipFile(projectFullPath, pathInZipFile, patchTopPath + "/" + zipFileName, 9);
            }

            string[] allAssetsBinDataFiles = Directory.GetFiles(assetBinDataPath, "*", SearchOption.AllDirectories);
            StringBuilder allZipCmds = new StringBuilder();
            allZipCmds.AppendFormat("if not exist \"{0}\" (MD \"{0}\") \n", patchTopPath);
            allZipCmds.AppendFormat("if not exist \"{0}\" (MD \"{0}\") \n", assertBinDataPatchPath);
            foreach (string apk_file in allAssetsBinDataFiles)
            {
                string relativePathHeader = "assets/bin/Data";
                int relativePathStart = apk_file.IndexOf(relativePathHeader);
                string filenameInZip = apk_file.Substring(relativePathStart);                                                       //file: assets/bin/Data/xxx/xxx
                string relativePath = apk_file.Substring(relativePathStart + relativePathHeader.Length + 1).Replace('\\', '/');     //file: xxx/xxx
                string zipFileName = relativePath.Replace("/", "__").Replace("\\", "__") + ".bin";                                  //file: xxx__xxx.bin

                allZipCmds.AppendFormat("cd {0} && {1} -8 \"{2}\" \"{3}\"\n", projectPath, ZIP_PATH, assertBinDataPatchPath + "/" + zipFileName, filenameInZip);
            }
            allZipCmds.Append("sleep 1\n");
            allZipCmds.AppendFormat("cd {0} && {1} -9 -r \"{2}\" \"{3}\"\n", patchTopPath, ZIP_PATH, parentPath + "/" + patchName + ".zip", "*");

            if (allZipCmds.Length > 0)
            {
                string zipPatchesFile = parentPath + "/" + "zip_patches.bat";
                File.WriteAllText(zipPatchesFile, allZipCmds.ToString());
                if (Framework.Core.Editor.EditorUtility.Exec(zipPatchesFile, zipPatchesFile) != 0)
                {
                    Debug.LogError("exec failed:" + zipPatchesFile);
                    return false;
                }
            }
            Debug.Log($"end generate bin patches: {Time.timeSinceLevelLoad}");
            return true;
        }        
    }
#endif    
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System;
using System.IO;

namespace Application.Editor
{
    public class TestPatchEditor
    {
        public static readonly string PROJECT_DIR = UnityEngine.Application.dataPath.Substring(0, UnityEngine.Application.dataPath.Length - 7);
        public static readonly string ANDROID_PROJECT_PATH = PROJECT_DIR + "/Deployment";
        public static string SO_DIR_NAME = "lib";
        public static string ZIP_PATH = PROJECT_DIR + "/Assets/Framework/Core/Utility/Editor/zip.exe";

        [MenuItem("Tools/TestPatch/Generate Bin Patches", false, 103)]
        public static void GenerateBinPatches()
        {
            // GenerateBinPatches(ANDROID_PROJECT_PATH + "/MyProject", "Patch_0");
            GenerateBinPatchesEx(ANDROID_PROJECT_PATH + "/MyProject", "Patch_1");
        }

        [MenuItem("Tools/TestPatch/Unzip Bin Patches", false, 103)]
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
}
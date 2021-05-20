using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Application.Runtime
{
    public class TestPatch : MonoBehaviour
    {
        public static string ZIP_PATCH_FORMAT { get { return UnityEngine.Application.streamingAssetsPath + "/Patch_{0}.zip"; } }
        public static string RUNTIME_PATCH_PATH_FORMAT { get { return UnityEngine.Application.persistentDataPath + "/Patch_{0}"; } }
        
        void OnGUI()
        {
            GUI.Button(new Rect(200, 200, 500, 300), "Base Version");

            if(GUI.Button(new Rect(1000, 200, 200, 100), "Load Patch 1"))
            {
                StartCoroutine(PreparePatchAndRestart(1));
            }

            if(GUI.Button(new Rect(1000, 400, 200, 100), "Load Patch 2"))
            {
                StartCoroutine(PreparePatchAndRestart(2));
            }

            if(GUI.Button(new Rect(1000, 600, 200, 100), "Reset Patch"))
            {

            }
        }

        private IEnumerator PreparePatchAndRestart(int version)
        {
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
            Debug.Log("UnZipBytes...patch");

            //3. prepare libil2cpp, unzip with name: libil2cpp.so.new
            string zipLibil2cppPath = runtimePatchPath + "/lib_" + Bootstrap.get_arch_abi() + "_libil2cpp.so.zip";
            if (!File.Exists(zipLibil2cppPath))
            {
                Debug.LogError($"file not found: {zipLibil2cppPath}");
                yield break;
            }
            ZipHelper.UnZip(zipLibil2cppPath, runtimePatchPath, "", true);
            Debug.Log("UnZip...so");

            //4. tell libboostrap.so to use the right patch after reboot
            string apkPath = "";
            string error = Bootstrap.use_data_dir(runtimePatchPath, apkPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"use failed. path: {zipLibil2cppPath}   {error}");
                yield break;
            }

            //6. reboot app
            yield return StartCoroutine(Restart());
        }

        private IEnumerator Restart()
        {
            Debug.Log($"The patch is ready. Reboot");
            yield return new WaitForSeconds(1);
            Bootstrap.reboot_app();
        }
    }
}
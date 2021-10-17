// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;

// using System.IO;
// using System.Security.Cryptography;
// using System.Text;

// #if UNITY_2020_1_OR_NEWER
// using UnityEditor.AssetImporters;
// #else
// using UnityEditor.Experimental.AssetImporters;
// #endif


// [ScriptedImporter(2, new[] {"lua"})]
// public class LuaImporter : ScriptedImporter
// {
//     public static bool compile  = false;    // compile to lua byte code
//     public static bool strip    = false;    // strip lua debug info
//     public static bool encode   = true;

//     public override void OnImportAsset(AssetImportContext ctx)
//     {
//         var asset = LuaAsset.CreateInstance<LuaAsset>();
//         byte[] data;
//         if (compile)
//         {
//             // compile to lua byte code
//             var outDir = $"obj/{Path.GetDirectoryName(ctx.assetPath)}";
//             if (!Directory.Exists(outDir))
//                 Directory.CreateDirectory(outDir);
//             var outPath = $"obj/{ctx.assetPath}c";
//             #if UNITY_EDITOR_OSX
//             var luac = "build/luac/build_unix/luac";
//             #elif UNITY_EDITOR_WIN
//             var luac = "build/luac/build32/luac.exe";
//             #elif UNITY_EDITOR_WIN64
//             var luac = "build/luac/build64/luac.exe";
//             #endif
//             var prmt = $"{(strip ? "-s" : "")} -o {outPath} -- {ctx.assetPath}";
//             Compile(luac, prmt);
//             data = File.ReadAllBytes(outPath);
//         }
//         else
//         {
//             data = File.ReadAllBytes(ctx.assetPath);
//         }

//         // TODO: your encode function, like xxtea
//         if(encode)
//         {
//             data = Security.XXTEA.Encrypt(data, LuaAsset.LuaDecodeKey);
//         }
        
//         asset.data = data;
//         asset.encode = encode;
//         ctx.AddObjectToAsset("main obj", asset, LoadIconTexture(Path.GetExtension(ctx.assetPath).Substring(1)));
//         ctx.SetMainObject(asset);
//     }

//     private static void Compile(string exe, string prmt)
//     {
//         var process = new System.Diagnostics.Process();
//         try
//         {
//             var pi = new System.Diagnostics.ProcessStartInfo(exe, prmt);
//             pi.WorkingDirectory = ".";
//             pi.RedirectStandardInput = false;
//             pi.RedirectStandardOutput = true;
//             pi.RedirectStandardError = true;
//             pi.UseShellExecute = false;
//             pi.CreateNoWindow = true;

//             process.OutputDataReceived += (sender, e) =>
//             {
//                 if (string.IsNullOrEmpty(e.Data))
//                     return;
//                 UnityEngine.Debug.Log(e.Data);
//             };
//             process.ErrorDataReceived += (sender, e) =>
//             {
//                 if (!string.IsNullOrEmpty(e.Data))
//                     UnityEngine.Debug.LogError(e.GetType() + ": " + e.Data);
//             };
//             process.Exited += (sender, e) =>
//             {
//                 UnityEngine.Debug.Log($"{exe} {prmt} Exit");
//             };

//             process.StartInfo = pi;
//             process.EnableRaisingEvents = true;
//             process.Start();
//             process.BeginOutputReadLine();
//             process.BeginErrorReadLine();
//             process.WaitForExit();
//         }
//         catch (System.Exception e)
//         {
//             UnityEngine.Debug.LogError("catch: " + e);
//         }

//         // UnityEngine.Debug.Log("finished: " + process.ExitCode);
//         EditorUtility.ClearProgressBar();
//     }
    
//     private Texture2D LoadIconTexture(string prefix)
//     {
//         return AssetDatabase.LoadAssetAtPath("Assets/Application/XLua/Editor/lua.png", typeof(Texture2D)) as
//             Texture2D;
//     }
// }


// #if UNITY_EDITOR
// [CustomEditor(typeof(LuaAsset))]
// public class LuaAssetEditor : Editor
// {
//     private static bool decode = true;
//     private LuaAsset mTarget;

//     private void OnEnable()
//     {
//         mTarget = target as LuaAsset;
//     }

//     public override void OnInspectorGUI()
//     {
//         GUI.enabled = true;
//         EditorGUILayout.LabelField("Global Config (need reimport asset)");
//         {
//             EditorGUI.BeginChangeCheck();
//             ++EditorGUI.indentLevel;
//             LuaImporter.compile = EditorGUILayout.Toggle("compile to lua byte code", LuaImporter.compile);
//             if (LuaImporter.compile)
//             {
//                 ++EditorGUI.indentLevel;
//                 LuaImporter.strip = EditorGUILayout.Toggle("strip", LuaImporter.strip);
//                 --EditorGUI.indentLevel;
//             }

//             LuaImporter.encode = EditorGUILayout.Toggle("encode", LuaImporter.encode);
//             --EditorGUI.indentLevel;
//             if(EditorGUI.EndChangeCheck())
//             {
//                 string[] guids = AssetDatabase.FindAssets("t:LuaAsset", new string[] {XLuaConfig.s_LuaRootPath});
//                 foreach(var guid in guids)
//                 {
//                     AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid));
//                 }
//                 AssetDatabase.Refresh();
//             }
//         }
//         EditorGUILayout.Space();

//         EditorGUILayout.LabelField("Preview");
//         {
//             ++EditorGUI.indentLevel;
//             GUI.enabled = false;
//             EditorGUILayout.Toggle("encoded", mTarget.encode);
//             GUI.enabled = true;

//             if (mTarget.encode)
//                 decode = EditorGUILayout.Toggle("decode", decode);
//             --EditorGUI.indentLevel;
//         }

//         // draw lua script text
//         var text = string.Empty;
//         if (mTarget.encode && decode)
//         {
//             text = Encoding.UTF8.GetString(Security.XXTEA.Decrypt(mTarget.data, LuaAsset.LuaDecodeKey));
//         }
//         else
//         {
//             text = Encoding.UTF8.GetString(mTarget.data);
//         }

//         var MaxTextPreviewLength = 4096;
//         if (text.Length > MaxTextPreviewLength + 3)
//         {
//             text = text.Substring(0, MaxTextPreviewLength) + "...";
//         }

//         GUIStyle style = "ScriptText";
//         Rect rect = GUILayoutUtility.GetRect(new GUIContent(text), style);
//         rect.x = 0f;
//         rect.y -= 3f;
//         rect.width = EditorGUIUtility.currentViewWidth + 1f;
//         GUI.Box(rect, text, style);
//     }
// }
// #endif


// class AutoSetLabelForLuaScript : AssetPostprocessor
// {
//     static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
//     {
//         if(UnityEngine.Application.isBatchMode)
//             return;
        
//         foreach (string str in importedAssets)
//         {
//             if (str.ToLower().EndsWith(".lua"))
//             {
//                 var lua_obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(str);
 
//                 AssetDatabase.SetLabels(lua_obj, new string[] { "lua" });
//             }
//         }
//     }
// }

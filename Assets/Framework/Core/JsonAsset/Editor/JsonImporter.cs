using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using Framework.Core;

#if UNITY_2020_1_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace Framework.Core.Editor
{
    [ScriptedImporter(1, new[] { "zjson" })]
    public class JsonImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = ScriptableObject.CreateInstance<JsonAsset>();
            asset.data = File.ReadAllBytes(ctx.assetPath);
            ctx.AddObjectToAsset("main obj", asset);
            ctx.SetMainObject(asset);
        }
    }

    [CustomEditor(typeof(JsonAsset))]
    public class JsonAssetEditor : UnityEditor.Editor
    {
        private JsonAsset mTarget;

        private void OnEnable()
        {
            mTarget = target as JsonAsset;
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = true;
            var text = Encoding.UTF8.GetString(mTarget.data); ;

            var MaxTextPreviewLength = 1024 * 1024;
            if (text.Length > MaxTextPreviewLength + 3)
            {
                text = text.Substring(0, MaxTextPreviewLength) + "...";
            }

            GUIStyle style = "ScriptText";
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(text), style);
            rect.x = 0f;
            rect.y -= 3f;
            rect.width = EditorGUIUtility.currentViewWidth + 1f;
            GUI.Box(rect, text, style);
        }

        //[MenuItem("Tools/Print MyManifest")]
        static private void PrintMyManifest()
        {
            JsonAsset obj = AssetDatabase.LoadAssetAtPath<JsonAsset>("Assets/Application/main/JsonAsset/MyManifest.zjson");
            MyManifest manifest = obj.Require<MyManifest>();
            if (manifest != null)
            {
                Debug.Log($"{manifest.m_Var1}");
                foreach (var item in manifest.VersionHistory)
                {
                    Debug.Log($"===== {item.Key}    {item.Value}");
                }
            }

            Debug.Log(AssetDatabase.GUIDToAssetPath("0dc49a423d8f9e3439c308b8efe49080"));
        }
    }

    class AutoSetLabelForJsonScript : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (UnityEngine.Application.isBatchMode)
                return;

            foreach (string str in importedAssets)
            {
                if (str.ToLower().EndsWith(".zjson"))
                {
                    var json_manifest_obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(str);

                    AssetDatabase.SetLabels(json_manifest_obj, new string[] { "zjson" });
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem.Profiler
{
    public class BatchProfilingWindow : EditorWindow
    {        
        // [MenuItem("Assets Management/Create BatchAssetCollection Setting", false, 1)]
        static private void CreateSetting()
        {
            BatchAssetCollection asset = GetDefault();
            if (asset != null)
                Selection.activeObject = asset;
            AssetDatabase.SaveAssets();
        }

        static public BatchAssetCollection GetDefault()
        {
            return GetOrCreateEditorConfigObject<BatchAssetCollection>("Assets/Application/MeshParticleSystem/ParticleProfiler");
        }

        static private T GetOrCreateEditorConfigObject<T>(string directoryPath) where T : UnityEngine.ScriptableObject
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
    }
}
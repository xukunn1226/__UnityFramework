using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Core;
using Framework.Core.Editor;

namespace Framework.AssetManagement.AssetEditorWindow
{
    internal class GameBuilderSettingCollection : ScriptableObject
    {
        static public string s_DefaultSettingPath = "Assets/Framework/AssetManagement/GameBuilder/Data";

        [SerializeField] private List<GameBuilderSetting>   m_settings          = new List<GameBuilderSetting>();

        public List<GameBuilderSetting> setting { get { return m_settings; } }

        /// <summary>
        /// 创建GameBuilderSetting.asset及相关的BundleBuilderSetting.asset、PlayerBuilderSetting.asset
        /// </summary>
        /// <param name="directoryPath"></param>
        public void Add(string directoryPath)
        {
            // create BundleBuilderSetting asset
            BundleBuilderSetting bundleSetting = CreateSetting<BundleBuilderSetting>(directoryPath, typeof(BundleBuilderSetting).Name);

            // create PlayerBuilderSetting asset
            PlayerBuilderSetting playerSetting = CreateSetting<PlayerBuilderSetting>(directoryPath, typeof(PlayerBuilderSetting).Name);

            // create GameBuilderSetting asset
            GameBuilderSetting gameSetting = CreateSetting<GameBuilderSetting>(directoryPath, typeof(GameBuilderSetting).Name);

            // error handling
            if(bundleSetting == null || playerSetting == null || gameSetting == null)
            {
                Debug.LogError("Failed to create GameBuildSetting asset.");
                if (bundleSetting != null)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(bundleSetting));
                }
                if (playerSetting != null)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(playerSetting));
                }
                if (gameSetting != null)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(gameSetting));
                }
                return;
            }

            gameSetting.displayName = gameSetting.name.ToLower();
            gameSetting.bundleSetting = bundleSetting;
            gameSetting.playerSetting = playerSetting;
            m_settings.Add(gameSetting);

            UnityEditor.EditorUtility.SetDirty(gameSetting);
            UnityEditor.EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void Remove(string displayName)
        {
            GameBuilderSetting gameSetting = GetData(displayName);
            if (!gameSetting)
                return;

            if (gameSetting.bundleSetting != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(gameSetting.bundleSetting));
            }
            if (gameSetting.playerSetting != null)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(gameSetting.playerSetting));
            }
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(gameSetting));
            m_settings.Remove(gameSetting);

            UnityEditor.EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public GameBuilderSetting GetData(string displayName)
        {
            return m_settings.Find(item => item.displayName.ToLower() == displayName.ToLower());
        }

        public bool Rename(string oldName, string newName)
        {
            if (GetData(newName) != null)
                return false;

            GameBuilderSetting pendingRenameSetting = GetData(oldName);
            if (!pendingRenameSetting)
                return false;

            pendingRenameSetting.displayName = newName;

            UnityEditor.EditorUtility.SetDirty(pendingRenameSetting);
            AssetDatabase.SaveAssets();
            return true;
        }

        private T CreateSetting<T>(string directoryPath, string assetName) where T : ScriptableObject
        {
            string assetPath = FindValidAssetName(directoryPath, assetName);
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        static private string FindValidAssetName(string directoryPath, string assetName)
        {
            if (!AssetDatabase.IsValidFolder(directoryPath))
                throw new System.Exception(directoryPath + " is not a valid folder");

            const int maxIndex = 20;
            int index = 0;
            string assetPath = string.Format("{0}/{1}_{2}.asset", directoryPath.TrimEnd(new char[] { '/' }), assetName, System.Guid.NewGuid().ToString());
            while (index < maxIndex)
            {
                assetPath = string.Format("{0}/{1}_{2}.asset", directoryPath.TrimEnd(new char[] { '/' }), index, assetName);
                if(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath) == null)
                {
                    return assetPath;
                }

                ++index;
            }
            return assetPath;
        }

        static public GameBuilderSettingCollection GetDefault()
        {
            return Core.Editor.EditorUtility.GetOrCreateEditorConfigObject<GameBuilderSettingCollection>(s_DefaultSettingPath);
        }
    }
}
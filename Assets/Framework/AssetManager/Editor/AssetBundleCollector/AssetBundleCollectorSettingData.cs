using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.AssetManagement.AssetBundleCollector
{
    public class AssetBundleCollectorSettingData : MonoBehaviour
    {
        static private AssetBundleCollectorSetting m_Setting;
        static public AssetBundleCollectorSetting Instance
        {
			get
            {
				if(m_Setting == null)
                {
					LoadSettingData();
                }
				return m_Setting;
            }
        }

		static private void LoadSettingData()
        {
			m_Setting = LoadSettingData<AssetBundleCollectorSetting>();
        }

		static public TSetting LoadSettingData<TSetting>() where TSetting : ScriptableObject
		{
			var settingType = typeof(TSetting);
			var guids = AssetDatabase.FindAssets($"t:{settingType.Name}");
			if (guids.Length == 0)
			{
				Debug.LogWarning($"Create new {settingType.Name}.asset");
				var setting = ScriptableObject.CreateInstance<TSetting>();
				string filePath = $"Assets/{settingType.Name}.asset";
				AssetDatabase.CreateAsset(setting, filePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				return setting;
			}
			else
			{
				if (guids.Length != 1)
				{
					foreach (var guid in guids)
					{
						string path = AssetDatabase.GUIDToAssetPath(guid);
						Debug.LogWarning($"Found multiple file : {path}");
					}
					throw new System.Exception($"Found multiple {settingType.Name} files !");
				}

				string filePath = AssetDatabase.GUIDToAssetPath(guids[0]);
				var setting = AssetDatabase.LoadAssetAtPath<TSetting>(filePath);
				return setting;
			}
		}
	}
}
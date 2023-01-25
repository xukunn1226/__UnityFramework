using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class AssetBundleCollectorSettingData : MonoBehaviour
    {
        private static readonly Dictionary<string, System.Type> _cachePackRuleTypes = new Dictionary<string, System.Type>();
        private static readonly Dictionary<string, IPackRule> _cachePackRuleInstance = new Dictionary<string, IPackRule>();

        private static readonly Dictionary<string, System.Type> _cacheFilterRuleTypes = new Dictionary<string, System.Type>();
        private static readonly Dictionary<string, IFilterRule> _cacheFilterRuleInstance = new Dictionary<string, IFilterRule>();

        public static bool isDirty { get; private set; } = false;

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

            // IPackRule
            {
                // 清空缓存集合
                _cachePackRuleTypes.Clear();
                _cachePackRuleInstance.Clear();

                // 获取所有类型
                List<Type> types = new List<Type>(100)
                {
                    typeof(PackFile),
                    typeof(PackDirectory),
                    typeof(PackTopDirectory),
                    typeof(PackCollector),
                    typeof(PackGroup),
                    typeof(PackRawFile),
                };

                var customTypes = EditorTools.GetAssignableTypes(typeof(IPackRule));
                types.AddRange(customTypes);
                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];
                    if (_cachePackRuleTypes.ContainsKey(type.Name) == false)
                        _cachePackRuleTypes.Add(type.Name, type);
                }
            }

            // IFilterRule
            {
                // 清空缓存集合
                _cacheFilterRuleTypes.Clear();
                _cacheFilterRuleInstance.Clear();

                // 获取所有类型
                List<Type> types = new List<Type>(100)
                {
                    typeof(CollectAll),
                    typeof(CollectScene),
                    typeof(CollectPrefab),
                    typeof(CollectSprite)
                };

                var customTypes = EditorTools.GetAssignableTypes(typeof(IFilterRule));
                types.AddRange(customTypes);
                for (int i = 0; i < types.Count; i++)
                {
                    Type type = types[i];
                    if (_cacheFilterRuleTypes.ContainsKey(type.Name) == false)
                        _cacheFilterRuleTypes.Add(type.Name, type);
                }
            }
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

        /// <summary>
		/// 存储配置文件
		/// </summary>
		public static void SaveFile()
        {
            if (m_Setting != null)
            {
                isDirty = false;
                EditorUtility.SetDirty(m_Setting);
                AssetDatabase.SaveAssets();
                Debug.Log($"{nameof(AssetBundleCollectorSetting)}.asset is saved!");
            }
        }

        public static List<RuleDisplayName> GetPackRuleNames()
        {
            if (m_Setting == null)
                LoadSettingData();

            List<RuleDisplayName> names = new List<RuleDisplayName>();
            foreach (var pair in _cachePackRuleTypes)
            {
                RuleDisplayName ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }
            return names;
        }

        public static List<RuleDisplayName> GetFilterRuleNames()
        {
            if (m_Setting == null)
                LoadSettingData();

            List<RuleDisplayName> names = new List<RuleDisplayName>();
            foreach (var pair in _cacheFilterRuleTypes)
            {
                RuleDisplayName ruleName = new RuleDisplayName();
                ruleName.ClassName = pair.Key;
                ruleName.DisplayName = GetRuleDisplayName(pair.Key, pair.Value);
                names.Add(ruleName);
            }
            return names;
        }

        private static string GetRuleDisplayName(string name, Type type)
        {
            var attribute = EditorAttribute.GetAttribute<DisplayNameAttribute>(type);
            if (attribute != null && string.IsNullOrEmpty(attribute.DisplayName) == false)
                return attribute.DisplayName;
            else
                return name;
        }

        public static bool HasPackRuleName(string ruleName)
        {
            return _cachePackRuleTypes.Keys.Contains(ruleName);
        }
        public static bool HasFilterRuleName(string ruleName)
        {
            return _cacheFilterRuleTypes.Keys.Contains(ruleName);
        }

        public static IPackRule GetPackRuleInstance(string ruleName)
        {
            if (_cachePackRuleInstance.TryGetValue(ruleName, out IPackRule instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cachePackRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IPackRule)Activator.CreateInstance(type);
                _cachePackRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new Exception($"{nameof(IPackRule)}类型无效：{ruleName}");
            }
        }

        public static IFilterRule GetFilterRuleInstance(string ruleName)
        {
            if (_cacheFilterRuleInstance.TryGetValue(ruleName, out IFilterRule instance))
                return instance;

            // 如果不存在创建类的实例
            if (_cacheFilterRuleTypes.TryGetValue(ruleName, out Type type))
            {
                instance = (IFilterRule)Activator.CreateInstance(type);
                _cacheFilterRuleInstance.Add(ruleName, instance);
                return instance;
            }
            else
            {
                throw new Exception($"{nameof(IFilterRule)}类型无效：{ruleName}");
            }
        }

        static public AssetBundleCollectorConfig CreateConfig(string configName, string configDesc)
        {
            if (string.IsNullOrEmpty(configName))
                throw new Exception($"CreateConfig: configName is null");

            AssetBundleCollectorConfig config = new AssetBundleCollectorConfig();
            config.ConfigName = configName;
            config.ConfigDesc = configDesc;
            Instance.Configs.Add(config);
            isDirty = true;
            return config;
        }

        static public void RemoveConfig(AssetBundleCollectorConfig config)
        {
            if(Instance.Configs.Remove(config))
            {
                isDirty = true;
            }
            else
            {
                Debug.LogWarning($"Failed to remove AssetBundleCollectorConfig: {config.ConfigName}");
            }
        }
    }
}
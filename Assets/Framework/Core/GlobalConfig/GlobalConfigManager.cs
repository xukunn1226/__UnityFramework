using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Framework.AssetManagement.Runtime;

namespace Framework.Core
{
    static public class GlobalConfigManager
    {
        static private string                           s_ConfigPath        = "assets/res/globalconfig";
        static private Dictionary<string, GlobalConfig> s_DefaultConfigs    = new Dictionary<string, GlobalConfig>();
        static private Dictionary<string, GlobalConfig> s_Configs           = new Dictionary<string, GlobalConfig>();

        static private AssetBundleLoader                m_BundleLoader;
        static private byte[]                           m_Buffer            = new byte[128 * 1024];

        static public void Init(bool InEditorMode = true)
        {
            Uninit();

            // 加载默认配置
            LoadDefaultConfigs(InEditorMode);

            // 根据默认配置创建可修改配置
            BuildSavedConfigs();
        }

        static private void LoadDefaultConfigs(bool InEditorMode = true)
        {
#if UNITY_EDITOR
#else
            InEditorMode = false;
#endif
            if (InEditorMode)
            { // from assetdatabase
#if UNITY_EDITOR
                DirectoryInfo di = new DirectoryInfo(s_ConfigPath);
                FileInfo[] fis = di.GetFiles("*.bytes", SearchOption.AllDirectories);
                foreach (var fi in fis)
                {
                    FileStream fs = new FileStream(fi.FullName, FileMode.Open);
                    int size = fs.Read(m_Buffer, 0, m_Buffer.Length);
                    fs.Close();
                    string json = System.Text.Encoding.UTF8.GetString(m_Buffer, 0, size);
                    GlobalConfig config = JsonConvert.DeserializeObject<GlobalConfig>(json);

                    s_DefaultConfigs.Add(Path.GetFileNameWithoutExtension(fi.Name), config);
                }
#endif
            }
            else
            { // from asset bundle
                m_BundleLoader = AssetManager.LoadAssetBundle(s_ConfigPath + ".ab");
                TextAsset[] tas = m_BundleLoader.assetBundle.LoadAllAssets<TextAsset>();
            }
        }

        static private void BuildSavedConfigs()
        {
            string directory = string.Format($"{Application.persistentDataPath}/Saved");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            foreach (var defaultConfig in s_DefaultConfigs)
            {
                GlobalConfig savedConfig = new GlobalConfig();

                string filePath = string.Format($"{directory}/{defaultConfig.Key}.json");
                bool isExist = File.Exists(filePath);
                {
                    FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                    if (isExist)
                    {
                        string json = System.Text.Encoding.UTF8.GetString(m_Buffer, 0, fs.Read(m_Buffer, 0, m_Buffer.Length));
                        savedConfig = JsonConvert.DeserializeObject<GlobalConfig>(json);
                    }

                    savedConfig.Repair(defaultConfig.Value);
                    s_Configs.Add(defaultConfig.Key, savedConfig);

                    // write to file
                    string data = JsonConvert.SerializeObject(savedConfig, Formatting.Indented);
                    int size = System.Text.Encoding.UTF8.GetBytes(data, 0, data.Length, m_Buffer, 0);
                    fs.Position = 0;
                    fs.Write(m_Buffer, 0, size);
                    fs.Dispose();
                    fs.Close();
                }
            }
        }

        static private void Uninit()
        {
            if(m_BundleLoader != null)
            {
                AssetManager.UnloadAssetBundle(m_BundleLoader);
                m_BundleLoader = null;
            }
            s_DefaultConfigs.Clear();
            s_Configs.Clear();
        }

        static public void Flush(string filename)
        {
            GlobalConfig config;
            if(!s_Configs.TryGetValue(filename, out config))
            {
                Debug.LogError($"can't find file in global configs: {filename}");
                return;
            }

            if (!config.isDirty)
                return;

            string filePath = string.Format($"{Application.persistentDataPath}/Saved/{filename}.json");
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write))
            {
                string data = JsonConvert.SerializeObject(config, Formatting.Indented);
                int size = System.Text.Encoding.UTF8.GetBytes(data, 0, data.Length, m_Buffer, 0);
                fs.Write(m_Buffer, 0, size);
                fs.Close();
            }
            config.isDirty = false;
        }

        static public void FlushAll()
        {
            foreach(var config in s_Configs)
            {
                Flush(config.Key);
            }
        }

        static private bool IsExist(string filename)
        {
            return s_Configs.ContainsKey(filename);
        }

        static public int GetInt(string filename, string sectionName, string propertyName)
        {
            GlobalConfig config;
            if(!s_Configs.TryGetValue(filename, out config))
            {
                Debug.LogWarning($"can't find config file: {filename}");
                return 0;
            }

            string value;
            if(!config.GetValue(sectionName, propertyName, out value))
            {
                Debug.LogWarning($"can't find [{sectionName}].[{propertyName}] from file: {filename}");
                return 0;
            }

            int v = 0;
            try
            {
                v = int.Parse(value);
            }
            catch(Exception e)
            {
                Debug.LogError($"GlobalConfigManager.GetInt throw exception: {e.Message}  filename: {filename}    sectionName: {sectionName}  propertyName: {propertyName}");
            }
            return v;
        }

        static public float GetFloat(string filename, string sectionName, string propertyName)
        {
            GlobalConfig config;
            if (!s_Configs.TryGetValue(filename, out config))
            {
                Debug.LogWarning($"can't find config file: {filename}");
                return 0;
            }

            string value;
            if (!config.GetValue(sectionName, propertyName, out value))
            {
                Debug.LogWarning($"can't find [{sectionName}].[{propertyName}] from file: {filename}");
                return 0;
            }

            float v = 0;
            try
            {
                v = float.Parse(value);
            }
            catch (Exception e)
            {
                Debug.LogError($"GlobalConfigManager.GetFloat throw exception: {e.Message}  filename: {filename}    sectionName: {sectionName}  propertyName: {propertyName}");
            }
            return v;
        }

        static public string GetString(string filename, string sectionName, string propertyName)
        {
            GlobalConfig config;
            if (!s_Configs.TryGetValue(filename, out config))
            {
                Debug.LogWarning($"can't find config file: {filename}");
                return null;
            }

            string value;
            if (!config.GetValue(sectionName, propertyName, out value))
            {
                Debug.LogWarning($"can't find [{sectionName}].[{propertyName}] from file: {filename}");
                return null;
            }
            return value;
        }

        static public void SetInt(string filename, string sectionName, string propertyName, int value)
        {
            GlobalConfig config;
            if (!s_Configs.TryGetValue(filename, out config))
            {
                Debug.LogWarning($"can't find config file: {filename}");
                return;
            }

            config.SetValue(sectionName, propertyName, value.ToString());
        }

        static public void SetFloat(string filename, string sectionName, string propertyName, float value)
        {
            GlobalConfig config;
            if (!s_Configs.TryGetValue(filename, out config))
            {
                Debug.LogWarning($"can't find config file: {filename}");
                return;
            }

            config.SetValue(sectionName, propertyName, value.ToString());
        }

        static public void SetString(string filename, string sectionName, string propertyName, string value)
        {
            GlobalConfig config;
            if (!s_Configs.TryGetValue(filename, out config))
            {
                Debug.LogWarning($"can't find config file: {filename}");
                return;
            }

            config.SetValue(sectionName, propertyName, value);
        }

        static public void LoadFromConfig(this IConfig instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            string ns = instance.GetType().Namespace;
            string cn = instance.GetType().Name;

            Type type = instance.GetType();
            FileConfigAttribute fileConfigAttr = (FileConfigAttribute)type.GetCustomAttribute(typeof(FileConfigAttribute), true);
            if (fileConfigAttr == null)
            {
                Debug.LogWarning($"can't find FileConfigAttribute in {type.Name}");
                return;
            }

            if(!IsExist(fileConfigAttr.Filename))
            {
                Debug.LogWarning($"{fileConfigAttr.Filename} not exist");
                return;
            }

            foreach (var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                Attribute propAttr = prop.GetCustomAttribute(typeof(PropertyConfigAttribute));
                if (propAttr == null) continue;

                if(Attribute.IsDefined(prop, typeof(IntPropertyConfigAttribute)))
                {
                    prop.SetValue(instance, GetInt(fileConfigAttr.Filename, ns, string.Format($"{cn}.{prop.Name}")));
                }
                else if(Attribute.IsDefined(prop, typeof(FloatPropertyConfigAttribute)))
                {
                    prop.SetValue(instance, GetFloat(fileConfigAttr.Filename, ns, string.Format($"{cn}.{prop.Name}")));
                }
                else if(Attribute.IsDefined(prop, typeof(StringPropertyConfigAttribute)))
                {
                    prop.SetValue(instance, GetString(fileConfigAttr.Filename, ns, string.Format($"{cn}.{prop.Name}")));
                }
                else
                {
                    Debug.LogWarning($"");
                }

                // Debug.Log($"{prop.Name}     {prop.PropertyType}     {prop.GetValue(instance)}");
            }

            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                Attribute propAttr = field.GetCustomAttribute(typeof(PropertyConfigAttribute));
                if (propAttr == null) continue;

                if(Attribute.IsDefined(field, typeof(IntPropertyConfigAttribute)))
                {
                    field.SetValue(instance, GetInt(fileConfigAttr.Filename, ns, string.Format($"{cn}.{field.Name}")));
                }
                else if(Attribute.IsDefined(field, typeof(FloatPropertyConfigAttribute)))
                {
                    field.SetValue(instance, GetFloat(fileConfigAttr.Filename, ns, string.Format($"{cn}.{field.Name}")));
                }
                else if(Attribute.IsDefined(field, typeof(StringPropertyConfigAttribute)))
                {
                    field.SetValue(instance, GetString(fileConfigAttr.Filename, ns, string.Format($"{cn}.{field.Name}")));
                }
                else
                {
                    Debug.LogWarning($"");
                }

                // Debug.Log($"{field.Name}     {field.FieldType}     {field.GetValue(instance)}");
            }
        }
    }

    public interface IConfig
    {
        void Load();
    }







    [FileConfig("Engine")]
    public class Actor : IConfig
    {
        [FloatPropertyConfig] static private float m_Float;
        [IntPropertyConfig] public int m_Int;
        [StringPropertyConfig] public string m_Str;
        public object Obj { get; }

        public void PrintAttribute()
        {
            FileConfigAttribute[] attr = (FileConfigAttribute[])GetType().GetCustomAttributes(typeof(FileConfigAttribute), false);
        }

        public void Load()
        {
            this.LoadFromConfig();

            FileConfigAttribute fileConfigAttr = (FileConfigAttribute)GetType().GetCustomAttribute(typeof(FileConfigAttribute), true);
            string ns = GetType().Namespace;
            GlobalConfigManager.SetFloat(fileConfigAttr.Filename, ns, "Actor.m_Float", 2.1f);
            GlobalConfigManager.Flush(fileConfigAttr.Filename);
        }
    }



    public class TestAttributes
    {
        public void Test()
        {
            PrintAuthorInfo(typeof(Actor));
            TestAssignableFrom();
        }

        private static void PrintAuthorInfo(System.Type t)
        {
            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(t);  // reflection

            foreach (System.Attribute attr in attrs)
            {
                if (attr is FileConfigAttribute)
                {
                    FileConfigAttribute a = (FileConfigAttribute)attr;
                    Debug.Log($"   {a.Filename}");
                }
            }
        }

        public static void TestAssignableFrom()
        {
            TestAttributes aa = new TestAttributes();
            Assembly assembly = aa.GetType().Assembly;
            Type[] types = assembly.GetTypes();
            Type type = typeof(Actor);
            for (int i = 0; i < types.Length; i++)
            {
                Debug.Log($"{types[i].Namespace}    {types[i].Name}     {types[i].FullName}");
                if (type.IsAssignableFrom(types[i]) && !types[i].IsInterface)
                {
                    Debug.Log($"{types[i].Name}");
                }
            }
        }
    }
}
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
        static private Dictionary<string, GlobalConfig> s_SavedConfigs      = new Dictionary<string, GlobalConfig>();

        static private AssetBundleLoader                m_BundleLoader;
        static private byte[]                           m_Buffer            = new byte[128 * 1024];

        static public void Init(bool InEditorMode = true)
        {
            Uninit();

            // 加载默认配置
            LoadDefaultConfigs(InEditorMode);

            // 根据默认配置创建本地可修改配置
            BuildSavedConfigs();

            // 赋值全局静态变量
            LoadStaticVariants();
        }

        static private void Uninit()
        {
            if(m_BundleLoader != null)
            {
                AssetManager.UnloadAssetBundle(m_BundleLoader);
                m_BundleLoader = null;
            }
            s_DefaultConfigs.Clear();
            s_SavedConfigs.Clear();
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
                    s_SavedConfigs.Add(defaultConfig.Key, savedConfig);

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

        // 初始化所有的静态变量
        static private void LoadStaticVariants()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assem in assemblies)
            {
                string assemblyName = assem.GetName().Name.ToLower();
                if(assemblyName.StartsWith("com.unity") ||
                   assemblyName.StartsWith("unityengine") ||
                   assemblyName.StartsWith("system") ||
                   assemblyName.StartsWith("unityeditor") ||
                   assemblyName.StartsWith("unity") ||
                   assemblyName.StartsWith("cinemachine") ||
                   assemblyName.Contains("editor") ||
                //    assemblyName.Contains("tests") ||
                   assemblyName.StartsWith("google.") ||
                   assemblyName.StartsWith("newtonsoft") ||
                   assemblyName.StartsWith("excss.unity") ||
                   assemblyName.StartsWith("nunit.framework") ||
                   assemblyName.StartsWith("icsharpcode") ||
                   assemblyName.StartsWith("mscorlib"))
                    continue;

                // Debug.Log($"{assem.FullName}-------------{assem.GetName().Name}");                
                foreach(var t in assem.GetTypes())
                {
                    // Debug.LogWarning($"---: {t.FullName}");

                    FileConfigAttribute attr = (FileConfigAttribute)t.GetCustomAttribute(typeof(FileConfigAttribute), true);
                    if(attr == null)
                        continue;

                    BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
                    string sectionName = string.Format($"{t.Namespace}.{t.Name}");

                    FieldInfo[] fis = t.GetFields(flags);
                    foreach(var fi in fis)
                    {
                        Attribute propAttr = fi.GetCustomAttribute(typeof(PropertyConfigAttribute));
                        if (propAttr == null) continue;

                        if (Attribute.IsDefined(fi, typeof(IntPropertyConfigAttribute)))
                        {
                            fi.SetValue(null, GetInt(attr.Filename, sectionName, fi.Name));
                        }
                        else if (Attribute.IsDefined(fi, typeof(FloatPropertyConfigAttribute)))
                        {
                            fi.SetValue(null, GetFloat(attr.Filename, sectionName, fi.Name));
                        }
                        else if (Attribute.IsDefined(fi, typeof(StringPropertyConfigAttribute)))
                        {
                            fi.SetValue(null, GetString(attr.Filename, sectionName, fi.Name));
                        }
                        else
                        {
                            Debug.LogWarning($"unknown PropertyConfigAttribute: {propAttr}");
                        }
                    }

                    PropertyInfo[] pis = t.GetProperties(flags);
                    foreach(var pi in pis)
                    {
                        Attribute propAttr = pi.GetCustomAttribute(typeof(PropertyConfigAttribute));
                        if (propAttr == null) continue;

                        if (Attribute.IsDefined(pi, typeof(IntPropertyConfigAttribute)))
                        {
                            pi.SetValue(null, GetInt(attr.Filename, sectionName, pi.Name));
                        }
                        else if (Attribute.IsDefined(pi, typeof(FloatPropertyConfigAttribute)))
                        {
                            pi.SetValue(null, GetFloat(attr.Filename, sectionName, pi.Name));
                        }
                        else if (Attribute.IsDefined(pi, typeof(StringPropertyConfigAttribute)))
                        {
                            pi.SetValue(null, GetString(attr.Filename, sectionName, pi.Name));
                        }
                        else
                        {
                            Debug.LogWarning($"unknown PropertyConfigAttribute: {propAttr}");
                        }
                    }
                }
            }
        }
        
        static public void Flush(string filename)
        {
            GlobalConfig config;
            if(!s_SavedConfigs.TryGetValue(filename, out config))
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
            foreach(var config in s_SavedConfigs)
            {
                Flush(config.Key);
            }
        }

        static private bool IsExist(string filename)
        {
            return s_SavedConfigs.ContainsKey(filename);
        }

        static public int GetInt(string configName, string sectionName, string propertyName)
        {
            GlobalConfig config;
            if(!s_SavedConfigs.TryGetValue(configName, out config))
            {
                Debug.LogWarning($"can't find config file: {configName}");
                return 0;
            }

            string value;
            if(!config.GetValue(sectionName, propertyName, out value))
            {
                Debug.LogWarning($"can't find [{sectionName}].[{propertyName}] from file: {configName}");
                return 0;
            }

            int v = 0;
            try
            {
                v = int.Parse(value);
            }
            catch(Exception e)
            {
                Debug.LogError($"GlobalConfigManager.GetInt throw exception: {e.Message}  filename: {configName}    sectionName: {sectionName}  propertyName: {propertyName}");
            }
            return v;
        }

        static public int GetInt(this IConfig inst, string propertyName)
        {
            return GetInt(inst.ConfigName, inst.SectionName, propertyName);
        }

        static public float GetFloat(string configName, string sectionName, string propertyName)
        {
            GlobalConfig config;
            if(!s_SavedConfigs.TryGetValue(configName, out config))
            {
                Debug.LogWarning($"can't find config file: {configName}");
                return 0;
            }

            string value;
            if(!config.GetValue(sectionName, propertyName, out value))
            {
                Debug.LogWarning($"can't find [{sectionName}].[{propertyName}] from file: {configName}");
                return 0;
            }

            float v = 0;
            try
            {
                v = float.Parse(value);
            }
            catch(Exception e)
            {
                Debug.LogError($"GlobalConfigManager.GetFloat throw exception: {e.Message}  filename: {configName}    sectionName: {sectionName}  propertyName: {propertyName}");
            }
            return v;
        }

        static public float GetFloat(this IConfig inst, string propertyName)
        {
            return GetFloat(inst.ConfigName, inst.SectionName, propertyName);
        }

        static public string GetString(string configName, string sectionName, string propertyName)
        {
            GlobalConfig config;
            if (!s_SavedConfigs.TryGetValue(configName, out config))
            {
                Debug.LogWarning($"can't find config file: {configName}");
                return null;
            }

            string value;
            if (!config.GetValue(sectionName, propertyName, out value))
            {
                Debug.LogWarning($"can't find [{sectionName}].[{propertyName}] from file: {configName}");
                return null;
            }
            return value;
        }

        static public string GetString(this IConfig inst, string propertyName)
        {
            return GetString(inst.ConfigName, inst.SectionName, propertyName);
        }

        static public void SetInt(this IConfig inst, string propName, int value, bool flush = false)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            Type t = inst.GetType();
            FieldInfo fi = t.GetField(propName, flag);
            if(fi != null)
            {
                SetInt(inst, fi, value, flush);
            }
            else
            {
                PropertyInfo pi = t.GetProperty(propName, flag);
                if(pi != null)
                {
                    SetInt(inst, pi, value, flush);
                }
                else
                {
                    Debug.LogWarning($"can't find propName({propName}) in Fields or Properties");
                }
            }
        }

        static public void SetInt(this IConfig inst, FieldInfo fieldInfo, int value, bool flush = false)
        {
            GlobalConfig config;
            if (!s_SavedConfigs.TryGetValue(inst.ConfigName, out config))
            {
                Debug.LogWarning($"can't find config file: {inst.ConfigName}");
                return;
            }

            fieldInfo.SetValue(inst, value);
            config.SetValue(inst.SectionName, fieldInfo.Name, value.ToString());

            if(flush)
            {
                Flush(inst.ConfigName);
            }
        }

        static public void SetInt(this IConfig inst, PropertyInfo propertyInfo, int value, bool flush = false)
        {
            GlobalConfig config;
            if (!s_SavedConfigs.TryGetValue(inst.ConfigName, out config))
            {
                Debug.LogWarning($"can't find config file: {inst.ConfigName}");
                return;
            }

            propertyInfo.SetValue(inst, value);
            config.SetValue(inst.SectionName, propertyInfo.Name, value.ToString());

            if(flush)
            {
                Flush(inst.ConfigName);
            }
        }        

        static public void SetFloat(this IConfig inst, string propName, float value, bool flush = false)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            Type t = inst.GetType();
            FieldInfo fi = t.GetField(propName, flag);
            if(fi != null)
            {
                SetFloat(inst, fi, value, flush);
            }
            else
            {
                PropertyInfo pi = t.GetProperty(propName, flag);
                if(pi != null)
                {
                    SetFloat(inst, pi, value, flush);
                }
                else
                {
                    Debug.LogWarning($"can't find propName({propName}) in Fields or Properties");
                }
            }
        }

        static public void SetFloat(this IConfig inst, FieldInfo fieldInfo, float value, bool flush = false)
        {
            GlobalConfig config;
            if (!s_SavedConfigs.TryGetValue(inst.ConfigName, out config))
            {
                Debug.LogWarning($"can't find config file: {inst.ConfigName}");
                return;
            }

            fieldInfo.SetValue(inst, value);
            config.SetValue(inst.SectionName, fieldInfo.Name, value.ToString());

            if(flush)
            {
                Flush(inst.ConfigName);
            }
        }

        static public void SetFloat(this IConfig inst, PropertyInfo propertyInfo, float value, bool flush = false)
        {
            GlobalConfig config;
            if (!s_SavedConfigs.TryGetValue(inst.ConfigName, out config))
            {
                Debug.LogWarning($"can't find config file: {inst.ConfigName}");
                return;
            }

            propertyInfo.SetValue(inst, value);
            config.SetValue(inst.SectionName, propertyInfo.Name, value.ToString());

            if(flush)
            {
                Flush(inst.ConfigName);
            }
        }

        static public void SetString(this IConfig inst, string propName, string value, bool flush = false)
        {
            BindingFlags flag = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            Type t = inst.GetType();
            FieldInfo fi = t.GetField(propName, flag);
            if(fi != null)
            {
                SetString(inst, fi, value, flush);
            }
            else
            {
                PropertyInfo pi = t.GetProperty(propName, flag);
                if(pi != null)
                {
                    SetString(inst, pi, value, flush);
                }
                else
                {
                    Debug.LogWarning($"can't find propName({propName}) in Fields or Properties");
                }
            }
        }

        static public void SetString(this IConfig inst, FieldInfo fieldInfo, string value, bool flush = false)
        {
            GlobalConfig config;
            if (!s_SavedConfigs.TryGetValue(inst.ConfigName, out config))
            {
                Debug.LogWarning($"can't find config file: {inst.ConfigName}");
                return;
            }

            fieldInfo.SetValue(inst, value);
            config.SetValue(inst.SectionName, fieldInfo.Name, value);

            if(flush)
            {
                Flush(inst.ConfigName);
            }
        }

        static public void SetString(this IConfig inst, PropertyInfo propertyInfo, string value, bool flush = false)
        {
            GlobalConfig config;
            if (!s_SavedConfigs.TryGetValue(inst.ConfigName, out config))
            {
                Debug.LogWarning($"can't find config file: {inst.ConfigName}");
                return;
            }

            propertyInfo.SetValue(inst, value);
            config.SetValue(inst.SectionName, propertyInfo.Name, value);

            if(flush)
            {
                Flush(inst.ConfigName);
            }
        }

        // 加载非静态变量
        static public void LoadFromConfig(this IConfig instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            if(!IsExist(instance.ConfigName))
            {
                Debug.LogWarning($"{instance.ConfigName} not exist");
                return;
            }

            Type type = instance.GetType();
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var prop in type.GetProperties(flags))
            {
                Attribute propAttr = prop.GetCustomAttribute(typeof(PropertyConfigAttribute));
                if (propAttr == null) continue;

                if(Attribute.IsDefined(prop, typeof(IntPropertyConfigAttribute)))
                {
                    prop.SetValue(instance, GetInt(instance, prop.Name));
                }
                else if(Attribute.IsDefined(prop, typeof(FloatPropertyConfigAttribute)))
                {
                    prop.SetValue(instance, GetFloat(instance, prop.Name));
                }
                else if(Attribute.IsDefined(prop, typeof(StringPropertyConfigAttribute)))
                {
                    prop.SetValue(instance, GetString(instance, prop.Name));
                }
                else
                {
                    Debug.LogWarning($"unknown PropertyConfigAttribute: {propAttr}");
                }
            }

            foreach (var field in type.GetFields(flags))
            {
                Attribute propAttr = field.GetCustomAttribute(typeof(PropertyConfigAttribute));
                if (propAttr == null) continue;

                if(Attribute.IsDefined(field, typeof(IntPropertyConfigAttribute)))
                {
                    field.SetValue(instance, GetInt(instance, field.Name));
                }
                else if(Attribute.IsDefined(field, typeof(FloatPropertyConfigAttribute)))
                {
                    field.SetValue(instance, GetFloat(instance, field.Name));
                }
                else if(Attribute.IsDefined(field, typeof(StringPropertyConfigAttribute)))
                {
                    field.SetValue(instance, GetString(instance, field.Name));
                }
                else
                {
                    Debug.LogWarning($"unknown PropertyConfigAttribute: {propAttr}");
                }
            }
        }
    }

    public interface IConfig
    {
        string ConfigName { get; }
        string SectionName { get; }
    }
}
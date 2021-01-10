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

        static private AssetBundleLoader m_BundleLoader;

        static public void Init(bool InEditorMode = true)
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
                byte[] array = new byte[1024 * 128];
                foreach (var fi in fis)
                {
                    FileStream fs = new FileStream(fi.FullName, FileMode.Open);
                    int size = fs.Read(array, 0, 1024 * 128);
                    fs.Close();
                    string json = System.Text.Encoding.UTF8.GetString(array, 0, size);
                    GlobalConfig config = JsonConvert.DeserializeObject<GlobalConfig>(json);

                    s_DefaultConfigs.Add(fi.Name, config);
                }
#endif
            }
            else
            { // from asset bundle
                m_BundleLoader = AssetManager.LoadAssetBundle(s_ConfigPath + ".ab");
                TextAsset[] tas = m_BundleLoader.assetBundle.LoadAllAssets<TextAsset>();
            }

            BuildSavedConfigs();
        }

        static private void BuildSavedConfigs()
        {
            foreach(var defaultConfig in s_DefaultConfigs)
            {

            }
        }

        static public void Uninit()
        {
            if(m_BundleLoader != null)
            {
                AssetManager.UnloadAssetBundle(m_BundleLoader);
                m_BundleLoader = null;
            }
            s_DefaultConfigs.Clear();
            s_Configs.Clear();
        }

        static public void LoadConfig(this IConfig instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            string ns = instance.GetType().Namespace;
            string cn = instance.GetType().Name;

            Type type = instance.GetType();
            Attribute fileConfigAttr = type.GetCustomAttribute(typeof(FileConfigAttribute), true);
            if (fileConfigAttr == null)
                return;

            foreach (var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                Attribute propAttr = prop.GetCustomAttribute(typeof(PropertyConfigAttribute));
                if (propAttr == null) continue;

                Debug.Log($"{prop.Name}     {prop.PropertyType}     {prop.GetValue(instance)}");
            }

            Debug.Log("--------------------------");

            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                Attribute propAttr = field.GetCustomAttribute(typeof(PropertyConfigAttribute));
                if (propAttr == null) continue;

                Debug.Log($"{field.Name}     {field.FieldType}     {field.GetValue(instance)}");
            }
        }
        
        static public void TestConfigSerialize()
        {
            GlobalConfig config = new GlobalConfig();

            ConfigSection section = new ConfigSection();
            section.Add("Actor.m_Float", "1.2f");
            section.Add("Actor.m_Str", "1234354");
            config.Add("Framework.Core", section);


            ConfigSection section1 = new ConfigSection();
            section1.Add("Player.m_Float", "1.2f");
            section1.Add("Player.m_Str", "1234354");
            config.Add("Framework.Asset", section1);

            string json = JsonConvert.SerializeObject(config, Formatting.Indented);

            System.IO.FileStream fs = new System.IO.FileStream("assets/framework/core/misc/tests/runtime/config.json", System.IO.FileMode.Create);
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(json);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
        }
    }

    public interface IConfig
    {
        void LoadConfig();
    }
}
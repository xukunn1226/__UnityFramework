using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Newtonsoft.Json;

namespace Framework.Core
{
    static public class GlobalConfigManager
    {
        static private string s_ConfigPath = "assets/res/globalconfig";

        static private Dictionary<string, GlobalConfig> s_ConfigDic = new Dictionary<string, GlobalConfig>();

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace Framework.Core.Tests
{
    [FileConfig("Engine")]
    public class TestActorConfig : IConfig
    {
        [FloatPropertyConfig]   static private float    m_Float;
        [IntPropertyConfig]     public int              m_Int;
        [StringPropertyConfig]  public string           m_Str;

        private string m_ConfigName;
        public string ConfigName
        { 
            get
            {
                if(string.IsNullOrEmpty(m_ConfigName))
                {
                    FileConfigAttribute attr = (FileConfigAttribute)GetType().GetCustomAttribute(typeof(FileConfigAttribute), true);
                    m_ConfigName = attr?.Filename ?? "Global";
                }
                return m_ConfigName;
            }
        }

        private string m_SectionName;
        public string SectionName
        {
            get
            {
                if(string.IsNullOrEmpty(m_SectionName))
                {
                    Type t = GetType();
                    m_SectionName = string.Format($"{t.Namespace}.{t.Name}");
                }
                return m_SectionName;
            }
        }

        public void Load()
        {
            this.LoadFromConfig();

            this.SetFloat("m_Float", 0.1111111f, true);
            Debug.Log(this.GetFloat("m_Float"));
        }
    }
}
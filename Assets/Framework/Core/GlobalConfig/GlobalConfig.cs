using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    public class ConfigSection : Dictionary<string, string>             // key: ClassName.FieldName; value: "value".toString
    {
        public void Repair(ConfigSection other)
        {
            foreach(var key in other.Keys)
            {
                string value;
                if(!TryGetValue(key, out value))
                {
                    Add(key, other[key]);
                }
            }
        }

        public bool GetValue(string property, out string value)
        {
            return TryGetValue(property, out value);
        }

        public bool SetValue(string property, string value)
        {
            bool isDirty = true;
            if(ContainsKey(property))
            {
                isDirty = string.Compare(this[property], value) != 0;
                this[property] = value;
            }
            else
            {
                Add(property, value);
            }
            return isDirty;
        }
    }

    public class GlobalConfig : Dictionary<string, ConfigSection>       // key: namespace
    {
        [System.NonSerialized] public bool isDirty;

        // 修复不存在的数据
        public void Repair(GlobalConfig other)
        {
            foreach(var key in other.Keys)
            {
                ConfigSection section;
                if(!TryGetValue(key, out section))
                {
                    Add(key, other[key]);
                }
                else
                {
                    section.Repair(other[key]);
                }
            }
        }

        public bool GetValue(string ns, string property, out string value)
        {
            ConfigSection section;
            if(!TryGetValue(ns, out section))
            {
                value = null;
                return false;
            }

            return section.GetValue(property, out value);
        }

        public void SetValue(string ns, string property, string value)
        {
            ConfigSection section;
            if(!TryGetValue(ns, out section))
            {
                section = new ConfigSection();
                Add(ns, section);
                isDirty = true;
            }
            isDirty |= section.SetValue(property, value);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite;

namespace Application.Runtime
{
    public partial class ConfigManager : Singleton<ConfigManager>
    {
        private SqlData m_Sql;

        protected override void Init()
        {
            m_Sql = new SqlData("Assets/Application/hotfix/SQLite/Editor/config.db");
        }

        protected override void OnDestroy()
        {
            m_Sql?.Close();
        }
        
        private void Parse(ref List<string> ret, string content)
        {
            string[] list = content.Split(';');
            for(int i = 0; i < list.Length; ++i)
            {
                if(string.IsNullOrEmpty(list[i])) continue;
                
                ret.Add(list[i]);
            }
        }

        private void Parse(ref List<int> ret, string content)
        {
            string[] list = content.Split(';');
            for(int i = 0; i < list.Length; ++i)
            {
                if(string.IsNullOrEmpty(list[i])) continue;
                
                ret.Add(int.Parse(list[i]));
            }
        }

        private void Parse(ref List<float> ret, string content)
        {
            string[] list = content.Split(';');
            for(int i = 0; i < list.Length; ++i)
            {
                if(string.IsNullOrEmpty(list[i])) continue;
                
                ret.Add(float.Parse(list[i]));
            }
        }
    }
}
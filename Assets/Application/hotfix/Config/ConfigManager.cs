using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SQLite;

namespace Application.Runtime
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        private SqlData m_Sql;

        protected override void Init()
        {
            m_Sql = new SqlData("config.db");
        }

        protected override void OnDestroy()
        {
            m_Sql?.Close();
        }

        private Dictionary<int, Player> m_PlayerDict = new Dictionary<int, Player>();

        public Player GetPlayerByID(int id)
        {
            Player desc;
            if(m_PlayerDict.TryGetValue(id, out desc))
            {
                return desc;
            }

            return null;
        }
    }
}
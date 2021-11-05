using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Mono.Data.Sqlite;

namespace Application.Runtime
{
    public partial class ConfigManager : Singleton<ConfigManager>
    {
        private Dictionary<int, Monster> m_MonsterDict = new Dictionary<int, Monster>();
        public Monster GetMonsterByID(int key1)
        {
            const string tableName = "Monster";

            Monster desc;
            if(m_MonsterDict.TryGetValue(key1, out desc))
            {
                return desc;
            }

            desc = new Monster();
            SqliteDataReader reader = m_Sql.ReadTable(tableName, "ID", "=", key1.ToString());
            while(reader.Read())
            {                
                desc.ID = reader.GetInt32(reader.GetOrdinal("ID"));
                desc.Name = reader.GetString(reader.GetOrdinal("Name"));
                desc.HP = reader.GetFloat(reader.GetOrdinal("HP"));
                desc.Male = reader.GetBoolean(reader.GetOrdinal("Male"));
            }

            m_MonsterDict.Add(key1, desc);
            return desc;
        }

        private Dictionary<string, Player> m_PlayerDict = new Dictionary<string, Player>();
        public Player GetPlayerByID(string key1)
        {
            const string tableName = "Player";

            Player desc;
            if(m_PlayerDict.TryGetValue(key1, out desc))
            {
                return desc;
            }

            desc = new Player();
            SqliteDataReader reader = m_Sql.ReadTable(tableName, "Building_ID", "=", key1.ToString());
            while(reader.Read())
            {                
                desc.Building_ID = reader.GetString(reader.GetOrdinal("Building_ID"));
                desc.Name = reader.GetString(reader.GetOrdinal("Name"));
                desc.HP = reader.GetFloat(reader.GetOrdinal("HP"));
                desc.Male = reader.GetBoolean(reader.GetOrdinal("Male"));
                desc.MonsterDesc = GetMonsterByID(reader.GetInt32(reader.GetOrdinal("MonsterDesc")));
                Parse(ref desc.variant1, reader.GetString(reader.GetOrdinal("variant1")));
                Parse(ref desc.variant2, reader.GetString(reader.GetOrdinal("variant2")));
                Parse(ref desc.variant3, reader.GetString(reader.GetOrdinal("variant3")));
                Parse(ref desc.variant4, reader.GetString(reader.GetOrdinal("variant4")));
            }

            m_PlayerDict.Add(key1, desc);
            return desc;
        }

    }
}


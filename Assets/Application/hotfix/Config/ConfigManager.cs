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
  
            Monster desc = null;
            if(FindMonsterData(key1, ref desc))
                return desc;

            SqliteDataReader reader = m_Sql.ReadTable(tableName, "ID", "=", key1.ToString());
            bool bFind = reader.HasRows;
            while(reader.Read())
            {                
                desc.ID = reader.GetInt32(reader.GetOrdinal("ID"));
                desc.Name = reader.GetString(reader.GetOrdinal("Name"));
                desc.HP = reader.GetFloat(reader.GetOrdinal("HP"));
                desc.Male = reader.GetBoolean(reader.GetOrdinal("Male"));
            }

            return bFind ? desc : null;
        }
        private bool FindMonsterData(int key1, ref Monster desc)
        {
            if(!m_MonsterDict.TryGetValue(key1, out desc))
            {
                desc = new Monster();
                m_MonsterDict.Add(key1, desc);
                return false;
            }
            return true;
        }

        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, Player>>>> m_PlayerDict = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, Player>>>>();
        public Player GetPlayerByID(string key1, string key2, string key3, int key4)
        {
            const string tableName = "Player";
  
            Player desc = null;
            if(FindPlayerData(key1, key2, key3, key4, ref desc))
                return desc;

            SqliteDataReader reader = m_Sql.ReadTable(tableName, new string[] {"Building_ID", "Name", "Address", "HP"}, "=", new string[] {key1.ToString(), key2.ToString(), key3.ToString(), key4.ToString()});
            bool bFind = reader.HasRows;
            while(reader.Read())
            {                
                desc.Building_ID = reader.GetString(reader.GetOrdinal("Building_ID"));
                desc.Name = reader.GetString(reader.GetOrdinal("Name"));
                desc.Address = reader.GetString(reader.GetOrdinal("Address"));
                desc.HP = reader.GetInt32(reader.GetOrdinal("HP"));
                desc.Male = reader.GetBoolean(reader.GetOrdinal("Male"));
                desc.MonsterDesc = GetMonsterByID(reader.GetInt32(reader.GetOrdinal("MonsterDesc")));
                Parse(ref desc.variant1, reader.GetString(reader.GetOrdinal("variant1")));
                Parse(ref desc.variant2, reader.GetString(reader.GetOrdinal("variant2")));
                Parse(ref desc.variant3, reader.GetString(reader.GetOrdinal("variant3")));
                Parse(ref desc.variant4, reader.GetString(reader.GetOrdinal("variant4")));
            }

            return bFind ? desc : null;
        }
        private bool FindPlayerData(string key1, string key2, string key3, int key4, ref Player desc)
        {
            Dictionary<string, Dictionary<string, Dictionary<int, Player>>> dict;
            if(!m_PlayerDict.TryGetValue(key1, out dict))
            {
                dict = new Dictionary<string, Dictionary<string, Dictionary<int, Player>>>();
                m_PlayerDict.Add(key1, dict);
            }

            Dictionary<string, Dictionary<int, Player>> dict2;
            if(!dict.TryGetValue(key2, out dict2))
            {
                dict2 = new Dictionary<string, Dictionary<int, Player>>();
                dict.Add(key2, dict2);
            }

            Dictionary<int, Player> dict3;
            if(!dict2.TryGetValue(key3, out dict3))
            {
                dict3 = new Dictionary<int, Player>();
                dict2.Add(key3, dict3);
            }

            if(!dict3.TryGetValue(key4, out desc))
            {
                desc = new Player();
                dict3.Add(key4, desc);
                return false;
            }
            return true;
        }

    }
}


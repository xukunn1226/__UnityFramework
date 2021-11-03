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
        public Monster GetMonsterByID(int id)
        {
            const string tableName = "Monster";
            int _id = id;

            Monster desc;
            if(m_MonsterDict.TryGetValue(_id, out desc))
            {
                return desc;
            }

            desc = new Monster();
            SqliteDataReader reader = m_Sql.ReadTable(tableName, "id", "=", id.ToString());
            while(reader.Read())
            {                
                desc.ID = reader.GetInt32(reader.GetOrdinal("ID"));
                desc.Name = reader.GetString(reader.GetOrdinal("Name"));
                desc.HP = reader.GetFloat(reader.GetOrdinal("HP"));
                desc.Male = reader.GetBoolean(reader.GetOrdinal("Male"));
            }

            m_MonsterDict.Add(_id, desc);
            return desc;
        }

        private Dictionary<int, Player> m_PlayerDict = new Dictionary<int, Player>();
        public Player GetPlayerByID(string id)
        {
            const string tableName = "Player";
            int _id = id.GetHashCode();

            Player desc;
            if(m_PlayerDict.TryGetValue(_id, out desc))
            {
                return desc;
            }

            desc = new Player();
            SqliteDataReader reader = m_Sql.ReadTable(tableName, "id", "=", id.ToString());
            while(reader.Read())
            {                
                desc.ID = reader.GetString(reader.GetOrdinal("ID"));
                desc.Name = reader.GetString(reader.GetOrdinal("Name"));
                desc.HP = reader.GetFloat(reader.GetOrdinal("HP"));
                desc.Male = reader.GetBoolean(reader.GetOrdinal("Male"));
                desc.MonsterDesc = GetMonsterByID(reader.GetInt32(reader.GetOrdinal("MonsterDesc")));
                Parse(ref desc.variant1, reader.GetString(reader.GetOrdinal("variant1")));
                Parse(ref desc.variant2, reader.GetString(reader.GetOrdinal("variant2")));
                Parse(ref desc.variant3, reader.GetString(reader.GetOrdinal("variant3")));
                Parse(ref desc.variant4, reader.GetString(reader.GetOrdinal("variant4")));
            }

            m_PlayerDict.Add(_id, desc);
            return desc;
        }

    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SQLite;
using Mono.Data.Sqlite;

namespace Application.Runtime
{
    public partial class ConfigManager : Singleton<ConfigManager>
    {
        private Dictionary<int, Player> m_PlayerDict = new Dictionary<int, Player>();

        public Player GetPlayerByID(int id)
        {
            const string tableName = "Player";

            Player desc;
            if(m_PlayerDict.TryGetValue(id, out desc))
            {
                return desc;
            }

            desc = new Player();
            SqliteDataReader reader = m_Sql.ReadTable(tableName, "id", "=", id.ToString());
            while(reader.Read())
            {
                desc.ID = reader.GetInt32(reader.GetOrdinal("id"));
                desc.Name = reader.GetString(reader.GetOrdinal("name"));
                desc.HP = reader.GetFloat(reader.GetOrdinal("hp"));
                desc.Male = reader.GetBoolean(reader.GetOrdinal("male"));
                Parse(ref desc.variant1, reader.GetString(reader.GetOrdinal("variant1")));
                Parse(ref desc.variant2, reader.GetString(reader.GetOrdinal("variant2")));
                Parse(ref desc.variant3, reader.GetString(reader.GetOrdinal("variant3")));
            }

            return desc;
        }
    }
}
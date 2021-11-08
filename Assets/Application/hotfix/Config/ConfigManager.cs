using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Mono.Data.Sqlite;

namespace Application.Runtime
{
    public partial class ConfigManager : Singleton<ConfigManager>
    {
        private Dictionary<string, buttonConfig> m_buttonConfigDict = new Dictionary<string, buttonConfig>();
        public buttonConfig GetbuttonConfigByID(string key1)
        {
            const string tableName = "buttonConfig";
  
            buttonConfig desc = null;
            if(FindbuttonConfigData(key1, ref desc))
                return desc;

            SqliteDataReader reader = m_Sql.ReadTable(tableName, "button_id", "=", key1.ToString());
            bool bFind = reader.HasRows;
            while(reader.Read())
            {                
                desc.button_id = reader.GetString(reader.GetOrdinal("button_id"));
                desc.duration_time = reader.GetInt32(reader.GetOrdinal("duration_time"));
                desc.born_effect = reader.GetString(reader.GetOrdinal("born_effect"));
                desc.continuous_effect = reader.GetString(reader.GetOrdinal("continuous_effect"));
                desc.reading_effect = reader.GetString(reader.GetOrdinal("reading_effect"));
                desc.button_picture = reader.GetString(reader.GetOrdinal("button_picture"));
                desc.button_content = reader.GetString(reader.GetOrdinal("button_content"));
            }

            return bFind ? desc : null;
        }
        private bool FindbuttonConfigData(string key1, ref buttonConfig desc)
        {
            if(!m_buttonConfigDict.TryGetValue(key1, out desc))
            {
                desc = new buttonConfig();
                m_buttonConfigDict.Add(key1, desc);
                return false;
            }
            return true;
        }

        private Dictionary<string, characterConfig> m_characterConfigDict = new Dictionary<string, characterConfig>();
        public characterConfig GetcharacterConfigByID(string key1)
        {
            const string tableName = "characterConfig";
  
            characterConfig desc = null;
            if(FindcharacterConfigData(key1, ref desc))
                return desc;

            SqliteDataReader reader = m_Sql.ReadTable(tableName, "character_id", "=", key1.ToString());
            bool bFind = reader.HasRows;
            while(reader.Read())
            {                
                desc.character_id = reader.GetString(reader.GetOrdinal("character_id"));
                desc.radius = reader.GetFloat(reader.GetOrdinal("radius"));
                desc.weight = reader.GetFloat(reader.GetOrdinal("weight"));
                desc.move_velocity = reader.GetFloat(reader.GetOrdinal("move_velocity"));
                desc.hp_max = reader.GetFloat(reader.GetOrdinal("hp_max"));
                desc.hp_recover = reader.GetFloat(reader.GetOrdinal("hp_recover"));
                desc.sp_max = reader.GetFloat(reader.GetOrdinal("sp_max"));
                desc.sp_recover = reader.GetFloat(reader.GetOrdinal("sp_recover"));
                desc.attack_id = reader.GetString(reader.GetOrdinal("attack_id"));
                desc.skill_id = reader.GetString(reader.GetOrdinal("skill_id"));
                desc.search_radius = reader.GetFloat(reader.GetOrdinal("search_radius"));
                desc.assist_radius = reader.GetFloat(reader.GetOrdinal("assist_radius"));
                desc.mesh_id = reader.GetString(reader.GetOrdinal("mesh_id"));
                desc.button_icon = reader.GetString(reader.GetOrdinal("button_icon"));
                desc.target_height = reader.GetFloat(reader.GetOrdinal("target_height"));
                desc.hpbar_height = reader.GetFloat(reader.GetOrdinal("hpbar_height"));
                desc.hpbar_type = reader.GetString(reader.GetOrdinal("hpbar_type"));
                desc.show_level = reader.GetInt32(reader.GetOrdinal("show_level"));
                desc.unit_type = reader.GetInt32(reader.GetOrdinal("unit_type"));
                desc.move_animetion_playspeed = reader.GetFloat(reader.GetOrdinal("move_animetion_playspeed"));
                desc.hit_perform = reader.GetInt32(reader.GetOrdinal("hit_perform"));
                desc.scale = reader.GetFloat(reader.GetOrdinal("scale"));
                desc.born_effect = reader.GetString(reader.GetOrdinal("born_effect"));
                desc.recovery_effect = reader.GetString(reader.GetOrdinal("recovery_effect"));
                desc.deadbody_recovery_delay = reader.GetFloat(reader.GetOrdinal("deadbody_recovery_delay"));
            }

            return bFind ? desc : null;
        }
        private bool FindcharacterConfigData(string key1, ref characterConfig desc)
        {
            if(!m_characterConfigDict.TryGetValue(key1, out desc))
            {
                desc = new characterConfig();
                m_characterConfigDict.Add(key1, desc);
                return false;
            }
            return true;
        }

        private Dictionary<int, Dictionary<int, dialogueConfig>> m_dialogueConfigDict = new Dictionary<int, Dictionary<int, dialogueConfig>>();
        public dialogueConfig GetdialogueConfigByID(int key1, int key2)
        {
            const string tableName = "dialogueConfig";
  
            dialogueConfig desc = null;
            if(FinddialogueConfigData(key1, key2, ref desc))
                return desc;

            SqliteDataReader reader = m_Sql.ReadTable(tableName, new string[] {"dialogue_id", "dialogue_order"}, "=", new string[] {key1.ToString(), key2.ToString()});
            bool bFind = reader.HasRows;
            while(reader.Read())
            {                
                desc.dialogue_id = reader.GetInt32(reader.GetOrdinal("dialogue_id"));
                desc.dialogue_order = reader.GetInt32(reader.GetOrdinal("dialogue_order"));
                desc.dialogue_type = reader.GetInt32(reader.GetOrdinal("dialogue_type"));
                desc.dialogue_character = reader.GetString(reader.GetOrdinal("dialogue_character"));
                desc.character_model = reader.GetString(reader.GetOrdinal("character_model"));
                desc.dialogue_voice = reader.GetString(reader.GetOrdinal("dialogue_voice"));
                desc.duration_time = reader.GetInt32(reader.GetOrdinal("duration_time"));
                desc.dialogue_content = reader.GetString(reader.GetOrdinal("dialogue_content"));
            }

            return bFind ? desc : null;
        }
        private bool FinddialogueConfigData(int key1, int key2, ref dialogueConfig desc)
        {
            Dictionary<int, dialogueConfig> dict;
            if(!m_dialogueConfigDict.TryGetValue(key1, out dict))
            {
                dict = new Dictionary<int, dialogueConfig>();
                m_dialogueConfigDict.Add(key1, dict);
            }

            if(!dict.TryGetValue(key2, out desc))
            {
                desc = new dialogueConfig();
                dict.Add(key2, desc);
                return false;
            }
            return true;
        }      

        private Dictionary<int, MonsterConfig> m_MonsterConfigDict = new Dictionary<int, MonsterConfig>();
        public MonsterConfig GetMonsterConfigByID(int key1)
        {
            const string tableName = "MonsterConfig";
  
            MonsterConfig desc = null;
            if(FindMonsterConfigData(key1, ref desc))
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
        private bool FindMonsterConfigData(int key1, ref MonsterConfig desc)
        {
            if(!m_MonsterConfigDict.TryGetValue(key1, out desc))
            {
                desc = new MonsterConfig();
                m_MonsterConfigDict.Add(key1, desc);
                return false;
            }
            return true;
        }

        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, PlayerConfig>>>> m_PlayerConfigDict = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, PlayerConfig>>>>();
        public PlayerConfig GetPlayerConfigByID(string key1, string key2, string key3, int key4)
        {
            const string tableName = "PlayerConfig";
  
            PlayerConfig desc = null;
            if(FindPlayerConfigData(key1, key2, key3, key4, ref desc))
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
                desc.MonsterDesc = GetMonsterConfigByID(reader.GetInt32(reader.GetOrdinal("MonsterDesc")));
                Parse(ref desc.variant1, reader.GetString(reader.GetOrdinal("variant1")));
                Parse(ref desc.variant2, reader.GetString(reader.GetOrdinal("variant2")));
                Parse(ref desc.variant3, reader.GetString(reader.GetOrdinal("variant3")));
                Parse(ref desc.variant4, reader.GetString(reader.GetOrdinal("variant4")));
            }

            return bFind ? desc : null;
        }
        private bool FindPlayerConfigData(string key1, string key2, string key3, int key4, ref PlayerConfig desc)
        {
            Dictionary<string, Dictionary<string, Dictionary<int, PlayerConfig>>> dict;
            if(!m_PlayerConfigDict.TryGetValue(key1, out dict))
            {
                dict = new Dictionary<string, Dictionary<string, Dictionary<int, PlayerConfig>>>();
                m_PlayerConfigDict.Add(key1, dict);
            }

            Dictionary<string, Dictionary<int, PlayerConfig>> dict2;
            if(!dict.TryGetValue(key2, out dict2))
            {
                dict2 = new Dictionary<string, Dictionary<int, PlayerConfig>>();
                dict.Add(key2, dict2);
            }

            Dictionary<int, PlayerConfig> dict3;
            if(!dict2.TryGetValue(key3, out dict3))
            {
                dict3 = new Dictionary<int, PlayerConfig>();
                dict2.Add(key3, dict3);
            }

            if(!dict3.TryGetValue(key4, out desc))
            {
                desc = new PlayerConfig();
                dict3.Add(key4, desc);
                return false;
            }
            return true;
        }

        private Dictionary<string, skillConfig> m_skillConfigDict = new Dictionary<string, skillConfig>();
        public skillConfig GetskillConfigByID(string key1)
        {
            const string tableName = "skillConfig";
  
            skillConfig desc = null;
            if(FindskillConfigData(key1, ref desc))
                return desc;

            SqliteDataReader reader = m_Sql.ReadTable(tableName, "skill_id", "=", key1.ToString());
            bool bFind = reader.HasRows;
            while(reader.Read())
            {                
                desc.skill_id = reader.GetString(reader.GetOrdinal("skill_id"));
                desc.skill_cd = reader.GetFloat(reader.GetOrdinal("skill_cd"));
                desc.skill_range = reader.GetFloat(reader.GetOrdinal("skill_range"));
                desc.skill_moveable = reader.GetInt32(reader.GetOrdinal("skill_moveable"));
                desc.skill_duration = reader.GetFloat(reader.GetOrdinal("skill_duration"));
                desc.skill_fire_delay = reader.GetFloat(reader.GetOrdinal("skill_fire_delay"));
                desc.skill_fire_interval = reader.GetFloat(reader.GetOrdinal("skill_fire_interval"));
                desc.skill_fire_times = reader.GetInt32(reader.GetOrdinal("skill_fire_times"));
                desc.skill_range_type = reader.GetInt32(reader.GetOrdinal("skill_range_type"));
                desc.skill_range_param1 = reader.GetFloat(reader.GetOrdinal("skill_range_param1"));
                desc.skill_range_param2 = reader.GetFloat(reader.GetOrdinal("skill_range_param2"));
                desc.skill_range_param3 = reader.GetFloat(reader.GetOrdinal("skill_range_param3"));
                desc.skill_range_param4 = reader.GetFloat(reader.GetOrdinal("skill_range_param4"));
                desc.skill_damage_base = reader.GetFloat(reader.GetOrdinal("skill_damage_base"));
                desc.skill_damage_rate = reader.GetFloat(reader.GetOrdinal("skill_damage_rate"));
                desc.skill_explode_damage_base = reader.GetFloat(reader.GetOrdinal("skill_explode_damage_base"));
                desc.skill_explode_damage_rate = reader.GetFloat(reader.GetOrdinal("skill_explode_damage_rate"));
                desc.perform_anime = reader.GetString(reader.GetOrdinal("perform_anime"));
                desc.attack_effect_point = reader.GetString(reader.GetOrdinal("attack_effect_point"));
                desc.attack_effect = reader.GetString(reader.GetOrdinal("attack_effect"));
                desc.perform_type = reader.GetInt32(reader.GetOrdinal("perform_type"));
                desc.perform_param = reader.GetFloat(reader.GetOrdinal("perform_param"));
                desc.bullet_penetrate = reader.GetInt32(reader.GetOrdinal("bullet_penetrate"));
                desc.bullet_effect = reader.GetString(reader.GetOrdinal("bullet_effect"));
                desc.bullet_tail_effect = reader.GetString(reader.GetOrdinal("bullet_tail_effect"));
                desc.bullet_lifetime = reader.GetFloat(reader.GetOrdinal("bullet_lifetime"));
                desc.hit_unit_effect = reader.GetString(reader.GetOrdinal("hit_unit_effect"));
                desc.hit_unit_pause = reader.GetFloat(reader.GetOrdinal("hit_unit_pause"));
                desc.hit_unit_twinkle = reader.GetFloat(reader.GetOrdinal("hit_unit_twinkle"));
                desc.hit_ground_effect = reader.GetString(reader.GetOrdinal("hit_ground_effect"));
                desc.gunpoint_type = reader.GetInt32(reader.GetOrdinal("gunpoint_type"));
                desc.gunpoint_effect = reader.GetString(reader.GetOrdinal("gunpoint_effect"));
                desc.perform_bullet_type = reader.GetInt32(reader.GetOrdinal("perform_bullet_type"));
                Parse(ref desc.skill_camp, reader.GetString(reader.GetOrdinal("skill_camp")));
                Parse(ref desc.gunpoint_name, reader.GetString(reader.GetOrdinal("gunpoint_name")));
                Parse(ref desc.perform_bullet_name, reader.GetString(reader.GetOrdinal("perform_bullet_name")));
            }

            return bFind ? desc : null;
        }
        private bool FindskillConfigData(string key1, ref skillConfig desc)
        {
            if(!m_skillConfigDict.TryGetValue(key1, out desc))
            {
                desc = new skillConfig();
                m_skillConfigDict.Add(key1, desc);
                return false;
            }
            return true;
        }

        private Dictionary<string, stageConfig> m_stageConfigDict = new Dictionary<string, stageConfig>();
        public stageConfig GetstageConfigByID(string key1)
        {
            const string tableName = "stageConfig";
  
            stageConfig desc = null;
            if(FindstageConfigData(key1, ref desc))
                return desc;

            SqliteDataReader reader = m_Sql.ReadTable(tableName, "stage_id", "=", key1.ToString());
            bool bFind = reader.HasRows;
            while(reader.Read())
            {                
                desc.stage_id = reader.GetString(reader.GetOrdinal("stage_id"));
                desc.stage_editor_id = reader.GetString(reader.GetOrdinal("stage_editor_id"));
                desc.player_lv_requirement = reader.GetInt32(reader.GetOrdinal("player_lv_requirement"));
            }

            return bFind ? desc : null;
        }
        private bool FindstageConfigData(string key1, ref stageConfig desc)
        {
            if(!m_stageConfigDict.TryGetValue(key1, out desc))
            {
                desc = new stageConfig();
                m_stageConfigDict.Add(key1, desc);
                return false;
            }
            return true;
        }

    }
}


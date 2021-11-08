using System.Collections;
using System.Collections.Generic;

namespace Application.Runtime
{
    public class button
    {
        public string button_id;
        public int duration_time;
        public string born_effect;
        public string continuous_effect;
        public string reading_effect;
        public string button_picture;
        public string button_content;
    }

    public class character
    {
        public string character_id;
        public float radius;
        public float weight;
        public float move_velocity;
        public float hp_max;
        public float hp_recover;
        public float sp_max;
        public float sp_recover;
        public string attack_id;
        public string skill_id;
        public float search_radius;
        public float assist_radius;
        public string mesh_id;
        public string button_icon;
        public float target_height;
        public float hpbar_height;
        public string hpbar_type;
        public int show_level;
        public int unit_type;
        public float move_animetion_playspeed;
        public int hit_perform;
        public float scale;
        public string born_effect;
        public string recovery_effect;
        public float deadbody_recovery_delay;
    }

    public class dialogue
    {
        public int dialogue_id;
        public int dialogue_order;
        public int dialogue_type;
        public string dialogue_character;
        public string character_model;
        public string dialogue_voice;
        public int duration_time;
        public string dialogue_content;
    }

    public class initialization
    {
        public string initialization_id;
    }

    public class Monster
    {
        public int ID;
        public string Name;
        public float HP;
        public bool Male;
    }

    public class Player
    {
        public string Building_ID;
        public string Name;
        public string Address;
        public int HP;
        public bool Male;
        public List<string> variant1 = new List<string>();
        public List<int> variant2 = new List<int>();
        public List<float> variant3 = new List<float>();
        public Monster MonsterDesc;
        public Dictionary<int, string> variant4 = new Dictionary<int, string>();
    }

    public class skill
    {
        public string skill_id;
        public float skill_cd;
        public float skill_range;
        public int skill_moveable;
        public float skill_duration;
        public float skill_fire_delay;
        public float skill_fire_interval;
        public int skill_fire_times;
        public int skill_range_type;
        public float skill_range_param1;
        public float skill_range_param2;
        public float skill_range_param3;
        public float skill_range_param4;
        public List<int> skill_camp = new List<int>();
        public float skill_damage_base;
        public float skill_damage_rate;
        public float skill_explode_damage_base;
        public float skill_explode_damage_rate;
        public string perform_anime;
        public string attack_effect_point;
        public string attack_effect;
        public int perform_type;
        public float perform_param;
        public int bullet_penetrate;
        public string bullet_effect;
        public string bullet_tail_effect;
        public float bullet_lifetime;
        public string hit_unit_effect;
        public float hit_unit_pause;
        public float hit_unit_twinkle;
        public string hit_ground_effect;
        public List<string> gunpoint_name = new List<string>();
        public int gunpoint_type;
        public string gunpoint_effect;
        public List<string> perform_bullet_name = new List<string>();
        public int perform_bullet_type;
    }

    public class stage
    {
        public string stage_id;
        public string stage_editor_id;
        public int player_lv_requirement;
    }

}
using System.Collections;
using System.Collections.Generic;

namespace Application.Runtime
{
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

}
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
        public int ID;
        public string Name;
        public float HP;
        public bool Male;
        public List<string> variant1;
        public List<int> variant2;
        public List<float> variant3;
    }

}
using System;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Framework.Core.Tests
{
    public class JsonNetSample : MonoBehaviour
    {
        public void Start()
        {
            LinqToJson();
            JsonPath();

            TestItemInfo();
            TestJson();

            SerailizeJson();
            DeserializeJson();

            //TestDicSerialize();
            //TestDicDeserialize();

            WriteLine("\nDone!");
        }

        void WriteLine(string msg)
        {
            Debug.Log(msg);
        }

        void LinqToJson()
        {
            WriteLine("* LinqToJson");

            JArray array = new JArray();
            array.Add("Manual text");
            array.Add(new DateTime(2000, 5, 23));

            JObject o = new JObject();
            o["MyArray"] = array;

            string json = o.ToString();
            WriteLine(json);
        }

        private void JsonPath()
        {
            WriteLine("* JsonPath");

            var o = JObject.Parse(@"{
            'Stores': [
            'Lambton Quay',
            'Willis Street'
            ],
            'Manufacturers': [
            {
                'Name': 'Acme Co',
                'Products': [
                {
                    'Name': 'Anvil',
                    'Price': 50
                }
                ]
            },
            {
                'Name': 'Contoso',
                'Products': [
                {
                    'Name': 'Elbow Grease',
                    'Price': 99.95
                },
                {
                    'Name': 'Headlight Fluid',
                    'Price': 4
                }
                ]
            }
            ]
            }");

            JToken acme = o.SelectToken("$.Manufacturers[?(@.Name == 'Acme Co')]");
            WriteLine(acme.ToString());

            IEnumerable<JToken> pricyProducts = o.SelectTokens("$..Products[?(@.Price >= 50)].Name");
            foreach (var item in pricyProducts)
            {
                WriteLine(item.ToString());
            }
        }

        //[System.Serializable]
        public class CharacterListItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public string Class { get; set; }
            public string Sex { get; set; }
        }

        void TestJson()
        {
            WriteLine("* TestJson");
            var json = "{\"Id\":51, \"Name\":\"padre\", \"Level\":-1, \"Class\":\"Vampire\", \"Sex\":\"F\"}";
            //var json = "{'Id':'51', 'Name':'padre', 'Level':'0', 'Class':'Vampire', 'Sex':'F'}";

            var c = JsonConvert.DeserializeObject<CharacterListItem>(json);
            WriteLine(c.Id + " " + c.Name);
        }


        public class Product
        {
            public string Name;
            public DateTime ExpiryDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            public decimal Price;
            public string[] Sizes;

            public override bool Equals(object obj)
            {
                if (obj is Product)
                {
                    Product p = (Product)obj;

                    return (p.Name == Name && p.ExpiryDate == ExpiryDate && p.Price == Price);
                }

                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return (Name ?? string.Empty).GetHashCode();
            }
        }

        void SerailizeJson()
        {
            WriteLine("* SerailizeJson");

            Product product = new Product();
            product.Name = "Apple";
            product.ExpiryDate = new DateTime(2008, 12, 28);
            product.Sizes = new string[] { "Small" };

            string json = JsonConvert.SerializeObject(product);
            WriteLine(json);
        }

        public class Movie
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Classification { get; set; }
            public string Studio { get; set; }
            public DateTime? ReleaseDate { get; set; }
            public List<string> ReleaseCountries { get; set; }
        }

        void DeserializeJson()
        {
            WriteLine("* DeserializeJson");

            string json = @"{
          'Name': 'Bad Boys',
          'ReleaseDate': '1995-4-7T00:00:00',
          'Genres': [
            'Action',
            'Comedy'
          ]
        }";

            Movie m = JsonConvert.DeserializeObject<Movie>(json);

            string name = m.Name;
            WriteLine(name);
        }


        [Serializable]
        public class TestClass
        {
            public string Name = "";
            public int Value;

            public TestClass(string n, int v)
            {
                Name = n;
                Value = v;
            }

            //public override bool Equals(object obj)
            //{
            //    TestClass other = obj as TestClass;
            //    if (other == null)
            //        return false;

            //    if (!base.GetType().Equals(obj.GetType()))
            //        return false;

            //    return (this.Name.Equals(other.Name));
            //}

            //public override int GetHashCode()
            //{
            //    return Name.GetHashCode();
            //}

            //public static explicit operator TestClass(string jsonString)
            //{
            //    return Newtonsoft.Json.JsonConvert.DeserializeObject<TestClass>(jsonString);
            //}

            //public override string ToString()
            //{
            //    return Newtonsoft.Json.JsonConvert.SerializeObject(this);
            //}
        }

        /// <summary>
        /// Json文本格式
        /*{
             "dic":
             {
                 "0":{"Name":"c0","Value":11},
                 "3":{"Name":"c1","Value":22},
                 "2":{"Name":"c2","Value":33},
                 "1":{"Name":"c3","Value":44}
             }
         }*/
        /// </summary>
        public class AllTestClass
        {
            public Dictionary<int, TestClass> dic = new Dictionary<int, TestClass>();
        }

        void TestDicSerialize()
        {
            //AllTestClass atc = new AllTestClass();
            Dictionary<int, TestClass> dic = new Dictionary<int, TestClass>();
            TestClass c = new TestClass("c0", 11);
            dic.Add(0, c);
            TestClass c1 = new TestClass("c1", 22);
            dic.Add(3, c1);
            TestClass c2 = new TestClass("c2", 33);
            dic.Add(2, c2);
            TestClass c3 = new TestClass("c3", 44);
            dic.Add(1, c3);
            //atc.dic = dic;

            string json = JsonConvert.SerializeObject(/*atc*/dic, Formatting.Indented);

            System.IO.FileStream fs = new System.IO.FileStream("assets/scripts/core/hybrid/tests/runtime/dic.txt", System.IO.FileMode.Create);
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(json);
            fs.Write(bs, 0, bs.Length);
            fs.Close();
        }

        void TestDicDeserialize()
        {
            System.IO.FileStream fs = new System.IO.FileStream("assets/scripts/core/hybrid/tests/runtime/dic.txt", System.IO.FileMode.Open);
            byte[] array = new byte[256];
            int size = fs.Read(array, 0, 256);
            fs.Close();
            string jsong = System.Text.Encoding.UTF8.GetString(array, 0, size);

            Dictionary<int, TestClass> dic = JsonConvert.DeserializeObject<Dictionary<int, TestClass>>(jsong);
            //AllTestClass atc = JsonConvert.DeserializeObject<AllTestClass>(jsong);
        }


        /// <summary>
        /// Json文本格式    
        /// </summary>
        /*
         * 
    {
        "itemList": [
            {
                "ID": "1001",
                "Name": "生命药水",
                "Des": "回复一定量的生命",
                "Price": "100",
                "Type": "药品",
                "WearType": "null",
                "WearLv": "0",
                "Quality": "普通",
                "Atk": "0",
                "Def": "0",
                "CurHp": "100",
                "MaxHp": "0",
                "CurMp": "0",
                "MaxMp": "0"
            },
            {
                "ID": "1002",
                "Name": "魔法药水",
                "Des": "回复一定量的魔法",
                "Price": "100",
                "Type": "药品",
                "WearType": "null",
                "WearLv": "0",
                "Quality": "普通",
                "Atk": "0",
                "Def": "0",
                "CurHp": "0",
                "MaxHp": "0",
                "CurMp": "100",
                "MaxMp": "0"
            },
            {
                "ID": "2001",
                "Name": "屠龙刀",
                "Des": "武林至尊宝刀屠龙号令天下谁敢不从",
                "Price": "2000",
                "Type": "装备",
                "WearType": "武器",
                "WearLv": "1",
                "Quality": "普通",
                "Atk": "200",
                "Def": "0",
                "CurHp": "0",
                "MaxHp": "0",
                "CurMp": "0",
                "MaxMp": "0"
            }
        ]
    }
         */

        public TextAsset m_TextAsset;
        private AllItemInfo m_ItemInfo;

        public class ItemInfo
        {
            public string Id;
            public string Name;
            public string des;
            public int price;
        }

        public class AllItemInfo
        {
            public List<ItemInfo> itemList = new List<ItemInfo>();
        }

        public void TestItemInfo()
        {
            m_ItemInfo = JsonConvert.DeserializeObject<AllItemInfo>(m_TextAsset.text);
        }
    }
}
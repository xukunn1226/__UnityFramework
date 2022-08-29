using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;

namespace Framework.AssetManagement.AssetProcess
{
    public class jsonSeriTest
    {
        static string JsonPath = UnityEngine.Application.dataPath + "/Framework/AssetManagement/AssetPresetTool/AssetProcessorData.json";
        public static void JsonSerialize(List<Category> categorys)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.Auto;
            string jsonStr = JsonConvert.SerializeObject(categorys,settings);
            File.WriteAllText(JsonPath, jsonStr);
        }

        //[MenuItem("JsonTest/JsonDeserialize")]
        public static List<Category> JsonDeserializeTest()
        {
            string jsonStr =File.ReadAllText(JsonPath);
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            settings.Formatting = Formatting.Indented;
            List<Category> res = JsonConvert.DeserializeObject<List<Category>>(jsonStr,settings);
            if(res == null)
            {
                res = new List<Category>();
            }
            return res;
        }
    }
}

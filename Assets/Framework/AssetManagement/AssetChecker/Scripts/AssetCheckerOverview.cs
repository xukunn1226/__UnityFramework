using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEditor;

namespace Framework.AssetManagement.AssetChecker
{
    public class AssetCheckerOverview
    {
        public const string kJsonAssetPath = "Assets/Framework/AssetManagement/AssetChecker/Editor/AssetCheckerOverview.json";

        public List<AssetChecker> AllCheckers { get; set; } = new List<AssetChecker>();

        static public void Serialize(AssetCheckerOverview overview)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            string json = JsonConvert.SerializeObject(overview, Formatting.Indented, settings);

            using (FileStream fs = new FileStream(kJsonAssetPath, FileMode.Create))
            {
                byte[] bs = System.Text.Encoding.UTF8.GetBytes(json);
                fs.Write(bs, 0, bs.Length);
            }
            AssetDatabase.ImportAsset(kJsonAssetPath);
        }

        static public AssetCheckerOverview Deserialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            using (FileStream fs = File.OpenRead(kJsonAssetPath))
            {
                byte[] array = new byte[1024 * 256];
                int size = fs.Read(array, 0, 1024 * 256);
                return JsonConvert.DeserializeObject<AssetCheckerOverview>(System.Text.Encoding.UTF8.GetString(array, 0, size), settings);
            }
        }
    }
}
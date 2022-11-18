using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEditor;

namespace Framework.AssetManagement.AssetChecker
{
    public class AssetProcessorOverview
    {
        public const string kJsonAssetPath = "Assets/Framework/AssetManagement/AssetChecker/Editor/AssetProcessorOverview.json";

        [SerializeField]
        public List<AssetChecker> AllCheckers { get; private set; } = new List<AssetChecker>();

        public void Add(AssetChecker item)
        {
            AllCheckers.Add(item);
        }

        public void Remove(AssetChecker item)
        {
            AllCheckers.Remove(item);
        }

        static public AssetProcessorOverview GetOrCreate()
        {
            if(File.Exists(kJsonAssetPath))
            {
                return Deserialize();
            }
            AssetProcessorOverview overview = new AssetProcessorOverview();
            Serialize(overview);
            return overview;
        }

        static public void Save(AssetProcessorOverview overview)
        {
            Serialize(overview);
        }

        static private void Serialize(AssetProcessorOverview overview)
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

        static private AssetProcessorOverview Deserialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.All;
            using (FileStream fs = File.OpenRead(kJsonAssetPath))
            {
                byte[] array = new byte[1024 * 256];
                int size = fs.Read(array, 0, 1024 * 256);
                return JsonConvert.DeserializeObject<AssetProcessorOverview>(System.Text.Encoding.UTF8.GetString(array, 0, size), settings);
            }
        }

        static public void DoProcessorAndExportAll(AssetProcessorOverview overview)
        {
            foreach(var item in overview.AllCheckers)
            {
                item.DoProcessorAndExport();
            }
        }

        // CI½Ó¿Ú
        static public void cmdDoProcessorAndExportAll()
        {
            AssetProcessorOverview overview = GetOrCreate();
            DoProcessorAndExportAll(overview);
        }
    }
}
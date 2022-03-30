using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using System.Text.RegularExpressions;

namespace Framework.AssetManagement.AssetBuilder
{
    [System.Serializable]
    public class TextureImporterProcessor : IAssetProcessor
    {
        public string Filter;
        public Preset Preset; 
        private Regex m_Reg;        

        public bool IsMatch(string filename, string extension)
        {
            if(string.IsNullOrEmpty(Filter))
                return true;
            
            if(m_Reg == null)
            {
                m_Reg = new Regex(Filter, RegexOptions.IgnoreCase);
            }

            return m_Reg.IsMatch(filename);
        }

        public string Execute(string assetPath)
        {
            if(Preset == null || !Preset.IsValid())
                return string.Format($"{typeof(TextureImporterProcessor).Name}: 导入资源{assetPath}时发生如下事件：Preset == null or not valid\n");

            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (!Preset.CanBeAppliedTo(importer))
            {
                return string.Format($"{typeof(TextureImporterProcessor).Name}: 不能应用于资源[{assetPath}]，请检查过滤器或资源命名\n");
            }
            Preset.ApplyTo(importer);
            AssetDatabase.ImportAsset(assetPath);
            return null;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    public class ShaderManager : SingletonMono<ShaderManager>
    {
        [System.Serializable]
        public class ShaderMappingInfo
        {
            public string shaderName;
            public string assetPath;
        }
        [SerializeField]
        private List<ShaderMappingInfo>                         m_ShaderInfoList    = new List<ShaderMappingInfo>();

        static private Dictionary<string, string>               m_ShaderNameToPath  = new Dictionary<string, string>();
        static private Dictionary<string, AssetLoader<Shader>>  m_ShaderLoaderDict  = new Dictionary<string, AssetLoader<Shader>>();

        protected override void Awake()
        {
            base.Awake();

            foreach(var item in m_ShaderInfoList)
            {
                m_ShaderNameToPath.Add(item.shaderName, item.assetPath);
            }
        }

        protected override void OnDestroy()
        {
            foreach(var item in m_ShaderLoaderDict)
            {
                AssetManager.UnloadAsset(item.Value);
            }
            m_ShaderLoaderDict.Clear();
            base.OnDestroy();
        }

        static public Shader Find(string shaderName)
        {
#if UNITY_EDITOR
            if(!UnityEditor.EditorApplication.isPlaying)
            {
                return Shader.Find(shaderName);
            }
#endif
            if (m_ShaderNameToPath.Count == 0)
            {
                return Shader.Find(shaderName);
            }

            string assetPath;
            if(!m_ShaderNameToPath.TryGetValue(shaderName, out assetPath))
            {
                Debug.LogError($"can't find config match to {shaderName}, plz check ShaderManager");
                return null;
            }
            
            AssetLoader<Shader> loader;
            if (!m_ShaderLoaderDict.TryGetValue(shaderName, out loader))
            {
                loader = AssetManager.LoadAsset<Shader>(assetPath);
                m_ShaderLoaderDict.Add(shaderName, loader);
            }

            return loader.asset;
        }
    }
}
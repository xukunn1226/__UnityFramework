using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Application.Runtime;
using UnityEngine.U2D;

namespace Application.Logic
{
    public class AtlasManager
    {
        static private string       AtlasRootPath       = "assets/res/ui/atlases";                              // 图集资源目录
        static private string[]     m_PersistentAtlas   = new string[]  {                                       // 常驻内存的图集
                                                                            "icon0", 
                                                                            "icon1" 
                                                                        };
        static private Dictionary<string, AssetLoader<SpriteAtlas>>  m_AtlasLoaderDict = new Dictionary<string, AssetLoader<SpriteAtlas>>();   // 图集loader

        static public void InitPersistentAtlas()
        {
            UninitPersistentAtlas();
            for(int i = 0; i < m_PersistentAtlas.Length; ++i)
            {
                GetOrLoadSpriteAtlas(m_PersistentAtlas[i]);
            }
        }

        static public void UninitPersistentAtlas()
        {
            foreach(var item in m_AtlasLoaderDict)
            {
                AssetManager.UnloadAsset(item.Value);
            }
            m_AtlasLoaderDict.Clear();
        }

        static private AssetLoader<SpriteAtlas> GetOrLoadSpriteAtlas(string atlasName)
        {
            AssetLoader<SpriteAtlas> loader;
            if(!m_AtlasLoaderDict.TryGetValue(atlasName, out loader))
            {
                string assetPath = string.Format($"{AtlasRootPath}/{atlasName}/{atlasName}.spriteatlas");
                loader = AssetManager.LoadAsset<SpriteAtlas>(assetPath);
                m_AtlasLoaderDict.Add(atlasName, loader);
                if(loader.asset == null)
                {
                    UnityEngine.Debug.LogError($"GetOrLoadSpriteAtlas: failed to load sprite atlas from asset path [{assetPath}]");
                }
            }
            return loader;
        }

        static public Sprite GetSprite(string atlasName, string spriteName)
        {
            AssetLoader<SpriteAtlas> loader = GetOrLoadSpriteAtlas(atlasName);
            return loader.asset?.GetSprite(spriteName);
        }                                                                                    
    }
}
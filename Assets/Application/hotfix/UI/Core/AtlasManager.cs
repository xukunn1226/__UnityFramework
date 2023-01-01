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
        static private Dictionary<string, AssetOperationHandle> m_AtlasLoaderDict = new Dictionary<string, AssetOperationHandle>();

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
            foreach(var pair in m_AtlasLoaderDict)
            {
                pair.Value.Release();
            }
            m_AtlasLoaderDict.Clear();
        }

        static private AssetOperationHandle GetOrLoadSpriteAtlas(string atlasName)
        {
            AssetOperationHandle handle;
            if(m_AtlasLoaderDict.TryGetValue(atlasName, out handle) == false)
            {
                string assetPath = string.Format($"{AtlasRootPath}/{atlasName}/{atlasName}.spriteatlas");
                handle = AssetManagerEx.LoadAsset<SpriteAtlas>(assetPath);
                m_AtlasLoaderDict.Add(atlasName, handle);
                if (handle.assetObject == null)
                {
                    UnityEngine.Debug.LogError($"GetOrLoadSpriteAtlas: failed to load sprite atlas from asset path [{assetPath}]");
                }
            }
            return handle;
        }

        static public Sprite GetSprite(string atlasName, string spriteName)
        {
            var loader = GetOrLoadSpriteAtlas(atlasName);
            return ((SpriteAtlas)loader.assetObject)?.GetSprite(spriteName);
        }                                                                                    
    }
}
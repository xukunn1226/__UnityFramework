using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    static public class LuaMainLoop
    {
        private const string m_CustomLuaPath = "assets/application/xlua/lua";
        static private LuaEnv m_LuaEnv;
        static private Dictionary<string, AssetLoader<TextAsset>> m_LuaScriptLoaders = new Dictionary<string, AssetLoader<TextAsset>>();

        static public void Init()
        {
            if(m_LuaEnv != null)
            {
                throw new System.Exception("LuaMainLoop::Init   m_LuaEnv != null");
            }
            m_LuaEnv = new LuaEnv();
            SetupCustomLoader();
            m_LuaEnv.DoString("require 'luamainloop.lua'");
        }

        static public void Uninit()
        {
            foreach(var loader in m_LuaScriptLoaders)
            {
                ResourceManager.UnloadAsset(loader.Value);
            }
            m_LuaScriptLoaders.Clear();

            m_LuaEnv?.Dispose();
            m_LuaEnv = null;
        }

        static private void SetupCustomLoader()
        {
            LoaderType type = Launcher.GetLauncherMode();
            switch(type)
            {
                case LoaderType.FromEditor:
                    m_LuaEnv.AddLoader(CustomLoaderFromEditor);
                    break;
                case LoaderType.FromStreamingAssets:
                case LoaderType.FromPersistent:
                    m_LuaEnv.AddLoader(CustomLoaderFromBundle);
                    break;
            }
        }

	    static private byte[] CustomLoaderFromEditor(ref string filepath)
        {
            filepath = string.Format($"{m_CustomLuaPath}/{filepath.ToLower()}.bytes");
            string txtString = System.IO.File.ReadAllText(filepath);
            return System.Text.Encoding.UTF8.GetBytes(txtString);
        }

        static private byte[] CustomLoaderFromBundle(ref string filepath)
        {
            if(m_LuaScriptLoaders.ContainsKey(filepath))
            {
                Debug.LogError($"lua script [{filepath}] has already loaded.");
                return null;
            }
            
            AssetLoader<TextAsset> loader = ResourceManager.LoadAsset<TextAsset>(string.Format($"{m_CustomLuaPath}/{filepath.ToLower()}.bytes"));
            if(loader == null || loader.asset == null)
            {
                Debug.LogError($"failed to load TextAsset from {m_CustomLuaPath}/{filepath}");
                return null;
            }
            m_LuaScriptLoaders.Add(filepath, loader);
            return loader.asset.bytes;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    /// <summary>
    /// Lua管理器入口
    /// 注释LUA框架的几个步骤：
    /// 1、注释Init,Uninit,Tick调用
    /// 2、注释XLuaConfig.IPreprocessBuildWithReport
    /// </summary>
    static public class LuaMainLoop
    {
        private const string m_CustomLuaPath = "assets/application/main/xlua/lua";
        static private LuaEnv m_LuaEnv;
        static private Dictionary<string, AssetOperationHandle>     m_LuaScriptLoaderDict = new Dictionary<string, AssetOperationHandle>();

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
            foreach(var pair in m_LuaScriptLoaderDict)
            {
                pair.Value.Release();
            }
            m_LuaScriptLoaderDict.Clear();

            m_LuaEnv?.Dispose();
            m_LuaEnv = null;
        }

        static public void Tick()
        {
            m_LuaEnv?.Tick();
        }

        static private void SetupCustomLoader()
        {
            EPlayMode mode = Launcher.GetPlayMode();
            switch(mode)
            {
                case EPlayMode.FromEditor:
                    m_LuaEnv.AddLoader(CustomLoaderFromEditor);
                    break;
                case EPlayMode.FromStreaming:
                case EPlayMode.FromHost:
                    m_LuaEnv.AddLoader(CustomLoaderFromBundle);
                    break;
            }
        }

	    static private byte[] CustomLoaderFromEditor(ref string filepath)
        {
            filepath = string.Format($"{m_CustomLuaPath}/{filepath.ToLower()}");
            string txtString = System.IO.File.ReadAllText(filepath);
            return System.Text.Encoding.UTF8.GetBytes(txtString);
        }

        // lua脚本当作TextAsset加载
        // static private byte[] CustomLoaderFromBundle(ref string filepath)
        // {
        //     if(m_LuaScriptLoaders.ContainsKey(filepath))
        //     {
        //         Debug.LogError($"lua script [{filepath}] has already loaded.");
        //         return null;
        //     }
            
        //     AssetLoader<TextAsset> loader = ResourceManager.LoadAsset<TextAsset>(string.Format($"{m_CustomLuaPath}/{filepath.ToLower()}.bytes"));
        //     if(loader == null || loader.asset == null)
        //     {
        //         Debug.LogError($"failed to load TextAsset from {m_CustomLuaPath}/{filepath}");
        //         return null;
        //     }
        //     m_LuaScriptLoaders.Add(filepath, loader);
        //     return loader.asset.bytes;
        // }

        // lua脚本当作LuaAsset加载
        static private byte[] CustomLoaderFromBundle(ref string filepath)
        {
            if (m_LuaScriptLoaderDict.ContainsKey(filepath))
            {
                Debug.LogError($"lua script [{filepath}] has already loaded.");
                return null;
            }

            AssetOperationHandle handle = AssetManagerEx.LoadAsset<LuaAsset>(string.Format($"{m_CustomLuaPath}/{filepath.ToLower()}"));
            if (handle.status == EOperationStatus.Failed)
                return null;
            m_LuaScriptLoaderDict.Add(filepath, handle);
            return ((LuaAsset)handle.assetObject).Require();
        }
    }
}
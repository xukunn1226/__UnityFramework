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

        static public void Init()
        {
            if(m_LuaEnv != null)
            {
                throw new System.Exception("LuaMainLoop::Init   m_LuaEnv != null");
            }
            m_LuaEnv = new LuaEnv();
            m_LuaEnv.AddLoader(CustomLoaderFromEditor);
            m_LuaEnv.DoString("require 'LuaMainLoop.lua'");
        }

        static public void Uninit()
        {
            m_LuaEnv?.Dispose();
            m_LuaEnv = null;
        }

	    static private byte[] CustomLoaderFromEditor(ref string filepath)
        {
            filepath = m_CustomLuaPath + "/" + filepath;
            string txtString = System.IO.File.ReadAllText(filepath);
            return System.Text.Encoding.UTF8.GetBytes(txtString);
        }

        static private byte[] CustomLoaderFromBundle(ref string filepath)
        {
            filepath = m_CustomLuaPath + "/" + filepath;
            AssetLoader<TextAsset> loader = ResourceManager.LoadAsset<TextAsset>(filepath);
            return null;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Application.Runtime
{
    static public class LuaMainLoop
    {
        private const string m_CustomLuaPath = "Assets/Application/XLua/Lua";

        static private LuaEnv m_LuaEnv;

        static public void Init()
        {
            if(m_LuaEnv != null)
            {
                throw new System.Exception("LuaMainLoop::Init   m_LuaEnv != null");
            }
            m_LuaEnv = new LuaEnv();
            m_LuaEnv.AddLoader(MyCustomLoader);
            m_LuaEnv.DoString("require 'LuaMainLoop.lua'");
        }

        static public void Uninit()
        {
            m_LuaEnv?.Dispose();
            m_LuaEnv = null;
        }

	    static private byte[] MyCustomLoader(ref string filepath)
        {
            filepath = m_CustomLuaPath + "/" + filepath;
            string txtString = System.IO.File.ReadAllText(filepath);
            return System.Text.Encoding.UTF8.GetBytes(txtString);
        }
    }
}
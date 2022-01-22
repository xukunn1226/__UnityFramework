using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Application.Logic
{
    public class UIDefines
    {
        static private Dictionary<int, UIDefines> s_Defines = new Dictionary<int, UIDefines>();
        static public string    s_RootPath      = "";                           // root path of ui prefab
        public virtual int      id              { get; protected set; }
        public virtual string   layer           { get; protected set; }
        public virtual string   path            { get; protected set; }
        public virtual int      hideMode        { get; protected set; }         // 隐藏时的方式（SetActive（false）、out of screen、disable canvas、set view layer）
        public virtual bool     isPersistent    { get; protected set; }         // true: 常驻内存，不会被销毁; false: LRU管理

        protected UIDefines()
        {}

        static public UIDefines Get(int id)
        {
            UIDefines def;
            s_Defines.TryGetValue(id, out def);
            return def;
        }

        static public void Init()
        {
            List<Type> types = FindAllDerivedTypes<UIDefines>();
            int count = types.Count;
            for(int i = 0; i < count; ++i)
            {
                UIDefines def = (UIDefines)Activator.CreateInstance(types[i]);
                s_Defines.Add(def.id, def);
            }
        }

        public static List<Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }

        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly.GetTypes().Where(t => t != derivedType && derivedType.IsAssignableFrom(t)).ToList();
        }
    }

    public class UILayer
    {
        static public string    Main        { get; private set; }   = "_Main";
        static public string    Fullscreen  { get; private set; }   = "_Fullscreen";
        static public string    Windowed    { get; private set; }   = "_Windowed";
        static public string    Tips        { get; private set; }   = "_Tips";
        static public string    Popup       { get; private set; }   = "_Popup";
        static public string    Loading     { get; private set; }   = "_Loading";
        static public string    Alert       { get; private set; }   = "_Alert";

        // static public IEnumerable<System.Object> LayerToLoad()
        // {
        //     yield return MainLayer;
        //     yield return FullscreenLayer;
        //     yield return WindowedLayer;
        //     yield return TipsLayer;
        //     yield return PopupLayer;
        //     yield return LoadingLayer;
        //     yield return AlertLayer;
        // }
        static public IEnumerator<System.Object> LayerToLoad()
        {
            yield return Main;
            yield return Fullscreen;
            yield return Windowed;
            yield return Tips;
            yield return Popup;
            yield return Loading;
            yield return Alert;
        }
    }
    
    public class UIPanelID
    {
        static public int       Main            { get; private set; }   = 0;
        static public int       Login           { get; private set; }   = 12;
        static public int       Setting         { get; private set; }   = 2;
        static public int       Guild           { get; private set; }   = 3;
    }
}
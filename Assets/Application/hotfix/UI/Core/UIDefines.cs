using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Application.Logic
{
    public class UIDefines
    {
        static private Dictionary<string, UIDefines> s_Defines = new Dictionary<string, UIDefines>();
        public virtual string       id              { get; protected set; }     // see UIPanelID
        public virtual string       layer           { get; protected set; }     // see UILayer
        public virtual string       assetPath       { get; protected set; }     // asset path
        public virtual EHideMode    hideMode        { get; protected set; }     // 隐藏时的方式（SetActive（false）、out of screen、disable canvas、set view layer）
        public virtual bool         isPersistent    { get; protected set; }     // true: 常驻内存，不会被销毁; false: LRU管理
        public virtual Type         typeOfPanel     { get; protected set; }     // type of panel

        protected UIDefines()
        {}

        static public UIDefines Get(string id)
        {
            UIDefines def;
            s_Defines.TryGetValue(id, out def);
            return def;
        }

        /// <summary>
        /// 初始化所有界面的UIDefine
        /// </summary>
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

    public enum EHideMode
    {
        SetActive,
        OutOfScreen,
        DisableCanvas,
        OutOfViewLayer,
    }
    
    public partial class UIPanelID
    {
        static public string    Setting         { get; private set; }   = "Setting";
        static public string    Guild           { get; private set; }   = "Guild";
    }
}
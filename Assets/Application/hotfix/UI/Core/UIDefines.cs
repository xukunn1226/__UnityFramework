using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{
    public class UIDefines
    {
        static public string s_RootPath = "";                               // root path of ui prefab
        public UIPanelType  panelType       { get; protected set; }
        public string       path            { get; protected set; }
        public bool         isFullscreen    { get; protected set; }
        public int          hideMode        { get; protected set; }         // 隐藏时的方式（SetActive（false）、out of screen、disable canvas、set view layer）
        public bool         registerUpdate  { get; protected set; }         // 是否注册Update
        public bool         isPersistent    { get; protected set; }         // true: 常驻内存，不会被销毁; false: LRU管理
    }

    public enum UIPanelType
    {
        Main,
        Login,
        Setting,
        Guild,
    }
}
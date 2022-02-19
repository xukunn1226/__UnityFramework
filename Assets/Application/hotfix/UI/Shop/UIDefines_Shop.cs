using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Logic
{
    public class UIDefines_Shop : UIDefines
    {
        public override string      id             { get; protected set; }  = UIPanelID.Shop;
        public override string      parentId       { get; protected set; }
        public override string      layer          { get; protected set; }  = UILayer.Fullscreen;
        public override string      assetPath      { get; protected set; }  = "assets/res/ui/prefabs/shop.prefab";
        public override EHideMode   hideMode       { get; protected set; }  = EHideMode.DisableCanvas;
        public override bool        isPersistent   { get; protected set; }  = false;        
        public override Type        typeOfPanel    { get; protected set; }  = typeof(UIPanel_Shop);
    }

    public partial class UIPanelID
    {
        static public string        Shop            { get; private set; }   = "Shop";
    }
}
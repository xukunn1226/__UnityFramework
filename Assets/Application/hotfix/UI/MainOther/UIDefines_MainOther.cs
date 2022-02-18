using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Logic
{
    public class UIDefines_MainOther : UIDefines
    {
        public override string      id             { get; protected set; }  = UIPanelID.MainOther;
        public override string      parentId       { get; protected set; }  = UIPanelID.Main;
        public override string      layer          { get; protected set; }  = UILayer.Windowed;
        public override string      assetPath      { get; protected set; }  = "assets/res/ui/prefabs/mainother.prefab";
        public override EHideMode   hideMode       { get; protected set; }  = EHideMode.DisableCanvas;
        public override bool        isPersistent   { get; protected set; }  = true;        
        public override Type        typeOfPanel    { get; protected set; }  = typeof(UIPanel_MainOther);
    }

    public partial class UIPanelID
    {
        static public string        MainOther       { get; private set; }   = "MainOther";
    }
}
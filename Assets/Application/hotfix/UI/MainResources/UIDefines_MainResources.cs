using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Logic
{
    public class UIDefines_MainResources : UIDefines
    {
        public override string      id              { get; protected set; } = UIPanelID.MainResources;
        public override string      parentId        { get; protected set; } = UIPanelID.Main;
        public override string      layer           { get; protected set; } = UILayer.Windowed;
        public override string      assetPath       { get; protected set; } = "assets/res/ui/prefabs/mainresources.prefab";
        public override EHideMode   hideMode        { get; protected set; } = EHideMode.DisableCanvas;
        public override bool        isPersistent    { get; protected set; } = true;
        public override Type        typeOfPanel     { get; protected set; } = typeof(UIPanel_MainResources);
    }

    public partial class UIPanelID
    {
        static public string        MainResources   { get; private set; }   = "MainResources";
    }
}
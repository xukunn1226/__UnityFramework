using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Logic
{
    public class UILoginDefines : UIDefines
    {
        public override string      id              { get; protected set; } = UIPanelID.Login;
        public override string      layer           { get; protected set; } = UILayer.Fullscreen;
        public override string      assetPath       { get; protected set; } = "assets/res/ui/prefabs/login.prefab";
        public override EHideMode   hideMode        { get; protected set; } = EHideMode.SetActive;
        public override bool        isPersistent    { get; protected set; } = false;
        public override Type        typeOfPanel     { get; protected set; } = typeof(UILoginPanel);
    }

    public partial class UIPanelID
    {
        static public string        Login           { get; private set; }   = "Login";
    }
}

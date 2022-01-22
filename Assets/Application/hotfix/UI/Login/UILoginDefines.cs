using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application.Logic;

namespace Application.Logic
{
    public class UILoginDefines : UIDefines
    {
        public override string   id             { get; protected set; } = UIPanelID.Login;
        public override string   layer          { get; protected set; } = UILayer.Windowed;
        public override string   assetPath      { get; protected set; } = "assets/res/ui/prefabs/login.prefab";
    }

    public partial class UIPanelID
    {
        static public string    Login           { get; private set; }   = "Login";
    }
}

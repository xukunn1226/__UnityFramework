using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{
    public class UIMainDefines : UIDefines
    {
        public override string  id             { get; protected set; } = UIPanelID.Main;
        public override string  layer          { get; protected set; } = UILayer.Main;
        public override string  assetPath      { get; protected set; } = "assets/res/ui/prefabs/main.prefab";
    }

    public partial class UIPanelID
    {
        static public string    Main            { get; private set; }   = "Main";
    }
}
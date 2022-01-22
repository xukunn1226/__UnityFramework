using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application.Logic;

namespace Application.Logic
{
    public class UILoginDefines : UIDefines
    {
        public override int      id             { get; protected set; } = UIPanelID.Login;
        public override string   layer          { get; protected set; } = UILayer.Windowed;
        public override string   path           { get; protected set; } = "";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class BuildParametersContext : IContextObject
    {
        public GameBuilderSetting gameBuilderSetting { get; private set; }

        public BuildParametersContext(GameBuilderSetting gameBuilderSetting)
        {
            this.gameBuilderSetting = gameBuilderSetting;
        }
    }
}
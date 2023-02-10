using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build.Pipeline.Interfaces;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class BuildResultContext : IContextObject
    {
        public IBundleBuildResults Results;
    }
}
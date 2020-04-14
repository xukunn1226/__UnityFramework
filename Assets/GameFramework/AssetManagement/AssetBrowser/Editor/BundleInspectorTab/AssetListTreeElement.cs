using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UITreeView;

namespace AssetManagement.AssetBrowser
{
    internal class AssetListTreeElement : TreeElement
    {
        public AssetFileInfo assetInfo;

        public AssetListTreeElement(string name, int depth, int id) : base(name, depth, id)
        { }
    }
}
using System.Collections.Generic;
using UITreeView;
using System;

namespace Framework.AssetBrowser
{
    internal class BundleListTreeElement : TreeElement
    {
        public BundleFileInfo               bundleFileInfo;

        public BundleListTreeElement(string name, int depth, int id) : base(name, depth, id)
        { }
    }
}
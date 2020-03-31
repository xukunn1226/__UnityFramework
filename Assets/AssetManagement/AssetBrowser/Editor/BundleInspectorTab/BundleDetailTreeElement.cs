using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UITreeView;

namespace AssetManagement.AssetBrowser
{
    public class BundleDetailTreeElement : TreeElement
    {
        public BundleDetailTreeElement(string name, int depth, int id) : base(name, depth, id)
        { }
    }
}
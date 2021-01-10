using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UITreeView;

namespace Framework.AssetManagement.AssetBrowser
{
    internal class ReferencedObjectTreeElement : TreeElement
    {
        public ReferencedObjectInfo referencedObjectInfo { get; set; }

        public ReferencedObjectTreeElement(string name, int depth, int id) : base(name, depth, id)
        { }
    }
}


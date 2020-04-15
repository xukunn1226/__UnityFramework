using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UITreeView;

namespace Framework.AssetBrowser
{
    internal class ReferencedObjectTreeElement : TreeElement
    {
        public ReferencedObjectInfo referencedObjectInfo { get; set; }

        public ReferencedObjectTreeElement(string name, int depth, int id) : base(name, depth, id)
        { }
    }
}


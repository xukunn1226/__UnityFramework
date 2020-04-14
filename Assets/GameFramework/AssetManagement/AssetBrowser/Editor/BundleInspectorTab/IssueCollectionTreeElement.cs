using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UITreeView;

namespace AssetManagement.AssetBrowser
{
    internal class IssueCollectionTreeElement : TreeElement
    {
        public IssueCollection issueInfo { get; set; }

        public IssueCollectionTreeElement(string name, int depth, int id) : base(name, depth, id)
        { }
    }
}


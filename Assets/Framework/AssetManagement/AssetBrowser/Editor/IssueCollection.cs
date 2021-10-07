using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetBrowser
{
    internal class IssueCollection
    {
        static public Dictionary<int, IssueCollection> collections = new Dictionary<int, IssueCollection>();

        public string               name                { get; private set; }           // issue name

        public string               assetPath           { get; private set; }           // issue assetPath description

        public Type                 type                { get; private set; }

        public bool                 isBuiltIn           { get; private set; }

        public bool                 isExternal          { get; private set; }

        public int                  referencedCount     { get; private set; }

        public bool                 hasFixed            { get; set; }

        private List<AssetFileInfo> m_AssetCollection   = new List<AssetFileInfo>();    // asset collection has similar problems

        public List<AssetFileInfo>  assetCollection
        {
            get
            {
                return m_AssetCollection;
            }
        }

        public IssueCollection(string name, string assetPath)
        {
            this.name = name;
            this.assetPath = assetPath;
            this.referencedCount = 1;
        }

        static public void Clear()
        {
            collections.Clear();
        }

        static public void AddIssue(ReferencedObjectInfo referencedObjectInfo, AssetFileInfo assetFileInfo)
        {
            if (referencedObjectInfo == null || assetFileInfo == null)
                throw new System.NullReferenceException("referencedObjectInfo == null || assetFileInfo == null");
                        
            int hashCode = string.Format(@"{0}/{1}/{2}", referencedObjectInfo.name, referencedObjectInfo.type.Name, referencedObjectInfo.assetPath).GetHashCode();
            IssueCollection collection;
            if(collections.TryGetValue(hashCode, out collection))
            {
                ++collection.referencedCount;
                collection.m_AssetCollection.Add(assetFileInfo);
            }
            else
            {
                collection = new IssueCollection(referencedObjectInfo.name, referencedObjectInfo.assetPath);
                collection.type = referencedObjectInfo.type;
                collection.isBuiltIn = referencedObjectInfo.isBuiltIn;
                collection.isExternal = referencedObjectInfo.isExternal;
                collection.m_AssetCollection.Add(assetFileInfo);

                collections.Add(hashCode, collection);
            }
        }
    }
}
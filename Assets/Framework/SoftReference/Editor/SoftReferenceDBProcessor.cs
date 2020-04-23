using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.Core.Editor
{
    public class SoftReferenceDBProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for (int i = 0; i < importedAssets.Length; ++i)
            {
                UpdateDB(importedAssets[i]);
            }

            for (int i = 0; i < deletedAssets.Length; ++i)
            {
                UpdateDB(deletedAssets[i]);
            }

            for (int i = 0; i < movedAssets.Length; ++i)
            {
                UpdateDB(movedAssets[i]);
            }

            for (int i = 0; i < movedFromAssetPaths.Length; ++i)
            {
                UpdateDB(movedFromAssetPaths[i]);
            }
        }

        static private void UpdateDB(string assetPath)
        {

        }
    }

    [System.Serializable]
    public class SoftReferenceInfo
    {
        public string m_GUID;

        public class UserInfo
        {
            public string   m_GUID;
            public long     m_FileID;
        }
        public List<UserInfo> m_UserInfoList = new List<UserInfo>();
    }

    [InitializeOnLoad]
    public class SoftReferencesDB
    {
        static SoftReferencesDB()
        {

        }
    }
}
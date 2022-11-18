using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

namespace Framework.AssetManagement.AssetChecker
{
    public class AssetProcessor_AssetMissingComponent : IAssetProcessor
    {
        private string errorInfo;
        public string DoProcess(string assetPath)
        {
            errorInfo = "";
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (go != null)
            {
                CheckGameObjectMissingComponentRecursively(
                    rootgo: assetPath,
                    toCheckGo: go
                );

                if (!string.IsNullOrEmpty(errorInfo))
                    return errorInfo;
            }
            else
            {
                return $"{assetPath} is not a GameObject";
            }
            return null;
        }

        public void CheckGameObjectMissingComponentRecursively(string rootgo, GameObject toCheckGo)
        {
            var componets = toCheckGo.GetComponents<Component>();
            foreach (var component in componets)
            {
                if (component == null)
                {
                    errorInfo += $"Missing Component <{toCheckGo.name}> finded in Prabab: <{rootgo}> \n";
                }
            }

            foreach (Transform child in toCheckGo.transform)
            {
                CheckGameObjectMissingComponentRecursively(rootgo, child.gameObject);
            }
        }
    }
}
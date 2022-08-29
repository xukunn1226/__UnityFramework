using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Framework.AssetManagement.AssetTreeView;

namespace Framework.AssetManagement.AssetProcess
{
    // 改名Prefix 处理器
    [Serializable]
    public class FixPrefixProcessor : ProcessorBaseData
    {
        public static string DisplayName = "前缀处理器";
        public string NewPrefix = "";
        public static string parent= UnityEngine.Application.dataPath;
        public override void OnProcessorApply(Category category)
        {
            //path
            parent = UnityEngine.Application.dataPath;
            var parentSplits = parent.Split("/");
            parent = ""; 
            for(int i = 0;i < parentSplits.Length-1; i++)
            {
                parent+= ("/" + parentSplits[i]);
            }

            List<string> paths = category.Rules[ruleIndex].Paths;
            List<FilterMode> modes = category.Rules[ruleIndex].Processors[processorIndex].filterModes;
            List<bool> includeSubFold = category.Rules[ruleIndex].includeSubFold;

            List<string> matchFilePaths = AssetFileUtility.GetMatchFiles(paths, modes,includeSubFold);
            EditorUtility.ClearProgressBar();
            int value = 0;
            foreach(string path in matchFilePaths)
            {
                //Debug.Log($"prefix:{path}");
                string assetName = Path.GetFileName(path);
                float progress = value/matchFilePaths.Count;
                EditorUtility.DisplayProgressBar("rename",$"reName:{assetName}",progress);
                value++;
                FixPrefix(path, assetName, NewPrefix);

            }

            EditorUtility.ClearProgressBar();
        }

        public string FixPrefix(string AssetPath,string AssetName, string newPrefix)
        {
            string preName = AssetName;
            int offset = 0;
            List<string> nameClips = new List<string>(preName.Split("_"));
            for(int i = 0; i < nameClips.Count; i++)
            {
                if( nameClips[i] == "FX" ||
                    nameClips[i] == "fx" ||
                    nameClips[i] == "Fx" ||
                    nameClips[i] == "Mat" ||
                    nameClips[i] == "M" ||
                    nameClips[i] == "model" ||
                    nameClips[i] == "P" ||
                    nameClips[i] == "Pb" ||
                    nameClips[i] == "pB" ||
                    nameClips[i] == "Rig" ||
                    nameClips[i] == "An" ||
                    nameClips[i] == "G" ||
                    nameClips[i] == "T" 
                    )
                {
                    offset++;
                }
                else
                {
                    break;
                }
            }

            List<string> assetPeopleSetedName = nameClips.GetRange(offset, nameClips.Count - offset);
            string newName = newPrefix;
            foreach (string clip in assetPeopleSetedName)
            {
                newName += $"_{clip}";
            }
            if(newName != preName)
            {
                ////change
                //string path = Path.GetDirectoryName(preName);
                //string fullPath = parent +  path+ "/"  + AssetPath;
                //string NewfullPath = parent +path+ "/"  + newName;
                ////
                //string metaPreName = preName + ".meta";
                //string metaNewName = newName + ".meta";
                //string metaFullPath = parent + path+ "/"  + metaPreName;
                //string NewmetaFullPath = parent + path+ "/"  + metaNewName;

                //
                ////
                //try
                //{
                //    if(File.Exists(fullPath))
                //    {

                //    }
                //    File.Move(fullPath, NewfullPath);
                //    File.Move(metaFullPath, NewmetaFullPath);
                //}
                //catch{
                //}
                //Debug.Log($"Path:{fullPath},meta:{metaFullPath}");
                AssetDatabase.RenameAsset(AssetPath, newName);

            }
            return newName;
        }


        public override void ProceeosrDraw(Rect cellRect, TreeViewItem<ProcessorTreeViewItem> item, int column)
        {
            var labelRect = cellRect;
            labelRect.width /= 3;
            EditorGUI.LabelField(labelRect, "新前缀");
            FixPrefixProcessor fixPrefixProcessor = item.data.processor.data as FixPrefixProcessor;
            labelRect.x += (cellRect.width / 2);
            labelRect.width =cellRect.x+ cellRect.width - labelRect.x;
            fixPrefixProcessor.NewPrefix = EditorGUI.TextField(labelRect,fixPrefixProcessor.NewPrefix);
            if (string.IsNullOrEmpty(fixPrefixProcessor.NewPrefix))
            {
                cellRect.y += 25;
                cellRect.x += cellRect.width/2;
                GUIStyle notiflyStyle = new GUIStyle(GUI.skin.label);
                notiflyStyle.normal.textColor = Color.red;
                EditorGUI.LabelField(cellRect, "前缀字段为空！", notiflyStyle);
            }

        }
    }
}

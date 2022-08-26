using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Framework.Core;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Framework.AssetManagement.AssetPackageEditor.Editor
{
    public enum AssetPackageBuildBundleType
    {
        // no sub dir : 按当前文件夹
        ByDirectionary = 0,
        // all in one : 按子文件夹
        ByAllInOne = 1,
        // segmentation : 按各文件
        ByFile = 2,
        // follow Parent setting : 按父目录
        ByFollowParent = 3,
    }

    [System.Serializable]
    public class AssetPackageSettingItem
    {
        // dir:  Asset/.../
        // file: Asset/.../...
        public string path;
        // defalut packageID is Base
        public string packageID;
        public AssetPackageBuildBundleType buildBundleType;

        public AssetPackageSettingItem(string path)
        {
            this.path = path;
            packageID = "base";
            buildBundleType = AssetPackageBuildBundleType.ByDirectionary;
        }
        public AssetPackageSettingItem()
        {
            path = "";
            packageID = "base";
            buildBundleType = AssetPackageBuildBundleType.ByDirectionary;
        }
    }

    public class AssetPackageEditorSetting : ScriptableObject
    {
        public List<AssetPackageSettingItem> editorAssetPackageSettingItems;
    }

    public class PackageHistoryItem
    {
        public string packageID;
        public string newPackageID;
        public string type;
        public PackageHistoryItem(string packageID, string newPackageID, string type)
        {
            this.packageID = packageID;
            this.newPackageID = newPackageID;
            this.type = type;
        }

        public override string ToString()
        {
            if (type == "add")
            {
                return $"add PakageID: {packageID} \n";
            }

            if (type == "remove")
            {
                return $"remove PakageID: {packageID} \n";
            }

            return $"replace ID: {packageID} to {newPackageID} \n";
        }
    }

    public class PackageIDDefinesReorderableList
    {
        ReorderableList reorderableList;
        private List<string> packageIDS;
        private List<PackageHistoryItem> itemsChangeHistory = new List<PackageHistoryItem>();
        public Action<string, string> onItemReplace;
        public Action<string> onItemAdded;
        public Action<string> onItemDeleted;


        public void StripEditOperate()
        {
            List<PackageHistoryItem> newitemsChangeHistory = new List<PackageHistoryItem>();
            
            for(int i = 0 ;i<itemsChangeHistory.Count; i++)
            {
                // start strip
                var startItem = itemsChangeHistory[i];
                string startPackageID = startItem.packageID;
                string endPackageID = startItem.newPackageID;
                if (startItem.type == "edit")
                {
                    if (i == itemsChangeHistory.Count - 1)
                    {
                        newitemsChangeHistory.Add(startItem);
                        break;
                    }
                    
                    for (int a = i+1; a < itemsChangeHistory.Count; a++)
                    {
                        var currentItem = itemsChangeHistory[a];
                        if (currentItem.type != "edit")
                        {
                            PackageHistoryItem newItem = new PackageHistoryItem(startPackageID, endPackageID, "edit");
                            newitemsChangeHistory.Add(newItem);
                            i = a - 1;
                            break;
                        }
                        else
                        {
                            if (endPackageID == currentItem.packageID)
                            {
                                endPackageID = currentItem.newPackageID;
                                if (a == itemsChangeHistory.Count - 1)
                                {
                                     PackageHistoryItem newItem = new PackageHistoryItem(startPackageID, endPackageID, "edit");
                                     newitemsChangeHistory.Add(newItem);
                                     i = a;
                                     break;                           
                                }
                            }
                            else
                            {
                                PackageHistoryItem newItem =
                                    new PackageHistoryItem(startPackageID, endPackageID, "edit");
                                newitemsChangeHistory.Add(newItem);
                                i = a-1;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    newitemsChangeHistory.Add(startItem);
                }
            }

            itemsChangeHistory = newitemsChangeHistory;
        }
        public string DebugGetOperateInfo()
        {
            string info = "operateInfo \n";
            info += "[debug] \n";
            foreach (var item in itemsChangeHistory)
            {
                info += item.ToString();

            }
            return info;
        }

        public string SavePacakgeIDData()
        {
            StripEditOperate();
            
            string info = DebugGetOperateInfo();
            PackageIDDefines.Instance.UpdatePackageIDFromList(packageIDS);
            
            foreach (var item in itemsChangeHistory)
            {
                if (item.type == "edit")
                {
                    onItemReplace?.Invoke(item.packageID,item.newPackageID);
                }else if (item.type == "add")
                {
                    onItemAdded?.Invoke(item.packageID);
                }else if (item.type == "remove")
                {
                    onItemDeleted?.Invoke(item.packageID);
                }
            }
            itemsChangeHistory.Clear();
            return info;
        }

        public bool CanSavePackageData()
        {
            foreach (var id in packageIDS)
            {
                if (string.IsNullOrEmpty(id))
                {
                    return false;   
                }
            }

            return NoDuplicate();
        }

        public bool NoDuplicate()
        {
            Dictionary<string, int> idCounts = new Dictionary<string, int>();
            foreach (var id in packageIDS)
            {
                idCounts.TryAdd(id, 0);
            }
            foreach (var id in packageIDS)
            {
                idCounts[id]++;
            }

            foreach (var idCount in idCounts)
            {
                if (idCount.Value > 1)
                {
                    return false;
                }
            }
            return true;           
        }

        public bool IsValueDuplicate(string value)
        {
            int count = 0;
            foreach (var id in packageIDS)
            {
                if (id == value)
                    count++;
            }

            if (count > 1)
            {
                return true;
            }

            return false;
        }
        public void DoList(Rect rect)
        {
            reorderableList.DoList(rect);
        }
        public PackageIDDefinesReorderableList()
        {
            packageIDS = new List<string>(PackageIDDefines.Instance.GetPackageList());
            reorderableList = new ReorderableList(packageIDS,typeof(string));
            reorderableList.drawElementCallback = ElementCallbackDelegate;
            reorderableList.onAddCallback =AddCallbackDelegate ;
            reorderableList.onRemoveCallback =RemoveCallbackDelegate ;
            reorderableList.onCanRemoveCallback =CanRemoveCallbackDelegate ;
            reorderableList.draggable = false;
            reorderableList.onCanAddCallback = CanAddCallbackDelegate;
        }

        void ElementCallbackDelegate(Rect rect, int index, bool isActive, bool isFocused)
        {
            var itemString = packageIDS[index];
            Rect prefixRect = rect;
            prefixRect.width = 70;
            prefixRect.y += 5;
            prefixRect.height -= 5;
            EditorGUI.LabelField(prefixRect, new GUIContent("PackageID"));
            GUIStyle error =new GUIStyle(GUI.skin.label); 
            error.normal.textColor = Color.red;
            error.fontSize -= 2;
            //
            prefixRect.x += 65;
            prefixRect.width = 65;

            string errorStr = "";
            if(string.IsNullOrEmpty(itemString))
            {
                errorStr = "不能为空!";
            }
            else if(IsValueDuplicate(itemString))
            {
                errorStr = "不能重复!";
            }
            EditorGUI.LabelField(new Rect(prefixRect.x,prefixRect.y,prefixRect.width,prefixRect.height),
                errorStr,error
                );           
            
            prefixRect.x += 50 ;
            prefixRect.width = rect.width - prefixRect.x;
            if (index == 0 || index == 1)
            {
                GUI.enabled = false;
            }
            
            packageIDS[index] = GUI.TextField(prefixRect,itemString);
            
            if (packageIDS[index] != itemString)
            {
                if (string.IsNullOrEmpty(itemString))
                    itemString = "NAN";
                
                itemsChangeHistory.Add(new PackageHistoryItem(
                    itemString,
                    packageIDS[index],
                    "edit")
                );
            }

            GUI.enabled = true;
            
        }

        void AddCallbackDelegate(ReorderableList list)
        {
            packageIDS.Add("");
            
            // history
            itemsChangeHistory.Add(new PackageHistoryItem("NAN","","add"));
        }

        void RemoveCallbackDelegate(ReorderableList list)
        {
            int deletedIndex = list.index;
            if(list.selectedIndices.Count > 0)
            {
                deletedIndex = list.selectedIndices[0];
            }

            if (deletedIndex == 0 || deletedIndex == 1) return;
            //bool hasRemove = PackageIDDefines.Instance.RemovePackageIDFromIndex(deletedIndex);
            //if (hasRemove)
            //{
            //}
            var deletedStr = packageIDS[deletedIndex];
            
            packageIDS.RemoveAt(deletedIndex);
            
            itemsChangeHistory.Add(new PackageHistoryItem(deletedStr,"","remove"));
        }
        bool CanAddCallbackDelegate(ReorderableList list)
        {
            return CanSavePackageData();
        }

        bool CanRemoveCallbackDelegate(ReorderableList list)
        {
            int deletedIndex = list.index;
            if(list.selectedIndices.Count > 0)
            {
                deletedIndex = list.selectedIndices[0];
            }

            if (deletedIndex == 0 || deletedIndex == 1)
            {
                return false;
            }
            
            return true;
        }
    }
    
    public class PackageIDDefines : ScriptableObject
    {
        //ID:0 
        public string packageDefault = "base";
        //ID:1 
        public string packageExtra = "extra";
        //ID: -1
        public string packageError = "Not Found";

        private static string defaultPath = "Assets/Framework/AssetManagement/AssetPackageEditor/Editor/Setting/PackageIDDefines.asset";
        private static PackageIDDefines mInstance;

        public string[] GetPackageList()
        {
            List<string> packageOptions = new List<string>();
            packageOptions.Add(packageDefault);
            packageOptions.Add(packageExtra);
            packageOptions.AddRange(packageIDList);
            return packageOptions.ToArray();
        }

        public static PackageIDDefines Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = AssetDatabase.LoadAssetAtPath<PackageIDDefines>(defaultPath);
                    if (mInstance == null)
                    {
                        mInstance = ScriptableObject.CreateInstance<PackageIDDefines>();
                        AssetDatabase.CreateAsset(mInstance,defaultPath);
                        AssetDatabase.SaveAssets();
                    }
                }
                
                return mInstance;
            }
        }
        public List<string> packageIDList = new List<string>();

        public bool TryAddPakageId(string packageID)
        {
            if (packageID == packageDefault)
            {
                return false;
            }
            if (packageID == packageExtra)
            {
                return false;
            }

            if (!packageIDList.Contains(packageID))
            {
                packageIDList.Add(packageID);
                return true;
            }

            return false;
        }

        public void UpdatePackageIDFromList(List<string> list)
        {
            packageIDList.Clear();
            foreach (var item in list)
            {
                TryAddPakageId(item);
            }
            
        }

        public bool RemovePackageIDFromIndex(int index)
        {
            if (index == 0 ||
                index == 1 ||
                index == -1
               )
            {
                return false;
            }
            
            int realIndex = index -2;
            if (realIndex < packageIDList.Count)
            {
                packageIDList.RemoveAt(realIndex);
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetPackageIDIndexFromPackageID(string packageID)
        {
            if (packageID == packageDefault)
            {
                return 0;
            }
            
            if (packageID == packageExtra)
            {
                return 1;
            }
            
            if (packageIDList.Contains(packageID))
            {
                return packageIDList.FindIndex(s => s == packageID) + 2;
            }
            return -1;
        }

        public void UpdateCustomPackageID(int allIndex,string content)
        {
            if(allIndex == 0 || allIndex == 1 || allIndex == -1)
                return;
            allIndex -=2;
            if (allIndex < packageIDList.Count)
            {
                packageIDList[allIndex] = content;
            }
        }

        public string GetPackageIDByIndex(int index)
        {
            if (index == -1)
            {
                return packageError;
            }

            if (index == 0)
            {
                return packageDefault;
            }

            if (index == 1)
            {
                return packageExtra;
            }

            return packageIDList[index-2];
            
        }

        public void SavePackageID()
        {
            EditorUtility.SetDirty(mInstance);
            AssetDatabase.SaveAssets();
        }
    }
}
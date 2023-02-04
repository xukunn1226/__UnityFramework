//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using Sirenix.OdinInspector.Editor;
//using Sirenix.Utilities.Editor;
//using Sirenix.Utilities;
//using Sirenix.OdinInspector;
//using System;
//using System.Reflection;

//namespace Framework.AssetManagement.AssetEditorWindow
//{
//    public class AssetBundleViewer : OdinEditorWindow
//    {
//        public List<BuildBundleInfo> BuildBundleInfos;

//        public static void Open(BuildMapContext context)
//        {
//            var window = GetWindow<AssetBundleViewer>();
//            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
//            window.SetContext(context);
//        }

//        private void SetContext(BuildMapContext context)
//        {
//            BuildBundleInfos = context.BuildBundleInfos;
//        }

//        protected override void OnBeginDrawEditors()
//        {
//            base.OnBeginDrawEditors();
//            SirenixEditorGUI.BeginHorizontalToolbar();
//            {
//                GUILayout.FlexibleSpace();
//                if (SirenixEditorGUI.ToolbarButton("检查"))
//                {
                    
//                }
//                if (SirenixEditorGUI.ToolbarButton("模拟打包"))
//                {
                    
//                }
//            }
//            SirenixEditorGUI.EndHorizontalToolbar();
//        }
//    }

//    public class BuildBundleInfoClassAttributeProcessor : OdinAttributeProcessor<BuildBundleInfo>
//    {
//        public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
//        {
//            return typeof(IList).IsAssignableFrom(parentProperty.ParentType);
//        }

//        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
//        {
//            attributes.Add(new InlinePropertyAttribute());
//            attributes.Add(new HideReferenceObjectPickerAttribute());
//        }

//        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
//        {
//            //attributes.Add(new HideLabelAttribute());
//            //attributes.Add(new BoxGroupAttribute("Box", showLabel: false));
            
//            attributes.Add(new ReadOnlyAttribute());

//            if(member.Name == "BundleName")
//            {
//                attributes.Add(new ShowInInspectorAttribute());
//                attributes.Add(new PropertyOrderAttribute(0));
//            }
//            else if(member.Name == "BuildinAssets")
//            {
//                attributes.Add(new ShowInInspectorAttribute());
//                attributes.Add(new PropertyOrderAttribute(1));
//            }
//        }
//    }

//    public class BuildAssetInfoClassAttributeProcessor : OdinAttributeProcessor<BuildAssetInfo>
//    {
//        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
//        {
//            attributes.Add(new ReadOnlyAttribute());
//            attributes.Add(new InlinePropertyAttribute());
//            attributes.Add(new HideReferenceObjectPickerAttribute());
//        }

//        // BuildAssetInfo创建了CustomDrawer，这里添加attribute将失效
//        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
//        {
//            //attributes.Add(new HideLabelAttribute());
//            //attributes.Add(new BoxGroupAttribute("Box", showLabel: false));

//            attributes.Add(new ReadOnlyAttribute());

//            if (member.Name == "AssetPath")
//            {
//                attributes.Add(new ShowInInspectorAttribute());
//                attributes.Add(new PropertyOrderAttribute(0));
//            }
//            else if (member.Name == "MainBundleName")
//            {
//                attributes.Add(new ShowInInspectorAttribute());
//                attributes.Add(new PropertyOrderAttribute(1));
//            }
//            else if (member.Name == "AllDependBundleNames")
//            {
//                attributes.Add(new ShowInInspectorAttribute());
//                attributes.Add(new PropertyOrderAttribute(2));
//            }
//        }
//    }

//    //public class BuildAssetInfoDrawer : OdinValueDrawer<BuildAssetInfo>
//    //{
//    //    //protected override bool CanDrawValueProperty(InspectorProperty property)
//    //    //{
//    //    //    if(property.Index == 1)
//    //    //        return false;
//    //    //    return true;
//    //    //}

//    //    protected override void DrawPropertyLayout(GUIContent label)
//    //    {
//    //        BuildAssetInfo value = ValueEntry.SmartValue;

//    //        var rect = EditorGUILayout.GetControlRect();
//    //        if (label != null)
//    //        {
//    //            rect = EditorGUI.PrefixLabel(rect, label);
//    //        }

//    //        EditorGUI.LabelField(rect, "AssetPath", string.Format($"{value.AssetPath} [{value.MainBundleName}]"));

//    //        //rect = EditorGUILayout.GetControlRect();
//    //        //EditorGUI.BeginFoldoutHeaderGroup(rect, true, $"依赖的资源包: {value.AllDependBundleNames.Count}");
//    //        //foreach (var dependBundle in value.AllDependBundleNames)
//    //        //{
//    //        //    EditorGUI.LabelField(rect, "DependBundle", $"{dependBundle}");
//    //        //}
//    //        //EditorGUI.EndFoldoutHeaderGroup();
//    //    }
//    //}

//    //public class BuildBundleInfoDrawer : OdinValueDrawer<BuildBundleInfo>
//    //{
//    //    protected override void DrawPropertyLayout(GUIContent label)
//    //    {
//    //        BuildBundleInfo value = ValueEntry.SmartValue;

//    //        var rect = EditorGUILayout.GetControlRect();
//    //        if (label != null)
//    //        {
//    //            rect = EditorGUI.PrefixLabel(rect, label);
//    //        }

//    //        //EditorGUI.LabelField(rect, "AssetPath", string.Format($"{value.AssetPath} [{value.MainBundleName}]"));

//    //        //rect = EditorGUILayout.GetControlRect();
//    //        //EditorGUI.BeginFoldoutHeaderGroup(rect, true, $"依赖的资源包: {value.AllDependBundleNames.Count}");
//    //        //foreach (var dependBundle in value.AllDependBundleNames)
//    //        //{
//    //        //    EditorGUI.LabelField(rect, "DependBundle", $"{dependBundle}");
//    //        //}
//    //        //EditorGUI.EndFoldoutHeaderGroup();
//    //    }
//    //}
//}
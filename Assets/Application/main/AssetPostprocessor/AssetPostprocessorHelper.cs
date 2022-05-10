using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;
using System;

namespace Application.Editor
{
    static public class AssetPostprocessorHelper
    {
        static public void GetTextureOriginalSize(TextureImporter ti, out int width, out int height)
        {
            if (ti == null)
            {
                width = 0;
                height = 0;
                return;
            }

            object[] args = new object[2] { 0, 0 };
            MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(ti, args);

            width = (int)args[0];
            height = (int)args[1];
        }
        
        // 优化anim数据精度
        static public void OptimizeAnim2(AnimationClip clip)
        {
            if (clip == null)
                return;

            SerializedObject serializedObject = new SerializedObject(clip);
            SerializedProperty curvesProperty = serializedObject.FindProperty("m_FloatCurves");
            OptCurves(curvesProperty);
            SerializedProperty rotationProperty = serializedObject.FindProperty("m_RotationCurves");
            OptCurves(rotationProperty);
            SerializedProperty positionProperty = serializedObject.FindProperty("m_PositionCurves");
            OptCurves(positionProperty);
            SerializedProperty scaleProperty = serializedObject.FindProperty("m_ScaleCurves");
            OptCurves(scaleProperty);

            serializedObject.ApplyModifiedProperties();
        }

        static private void OptCurves(SerializedProperty property)
        {
            if (property != null && property.isArray)
            {
                for (int i = 0; i < property.arraySize; ++i)
                {
                    SerializedProperty curveProperty = property.GetArrayElementAtIndex(i).FindPropertyRelative("curve");
                    SerializedProperty keyframeProperty = curveProperty.FindPropertyRelative("m_Curve");
                    if (keyframeProperty != null && keyframeProperty.isArray)
                    {
                        for (int j = 0; j < keyframeProperty.arraySize; ++j)
                        {
                            SerializedProperty kf = keyframeProperty.GetArrayElementAtIndex(j);

                            SerializedProperty time = kf.FindPropertyRelative("time");
                            OptValue(time);

                            SerializedProperty value = kf.FindPropertyRelative("value");
                            OptValue(value);

                            SerializedProperty inSlope = kf.FindPropertyRelative("inSlope");
                            OptValue(inSlope);

                            SerializedProperty outSlope = kf.FindPropertyRelative("outSlope");
                            OptValue(outSlope);
                        }
                    }
                }
            }
        }

        static private void OptValue(SerializedProperty target)
        {
            if (target.type == "float")
                target.floatValue = OptFloat(target.floatValue);
            else if (target.type == "Vector3")
                target.vector3Value = OptVector3(target.vector3Value);
            else if (target.type == "Vector4")
                target.vector4Value = OptVector4(target.vector4Value);
            else if (target.type == "Quaternion")
                target.quaternionValue = OptQuaternion(target.quaternionValue);
        }

        static private float OptFloat(float src)
        {
            return Mathf.Floor(src * 1000 + 0.5f) / 1000;
        }

        static private Quaternion OptQuaternion(Quaternion src)
        {
            return new Quaternion(OptFloat(src.x), OptFloat(src.y), OptFloat(src.z), OptFloat(src.w));
        }

        static private Vector4 OptVector4(Vector4 src)
        {
            return new Vector4(OptFloat(src.x), OptFloat(src.y), OptFloat(src.z), OptFloat(src.w));
        }

        static private Vector3 OptVector3(Vector3 src)
        {
            return new Vector3(OptFloat(src.x), OptFloat(src.y), OptFloat(src.z));
        }

        static public void SetClipProperty(ModelImporter mi)
        {
            SerializedObject serializedObject = new SerializedObject(mi);
            SerializedProperty clipsProperty = serializedObject.FindProperty("m_ClipAnimations");

            for (int i = 0; i < clipsProperty.arraySize; ++i)
            {
                SerializedProperty clipProperty = clipsProperty.GetArrayElementAtIndex(i);

                clipProperty.FindPropertyRelative("loopBlend").boolValue = false;
                clipProperty.FindPropertyRelative("loopBlendOrientation").boolValue = true;             // Root Transform Rotation -> Bake Into Pose
                clipProperty.FindPropertyRelative("loopBlendPositionY").boolValue = true;               // Root Transform Position(Y) -> Bake Into Pose
                //clipProperty.FindPropertyRelative("loopBlendPositionXZ").boolValue = true;              // Root Transform Position(XZ) -> Bake Into Pose
                clipProperty.FindPropertyRelative("keepOriginalOrientation").boolValue = true;          // Root Transform Rotation -> Based Upon
                clipProperty.FindPropertyRelative("keepOriginalPositionY").boolValue = true;            // Root Transform Position(Y) -> Based Upon
                clipProperty.FindPropertyRelative("keepOriginalPositionXZ").boolValue = true;          // Root Transform Position(XZ) -> Based Upon
            }
            serializedObject.ApplyModifiedProperties();
        }

        static public void OptimizeAnim(AnimationClip theAnimation/*, bool clearScaleCurve = false*/)
        {
            bool isDirty = false;
            try
            {
                // 新API创建curve，会有重复数据，暂时禁用
                // EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(theAnimation);
                // AnimationClipCurveData[] curves = new AnimationClipCurveData[curveBindings.Length];
                // for(int i = 0; i < curves.Length; ++i)
                // {
                //     curves[i] = new AnimationClipCurveData(curveBindings[i]);
                //     curves[i].curve = AnimationUtility.GetEditorCurve(theAnimation, curveBindings[i]);
                // }

#pragma warning disable 0618
                AnimationClipCurveData[] curves = null;
				curves = AnimationUtility.GetAllCurves(theAnimation);
#pragma warning restore 0618

                //浮点数精度压缩到f3
                Keyframe key;
                Keyframe[] keyFrames;

                // pass 1. 遍历是否有dirty数据
                for (int ii = 0; ii < curves.Length; ++ii)
                {
                    AnimationClipCurveData curveDate = curves[ii];
                    if (curveDate.curve == null || curveDate.curve.keys == null)
                    {
                        continue;
                    }
                    keyFrames = curveDate.curve.keys;
                    for (int i = 0; i < keyFrames.Length; i++)
                    {
                        key             = keyFrames[i];

                        {
                            float time = key.time;
                            isDirty |= OptFloat(ref time);
                            key.time = time;

                            float value = key.value;
                            isDirty |= OptFloat(ref value);
                            key.value = value;

                            float inTangent = key.inTangent;
                            isDirty |= OptFloat(ref inTangent);
                            key.inTangent = inTangent;

                            float outTangent = key.outTangent;
                            isDirty |= OptFloat(ref outTangent);
                            key.outTangent = outTangent;

                            float inWeight = key.inWeight;
                            isDirty |= OptFloat(ref inWeight);
                            key.inWeight = inWeight;

                            float outWeight = key.outWeight;
                            isDirty |= OptFloat(ref outWeight);
                            key.outWeight = outWeight;
                        }

                        keyFrames[i]    = key;
                    }
                }

                // pass 2. reconstruct curves
                if(isDirty)
                {
                    theAnimation.ClearCurves();
                    for (int ii = 0; ii < curves.Length; ++ii)
                    {
                        AnimationClipCurveData curveDate = curves[ii];
                        if (curveDate.curve == null || curveDate.curve.keys == null)
                        {
                            continue;
                        }
                        keyFrames = curveDate.curve.keys;
                        for (int i = 0; i < keyFrames.Length; i++)
                        {
                            key = keyFrames[i];

                            {
                                float time = key.time;
                                isDirty |= OptFloat(ref time);
                                key.time = time;

                                float value = key.value;
                                isDirty |= OptFloat(ref value);
                                key.value = value;

                                float inTangent = key.inTangent;
                                isDirty |= OptFloat(ref inTangent);
                                key.inTangent = inTangent;

                                float outTangent = key.outTangent;
                                isDirty |= OptFloat(ref outTangent);
                                key.outTangent = outTangent;

                                float inWeight = key.inWeight;
                                isDirty |= OptFloat(ref inWeight);
                                key.inWeight = inWeight;

                                float outWeight = key.outWeight;
                                isDirty |= OptFloat(ref outWeight);
                                key.outWeight = outWeight;
                            }

                            keyFrames[i] = key;
                        }

                        curveDate.curve.keys = keyFrames;
                        theAnimation.SetCurve(curveDate.path, curveDate.type, curveDate.propertyName, curveDate.curve);
                    }
                }

                //去除scale曲线
                // if(clearScaleCurve)
                // {
                //     // method 1. 删除所有scale，不做筛选
                //     foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(theAnimation))
                //     {
                //         string name = theCurveBinding.propertyName.ToLower();
                //         if (name.Contains("scale"))
                //         {
                //             AnimationUtility.SetEditorCurve(theAnimation, theCurveBinding, null);
                //         }
                //     }

                    // method 2. 删除限定scale
                    // string prevPath = null;
                    // int count = 0;
                    // EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(theAnimation);
                    // for(int i = 0; i < curves.Length; ++i)
                    // {
                    //     AnimationClipCurveData curveData = curves[i];
                    //     EditorCurveBinding curveBinding = curveBindings[i];
                    //     string name = curveBinding.propertyName.ToLower();
                    //     string path = curveBinding.path;
                    //     if(path != prevPath)
                    //     {
                    //         count = 0;
                    //         prevPath = path;
                    //     }

                    //     if(name.Contains("scale.x"))
                    //     {
                    //         keyFrames = curveData.curve.keys;
                    //         if((keyFrames.Length == 2 && Mathf.Approximately(keyFrames[0].value, 1) && Mathf.Approximately(keyFrames[1].value, 1))
                    //         || (keyFrames.Length == 1 && Mathf.Approximately(keyFrames[0].value, 1)))
                    //         {
                    //             count++;
                    //         }
                    //     }
                    //     else if(name.Contains("scale.y"))
                    //     {
                    //         keyFrames = curveData.curve.keys;
                    //         if((keyFrames.Length == 2 && Mathf.Approximately(keyFrames[0].value, 1) && Mathf.Approximately(keyFrames[1].value, 1))
                    //         || (keyFrames.Length == 1 && Mathf.Approximately(keyFrames[0].value, 1)))
                    //         {
                    //             count++;
                    //         }
                    //     }
                    //     else if(name.Contains("scale.z"))
                    //     {
                    //         keyFrames = curveData.curve.keys;
                    //         if((keyFrames.Length == 2 && Mathf.Approximately(keyFrames[0].value, 1) && Mathf.Approximately(keyFrames[1].value, 1))
                    //         || (keyFrames.Length == 1 && Mathf.Approximately(keyFrames[0].value, 1)))
                    //         {
                    //             count++;
                    //         }
                    //     }

                    //     if(count == 3)
                    //     {
                    //         AnimationUtility.SetEditorCurve(theAnimation, curveBindings[i-2], null);
                    //         AnimationUtility.SetEditorCurve(theAnimation, curveBindings[i-1], null);
                    //         AnimationUtility.SetEditorCurve(theAnimation, curveBindings[i], null);

                    //         isDirty |= true;
                    //     }
                    // }
                // }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("CompressAnimationClip Failed !!! animationClip : {0} error: {1}", theAnimation.name, e));
            }

            if(isDirty)
            {
                EditorUtility.SetDirty(theAnimation);
            }
        }

        static private bool OptFloat(ref float src)
        {
            float dst = Mathf.Floor(src * 1000 + 0.5f) / 1000;
            bool changed = dst != src;
            src = dst;
            return changed;
        }
        
        [MenuItem("Assets/美术资源工具/提取动画文件至同级目录", false, 996)]
        static private void ExtractAnimClipToSameDirectory()
        {
            if(Selection.assetGUIDs.Length == 0)
                return;

            UnityEditor.EditorUtility.DisplayProgressBar("Extract AnimClip", "", 0);
            for(int i = 0; i < Selection.assetGUIDs.Length; ++i)
            {
                string curAssetPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]);
                string dstDirectory = curAssetPath.Substring(0, curAssetPath.LastIndexOf("/"));
                ExtractAnimClip(curAssetPath, dstDirectory);
                UnityEditor.EditorUtility.DisplayProgressBar("Extract AnimClip", i + "/" + Selection.assetGUIDs.Length, i / (float)Selection.assetGUIDs.Length);
            }
            UnityEditor.EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/美术资源工具/提取动画文件至上级目录", false, 997)]
        static private void ExtractAnimClipToParentDirectory()
        {
            if(Selection.assetGUIDs.Length == 0)
                return;

            UnityEditor.EditorUtility.DisplayProgressBar("Extract AnimClip", "", 0);
            for(int i = 0; i < Selection.assetGUIDs.Length; ++i)
            {
                string curAssetPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]);
                string dstDirectory = curAssetPath.Substring(0, curAssetPath.LastIndexOf("/"));
                dstDirectory = dstDirectory.Substring(0, dstDirectory.LastIndexOf("/"));
                ExtractAnimClip(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]), dstDirectory);
                UnityEditor.EditorUtility.DisplayProgressBar("Extract AnimClip", i + "/" + Selection.assetGUIDs.Length, i / (float)Selection.assetGUIDs.Length);
            }
            UnityEditor.EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/美术资源工具/提取动画文件至指定目录...", false, 998)]
        static private void ExtractAnimClipToSpecialDirectory()
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            ExtractAnimWindow.Init(AssetDatabase.IsValidFolder(assetPath) ? assetPath : assetPath.Substring(0, assetPath.LastIndexOf("/")));
        }

        static public void DoExtractAnimClipBatch(string srcDirectory, string dstDirectory)
        {
            DirectoryInfo di = new DirectoryInfo(srcDirectory);
            FileInfo[] fis = di.GetFiles("*.fbx", SearchOption.TopDirectoryOnly);
            UnityEditor.EditorUtility.DisplayProgressBar("Extract AnimClip", "", 0);
            for(int i = 0; i < fis.Length; ++i)
            {
                string fbxAssetPath = Framework.Core.Utility.GetProjectPath(fis[i].FullName);
                ExtractAnimClip(fbxAssetPath, dstDirectory);
                UnityEditor.EditorUtility.DisplayProgressBar("Extract AnimClip", i + "/" + fis.Length, i / (float)fis.Length);
            }
            UnityEditor.EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 提取动画文件
        /// </summary>
        /// <param name="fbxAssetPath"></param>
        /// <param name="dstDirectory">null，表示提取至同层级目录</param>
        static public void ExtractAnimClip(string fbxAssetPath, string dstDirectory)
        {
            UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(fbxAssetPath);
            if (objs != null && objs.Length != 0)
            {
                foreach (UnityEngine.Object obj in objs)
                {
                    AnimationClip clip = obj as AnimationClip;
                    if(clip == null)
                        continue;
                    
                    //提取 *.anim
                    dstDirectory = string.IsNullOrEmpty(dstDirectory) ? fbxAssetPath.Substring(0, fbxAssetPath.LastIndexOf("/")) : dstDirectory.TrimEnd('/');
                    string filename = Path.GetFileNameWithoutExtension(fbxAssetPath) + ".anim"; 
                    string dstFilename = dstDirectory + "/" + filename;
                    {
                        UnityEngine.Object clone = UnityEngine.Object.Instantiate(clip);

                        // 优化精度
                        AssetPostprocessorHelper.OptimizeAnim(clone as AnimationClip);

                        AssetDatabase.CreateAsset(clone, dstFilename);
                        AssetDatabase.ImportAsset(dstFilename, ImportAssetOptions.ForceUpdate);
                        // AssetDatabase.Refresh();
                        Debug.Log($"提取动画完成：{dstFilename}", AssetDatabase.LoadAssetAtPath<AnimationClip>(dstFilename));
                    }
                }
            }
        }

        [MenuItem("Assets/美术资源工具/提取模型文件", false, 999)]
        static public void ExtractMesh()
        {
            if(Selection.assetGUIDs.Length == 0)
                return;

            UnityEditor.EditorUtility.DisplayProgressBar("Extract Mesh", "", 0);
            for(int i = 0; i < Selection.assetGUIDs.Length; ++i)
            {
                ExtractMesh(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]));
                UnityEditor.EditorUtility.DisplayProgressBar("Extract Mesh", i + "/" + Selection.assetGUIDs.Length, i / (float)Selection.assetGUIDs.Length);
            }
            UnityEditor.EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        static public void ExtractMesh(string assetPath)
        {
            UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            List<Mesh> meshList = new List<Mesh>();
            if (objs != null && objs.Length != 0)
            {
                foreach (UnityEngine.Object obj in objs)
                {
                    Mesh mesh = obj as Mesh;
                    if(mesh == null)
                        continue;
                    meshList.Add(mesh);
                }
                
                for(int i = 0; i < meshList.Count; ++i)
                {
                    //提取Mesh
                    string filename = Path.GetFileNameWithoutExtension(assetPath) + "_" + i.ToString();
                    string filePath = assetPath.Substring(0, assetPath.LastIndexOf("/") + 1) + filename + ".asset";
                    {
                        UnityEngine.Object clone = UnityEngine.Object.Instantiate(meshList[i]);

                        AssetDatabase.CreateAsset(clone, filePath);
                        AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
                        AssetDatabase.Refresh();
                        Debug.Log($"提取模型完成：{filePath}");
                    }
                }
            }
        }

        [MenuItem("Assets/美术检查工具/CheckFailParticleCulling", false)]
        static public void CheckFailParticleCulling()
        {
            Assembly ass = typeof(UnityEditor.Editor).Assembly;
            Type t = ass.GetType("UnityEditor.ParticleSystemUI");
            MethodInfo init = t.GetMethod("Init");
            FieldInfo m_SupportsCullingText = t.GetField("m_SupportsCullingText", BindingFlags.Instance | BindingFlags.NonPublic);
            object particleSystemUI = ass.CreateInstance("UnityEditor.ParticleSystemUI");

            foreach (var obj in Selection.objects)
            {
                if (obj is GameObject)
                {
                    ParticleSystem[] particles = (obj as GameObject).GetComponentsInChildren<ParticleSystem>(true);
                    string result = "";
                    foreach (ParticleSystem particle in particles)
                    {
                        init.Invoke(particleSystemUI, new object[] { null, new ParticleSystem[] { particle } });
                        string subResult = m_SupportsCullingText.GetValue(particleSystemUI) as string;
                        if (subResult != null)
                        {
                            result += particle.name + " (" + subResult.Replace("\n","") + ")\n";
                        }
                    }
                    if (result != "")
                    {
                        Debug.Log(AssetDatabase.GetAssetPath(obj) + "\n" + result,obj);
                    }
                }
            }
        }
    }
}
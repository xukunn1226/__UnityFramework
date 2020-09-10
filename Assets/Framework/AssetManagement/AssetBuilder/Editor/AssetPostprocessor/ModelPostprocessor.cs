using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Framework.AssetManagement.AssetBuilder
{
    public class ModelPostprocessor : AssetPostprocessor
    {
//        void OnPreprocessModel()
//        {
//            ModelImporter modelImporter = assetImporter as ModelImporter;
//#if UNITY_2019_1_OR_NEWER
//            modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
//#else
//            modelImporter.importMaterials = false;
//#endif
//        }

       void OnPostprocessModel(GameObject root)
       {
//            ModelImporter modelImporter = assetImporter as ModelImporter;
// #if UNITY_2019_1_OR_NEWER
//            if (modelImporter.materialImportMode == ModelImporterMaterialImportMode.None)
// #else
//            if (!modelImporter.importMaterials)
// #endif
//            {
//                 // 清空默认的内置引用资源
//                 Renderer[] rdrs = root.GetComponentsInChildren<Renderer>();
//                 for (int i = 0; i < rdrs.Length; ++i)
//                 {
//                     Material[] mats = new Material[rdrs[i].sharedMaterials.Length];
//                     rdrs[i].sharedMaterials = mats;
//                 }
//             }
            
            // OptimizeAnim(assetImporter.assetPath);
            // Debug.Log($"------import animation clip: ");
        }
        
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if(Application.isBatchMode)
                return;

            for (int i = 0; i < importedAssets.Length; ++i)
            {
                if(importedAssets[i].EndsWith(".anim"))
                {
                    OptimizeAnim(importedAssets[i]);
                    Debug.Log($"import animation clip: {importedAssets[i]}");
                }
            }
        }        

        static public void OptimizeAnim(GameObject go)
        {
            string assetPath = AssetDatabase.GetAssetPath(go);
            if(string.IsNullOrEmpty(assetPath))
                return;
            OptimizeAnim(assetPath);
        }

        static public void OptimizeAnim(string assetPath)
        {
            List<AnimationClip> animationClipList = GetAnimationClips(assetPath);
            foreach(AnimationClip theAnimation in animationClipList)
                OptimizeAnim(theAnimation);
        }

        static public void OptimizeAnim(AnimationClip theAnimation, bool clearScaleCurve = true)
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
                if(clearScaleCurve)
                {
                    // method 1. 删除所有scale，不做筛选
                    // foreach (EditorCurveBinding theCurveBinding in AnimationUtility.GetCurveBindings(theAnimation))
                    // {
                    //     string name = theCurveBinding.propertyName.ToLower();
                    //     if (name.Contains("scale"))
                    //     {
                    //         AnimationUtility.SetEditorCurve(theAnimation, theCurveBinding, null);
                    //     }
                    // }

                    // method 2. 删除限定scale
                    string prevPath = null;
                    int count = 0;
                    EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(theAnimation);
                    for(int i = 0; i < curves.Length; ++i)
                    {
                        AnimationClipCurveData curveData = curves[i];
                        EditorCurveBinding curveBinding = curveBindings[i];
                        string name = curveBinding.propertyName.ToLower();
                        string path = curveBinding.path;
                        if(path != prevPath)
                        {
                            count = 0;
                            prevPath = path;
                        }

                        if(name.Contains("scale.x"))
                        {
                            keyFrames = curveData.curve.keys;
                            if(keyFrames.Length == 2 && Mathf.Approximately(keyFrames[0].value, 1) && Mathf.Approximately(keyFrames[1].value, 1))
                            {
                                count++;
                            }
                        }
                        else if(name.Contains("scale.y"))
                        {
                            keyFrames = curveData.curve.keys;
                            if(keyFrames.Length == 2 && Mathf.Approximately(keyFrames[0].value, 1) && Mathf.Approximately(keyFrames[1].value, 1))
                            {
                                count++;
                            }
                        }
                        else if(name.Contains("scale.z"))
                        {
                            keyFrames = curveData.curve.keys;
                            if(keyFrames.Length == 2 && Mathf.Approximately(keyFrames[0].value, 1) && Mathf.Approximately(keyFrames[1].value, 1))
                            {
                                count++;
                            }
                        }

                        if(count == 3)
                        {
                            AnimationUtility.SetEditorCurve(theAnimation, curveBindings[i-2], null);
                            AnimationUtility.SetEditorCurve(theAnimation, curveBindings[i-1], null);
                            AnimationUtility.SetEditorCurve(theAnimation, curveBindings[i], null);

                            isDirty |= true;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("CompressAnimationClip Failed !!! animationClip : {0} error: {1}", theAnimation.name, e));
            }

            if(isDirty)
            {
                // EditorUtility.SetDirty(theAnimation);
                Debug.Log($"SetDirty: {theAnimation.name}");
            }
        }

        static private bool OptFloat(ref float src)
        {
            float dst = Mathf.Floor(src * 1000 + 0.5f) / 1000;
            bool changed = dst != src;
            src = dst;
            return changed;
        }

        static private List<AnimationClip> GetAnimationClips(string assetPath)
        {
            List<AnimationClip> list = new List<AnimationClip>();
            Object[] objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach(var obj in objects)
            {
                if(obj is AnimationClip)
                    list.Add((AnimationClip)obj);
            }
            return list;
        }



        static public void OptimizeAnimEx(AnimationClip clip)
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
    }
}

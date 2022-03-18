using UnityEditor;
using UnityEngine;
using System.IO;

namespace Framework.AssetManagement.AssetBuilder
{
    public class AnimationPostprocessor : AssetPostprocessor
    {




        [MenuItem("Assets/ZGame/提取动画文件", false, 999)]
        static public void ExtractAnimClip()
        {
            if(Selection.assetGUIDs.Length == 0)
                return;

            UnityEditor.EditorUtility.DisplayProgressBar("Extract AnimClip", "", 0);
            for(int i = 0; i < Selection.assetGUIDs.Length; ++i)
            {
                ExtractAnimClip(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]), true);
                UnityEditor.EditorUtility.DisplayProgressBar("Extract AnimClip", i + "/" + Selection.assetGUIDs.Length, i / (float)Selection.assetGUIDs.Length);
            }
            UnityEditor.EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }

        static public void ExtractAnimClip(string assetPath, bool bForceCreate = false)
        {
            UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            if (objs != null && objs.Length != 0)
            {
                foreach (UnityEngine.Object obj in objs)
                {
                    AnimationClip clip = obj as AnimationClip;
                    if(clip == null)
                        continue;
                    
                    //提取 *.anim
                    string filePath = assetPath.Substring(0, assetPath.LastIndexOf("/") + 1) + Path.GetFileNameWithoutExtension(assetPath) + ".anim";
                    {
                        UnityEngine.Object clone = UnityEngine.Object.Instantiate(clip);

                        // 优化精度
                        OptimizeAnim(clone as AnimationClip);

                        AssetDatabase.CreateAsset(clone, filePath);
                        AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
                        Debug.Log($"提取动画完成：{filePath}");
                    }
                }
            }
        }

        // 优化anim数据精度
        static public void OptimizeAnim(AnimationClip clip)
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
    }    
}
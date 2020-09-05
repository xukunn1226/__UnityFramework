using UnityEngine;
using UnityEditor;

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

//        void OnPostprocessModel(GameObject root)
//        {
//            ModelImporter modelImporter = assetImporter as ModelImporter;
//#if UNITY_2019_1_OR_NEWER
//            if(modelImporter.materialImportMode == ModelImporterMaterialImportMode.None)
//#else
//            if (!modelImporter.importMaterials)
//#endif
//            {
//                // 清空默认的内置引用资源
//                Renderer[] rdrs = root.GetComponentsInChildren<Renderer>();
//                for (int i = 0; i < rdrs.Length; ++i)
//                {
//                    Material[] mats = new Material[rdrs[i].sharedMaterials.Length];
//                    rdrs[i].sharedMaterials = mats;
//                }
//            }
//        }

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

        [MenuItem("Test/Optimize Anim")]
        static private void MenuItem_OptimizeAnim()
        {
            AnimationClip clip = Selection.activeObject as AnimationClip;
            if(clip == null)
                return;

            OptimizeAnim(clip);
        }

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

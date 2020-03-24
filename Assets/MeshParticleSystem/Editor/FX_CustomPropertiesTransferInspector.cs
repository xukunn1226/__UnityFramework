using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem
{
    [CustomEditor(typeof(FX_CustomPropertiesTransfer))]
    public class FX_CustomPropertiesTransferInspector : Editor
    {
        SerializedProperty  m_CustomColorListProp;
        SerializedProperty  m_CustomFloatListProp;
        SerializedProperty  m_CustomVector4ListProp;
        SerializedProperty  m_CustomUVProp;        

        const int           m_kIndent = 63;

        private void OnEnable()
        {
            m_CustomColorListProp   = serializedObject.FindProperty("m_CustomPropColorList");
            m_CustomFloatListProp   = serializedObject.FindProperty("m_CustomPropFloatList");
            m_CustomVector4ListProp = serializedObject.FindProperty("m_CustomPropVector4List");
            m_CustomUVProp          = serializedObject.FindProperty("m_CustomPropUV");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawColorPropList();
            EditorGUILayout.Space();
            DrawFloatPropList();
            EditorGUILayout.Space();
            DrawVector4PropList();
            EditorGUILayout.Space();
            DrawUVProp();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawColorPropList()
        {
            EditorGUILayout.LabelField("添加Color属性", new GUIStyle("LargeLabel"));
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                GUI.color = new Color(0, 0.8f, 0);
                if (GUILayout.Button("Add Color Properties"))
                {
                    m_CustomColorListProp.InsertArrayElementAtIndex(m_CustomColorListProp.arraySize);
                }
                GUI.color = Color.white;

                for (int i = 0; i < m_CustomColorListProp.arraySize; ++i)
                {
                    SerializedProperty colorProp = m_CustomColorListProp.GetArrayElementAtIndex(i);
                    DrawCustomColorProp(colorProp, i);
                    EditorGUILayout.Space();
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawCustomColorProp(SerializedProperty customColorProp, int index)
        {
            EditorGUILayout.BeginHorizontal();

            GUI.color = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(50)))
            {
                m_CustomColorListProp.DeleteArrayElementAtIndex(index);

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUI.color = Color.white;

                // id
                GUILayout.Space(10);
                SerializedProperty idProp = customColorProp.FindPropertyRelative("Id");
                if (idProp != null)
                {
                    idProp.intValue = EditorGUILayout.IntPopup("Property", idProp.intValue, FX_Const.colorDisplayedOptions, FX_Const.colorOptionValues);
                }
                EditorGUILayout.EndHorizontal();

                // delay
                SerializedProperty delayProp = customColorProp.FindPropertyRelative("Delay");
                if (delayProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(delayProp);
                    EditorGUILayout.EndHorizontal();
                }

                // duration
                SerializedProperty durationProp = customColorProp.FindPropertyRelative("Duration");
                if (durationProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(durationProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control mode
                SerializedProperty modeProp = customColorProp.FindPropertyRelative("Mode");
                if (modeProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(modeProp);
                    EditorGUILayout.EndHorizontal();

                    // isLoop, color, gradient
                    if (modeProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant)
                    {
                        SerializedProperty colorProp = customColorProp.FindPropertyRelative("Color");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(m_kIndent);
                        EditorGUILayout.PropertyField(colorProp);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        SerializedProperty loopProp = customColorProp.FindPropertyRelative("Loop");
                        SerializedProperty gradientProp = customColorProp.FindPropertyRelative("Gradient");

                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(m_kIndent);
                            EditorGUILayout.PropertyField(loopProp);
                            EditorGUILayout.EndHorizontal();
                        }

                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(m_kIndent);
                            EditorGUILayout.PropertyField(gradientProp);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

        private void DrawFloatPropList()
        {
            EditorGUILayout.LabelField("添加Float属性", new GUIStyle("LargeLabel"));
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                GUI.color = new Color(0, 0.8f, 0);
                if (GUILayout.Button("Add Float Properties"))
                {
                    m_CustomFloatListProp.InsertArrayElementAtIndex(m_CustomFloatListProp.arraySize);
                }
                GUI.color = Color.white;

                for (int i = 0; i < m_CustomFloatListProp.arraySize; ++i)
                {
                    SerializedProperty floatProp = m_CustomFloatListProp.GetArrayElementAtIndex(i);
                    DrawCustomFloatProp(floatProp, i);
                    EditorGUILayout.Space();
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawCustomFloatProp(SerializedProperty customFloatProp, int index)
        {
            EditorGUILayout.BeginHorizontal();

            GUI.color = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(50)))
            {
                m_CustomFloatListProp.DeleteArrayElementAtIndex(index);

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUI.color = Color.white;

                // id
                GUILayout.Space(10);
                SerializedProperty idProp = customFloatProp.FindPropertyRelative("Id");
                if (idProp != null)
                {
                    idProp.intValue = EditorGUILayout.IntPopup("Property", idProp.intValue, FX_Const.floatDisplayedOptions, FX_Const.floatOptionValues);
                }
                EditorGUILayout.EndHorizontal();

                // delay
                SerializedProperty delayProp = customFloatProp.FindPropertyRelative("Delay");
                if (delayProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(delayProp);
                    EditorGUILayout.EndHorizontal();
                }

                // duration
                SerializedProperty durationProp = customFloatProp.FindPropertyRelative("Duration");
                if (durationProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(durationProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control mode
                SerializedProperty modeProp = customFloatProp.FindPropertyRelative("Mode");
                if (modeProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(modeProp);
                    EditorGUILayout.EndHorizontal();

                    // isLoop, color, gradient
                    if (modeProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant)
                    {
                        SerializedProperty floatProp = customFloatProp.FindPropertyRelative("Value");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(m_kIndent);
                        EditorGUILayout.PropertyField(floatProp);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        SerializedProperty loopProp = customFloatProp.FindPropertyRelative("Loop");
                        SerializedProperty curveProp = customFloatProp.FindPropertyRelative("Curve");

                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(m_kIndent);
                            EditorGUILayout.PropertyField(loopProp);
                            EditorGUILayout.EndHorizontal();
                        }

                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(m_kIndent);
                            EditorGUILayout.PropertyField(curveProp);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

        private void DrawVector4PropList()
        {
            EditorGUILayout.LabelField("添加Vector4属性", new GUIStyle("LargeLabel"));
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                GUI.color = new Color(0, 0.8f, 0);
                if (GUILayout.Button("Add Vector4 Properties"))
                {
                    m_CustomVector4ListProp.InsertArrayElementAtIndex(m_CustomVector4ListProp.arraySize);
                }
                GUI.color = Color.white;

                for (int i = 0; i < m_CustomVector4ListProp.arraySize; ++i)
                {
                    SerializedProperty vector4Prop = m_CustomVector4ListProp.GetArrayElementAtIndex(i);
                    DrawCustomVector4Prop(vector4Prop, i);
                    EditorGUILayout.Space();
                }
            }
            GUILayout.EndVertical();
        }
        
        private void DrawCustomVector4Prop(SerializedProperty customVector4Prop, int index)
        {
            EditorGUILayout.BeginHorizontal();

            GUI.color = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(50)))
            {
                m_CustomVector4ListProp.DeleteArrayElementAtIndex(index);

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUI.color = Color.white;

                // id
                GUILayout.Space(10);
                SerializedProperty idProp = customVector4Prop.FindPropertyRelative("Id");
                if (idProp != null)
                {
                    idProp.intValue = EditorGUILayout.IntPopup("Property", idProp.intValue, FX_Const.vector4DisplayedOptions, FX_Const.vector4OptionValues);
                }
                EditorGUILayout.EndHorizontal();

                // delay
                SerializedProperty delayProp = customVector4Prop.FindPropertyRelative("Delay");
                if (delayProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(delayProp);
                    EditorGUILayout.EndHorizontal();
                }

                // duration
                SerializedProperty durationProp = customVector4Prop.FindPropertyRelative("Duration");
                if (durationProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(durationProp);
                    EditorGUILayout.EndHorizontal();
                }

                // isLoop
                SerializedProperty loopProp = customVector4Prop.FindPropertyRelative("Loop");
                if (loopProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(loopProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control modeX
                SerializedProperty modeXProp = customVector4Prop.FindPropertyRelative("ModeX");
                if (modeXProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(modeXProp);
                    EditorGUILayout.EndHorizontal();

                    // color, gradient
                    SerializedProperty valueXProp = modeXProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant ? customVector4Prop.FindPropertyRelative("ValueX") : customVector4Prop.FindPropertyRelative("CurveX");

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(valueXProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control modeY
                SerializedProperty modeYProp = customVector4Prop.FindPropertyRelative("ModeY");
                if (modeYProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(modeYProp);
                    EditorGUILayout.EndHorizontal();

                    // color, gradient
                    SerializedProperty valueYProp = modeYProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant ? customVector4Prop.FindPropertyRelative("ValueY") : customVector4Prop.FindPropertyRelative("CurveY");

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(valueYProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control modeZ
                SerializedProperty modeZProp = customVector4Prop.FindPropertyRelative("ModeZ");
                if (modeZProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(modeZProp);
                    EditorGUILayout.EndHorizontal();

                    // color, gradient
                    SerializedProperty valueZProp = modeZProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant ? customVector4Prop.FindPropertyRelative("ValueZ") : customVector4Prop.FindPropertyRelative("CurveZ");

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(valueZProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control modeW
                SerializedProperty modeWProp = customVector4Prop.FindPropertyRelative("ModeW");
                if (modeWProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(modeWProp);
                    EditorGUILayout.EndHorizontal();

                    // color, gradient
                    SerializedProperty valueWProp = modeWProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant ? customVector4Prop.FindPropertyRelative("ValueW") : customVector4Prop.FindPropertyRelative("CurveW");

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(m_kIndent);
                    EditorGUILayout.PropertyField(valueWProp);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawUVProp()
        {
            EditorGUILayout.LabelField("编辑UV动画", new GUIStyle("LargeLabel"));
            GUILayout.BeginVertical(new GUIStyle("HelpBox"));
            {
                SerializedProperty isActiveProp = m_CustomUVProp.FindPropertyRelative("Active");
                EditorGUILayout.PropertyField(isActiveProp);

                EditorGUI.BeginDisabledGroup(!isActiveProp.boolValue);
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.IntPopup("Property", 0, new string[] { "_MainTexPropertyBlockST" }, new int[] { 0 });
                    EditorGUILayout.EndHorizontal();

                    SerializedProperty delayProp = m_CustomUVProp.FindPropertyRelative("Delay");
                    EditorGUILayout.PropertyField(delayProp);

                    SerializedProperty durationProp = m_CustomUVProp.FindPropertyRelative("Duration");
                    EditorGUILayout.PropertyField(durationProp);

                    SerializedProperty speedProp = m_CustomUVProp.FindPropertyRelative("Speed");
                    EditorGUILayout.PropertyField(speedProp);

                    SerializedProperty startOffsetProp = m_CustomUVProp.FindPropertyRelative("StartOffset");
                    EditorGUILayout.PropertyField(startOffsetProp);

                    SerializedProperty tileScaleProp = m_CustomUVProp.FindPropertyRelative("TileScale");
                    EditorGUILayout.PropertyField(tileScaleProp);

                    SerializedProperty isLoopProp = m_CustomUVProp.FindPropertyRelative("Loop");
                    EditorGUILayout.PropertyField(isLoopProp);

                    SerializedProperty isLoopResetProp = m_CustomUVProp.FindPropertyRelative("LoopReset");
                    EditorGUILayout.PropertyField(isLoopResetProp);

                    SerializedProperty curveProp = m_CustomUVProp.FindPropertyRelative("Curve");
                    EditorGUILayout.PropertyField(curveProp);
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndVertical();
        }
    }
}
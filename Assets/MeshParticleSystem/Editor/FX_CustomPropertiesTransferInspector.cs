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
        //SerializedProperty customMeshProjectionProp;

        bool _isColorPropFoldout = true;
        bool _isFloatPropFoldout = true;
        bool _isVector4PropFoldout = true;
        bool _isUVProp = true;

        private void OnEnable()
        {
            m_CustomColorListProp = serializedObject.FindProperty("m_CustomPropColorList");
            m_CustomFloatListProp = serializedObject.FindProperty("m_CustomPropFloatList");
            m_CustomVector4ListProp = serializedObject.FindProperty("m_CustomPropVector4List");
            m_CustomUVProp = serializedObject.FindProperty("m_CustomPropUV");
            //customMeshProjectionProp = serializedObject.FindProperty("useMeshProjection");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            serializedObject.Update();

            DrawColorPropList();
            DrawFloatPropList();
            DrawVector4PropList();
            DrawUVProp();

            //EditorGUILayout.PropertyField(customMeshProjectionProp);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawColorPropList()
        {
            GUIStyle stageBoxStyle = GUI.skin.box;
            EditorGUILayout.BeginVertical(stageBoxStyle);
            {
                Rect rect = EditorGUILayout.GetControlRect(true);

                GUIContent label = new GUIContent("添加Color属性");
                label.image = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                float labelWidth = EditorGUIUtility.labelWidth - (4 + EditorGUI.indentLevel * 15);
                Rect r = rect; r.width = labelWidth;
                EditorGUI.LabelField(rect, label);

                _isColorPropFoldout = EditorGUI.Foldout(rect, _isColorPropFoldout, GUIContent.none, true);
                if (_isColorPropFoldout)
                {
                    GUI.color = new Color(0, 0.8f, 0);
                    if( GUILayout.Button("Add Color Properties") )
                    {
                        m_CustomColorListProp.InsertArrayElementAtIndex(m_CustomColorListProp.arraySize);
                    }
                    GUI.color = Color.white;

                    for(int i = 0; i < m_CustomColorListProp.arraySize; ++i)
                    {
                        SerializedProperty colorProp = m_CustomColorListProp.GetArrayElementAtIndex(i);
                        DrawCustomColorProp(colorProp, i);
                        EditorGUILayout.Space();
                    }
                }
            }
            EditorGUILayout.EndVertical();
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
                SerializedProperty idProp = customColorProp.FindPropertyRelative("id");
                if (idProp != null)
                {
                    idProp.intValue = EditorGUILayout.IntPopup(idProp.intValue, FX_Const.colorDisplayedOptions, FX_Const.colorOptionValues);
                }
                EditorGUILayout.EndHorizontal();

                // delay
                SerializedProperty delayProp = customColorProp.FindPropertyRelative("delay");
                if(delayProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(delayProp);
                    EditorGUILayout.EndHorizontal();
                }

                // duration
                SerializedProperty durationProp = customColorProp.FindPropertyRelative("duration");
                if (durationProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(durationProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control mode
                SerializedProperty modeProp = customColorProp.FindPropertyRelative("mode");
                if(modeProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(modeProp);
                    EditorGUILayout.EndHorizontal();

                    // isLoop, color, gradient
                    if (modeProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant)
                    {
                        SerializedProperty colorProp = customColorProp.FindPropertyRelative("color");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(60);
                        EditorGUILayout.PropertyField(colorProp);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        SerializedProperty loopProp = customColorProp.FindPropertyRelative("isLoop");
                        SerializedProperty gradientProp = customColorProp.FindPropertyRelative("gradient");

                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(60);
                            EditorGUILayout.PropertyField(loopProp);
                            EditorGUILayout.EndHorizontal();
                        }

                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(60);
                            EditorGUILayout.PropertyField(gradientProp);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

        private void DrawFloatPropList()
        {
            GUIStyle stageBoxStyle = GUI.skin.box;
            EditorGUILayout.BeginVertical(stageBoxStyle);
            {
                Rect rect = EditorGUILayout.GetControlRect(true);

                GUIContent label = new GUIContent("添加Float属性");
                label.image = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                float labelWidth = EditorGUIUtility.labelWidth - (4 + EditorGUI.indentLevel * 15);
                Rect r = rect; r.width = labelWidth;
                EditorGUI.LabelField(rect, label);

                _isFloatPropFoldout = EditorGUI.Foldout(rect, _isFloatPropFoldout, GUIContent.none, true);
                if (_isFloatPropFoldout)
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
            }
            EditorGUILayout.EndVertical();
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
                SerializedProperty idProp = customFloatProp.FindPropertyRelative("id");
                if (idProp != null)
                {
                    idProp.intValue = EditorGUILayout.IntPopup(idProp.intValue, FX_Const.floatDisplayedOptions, FX_Const.floatOptionValues);
                }
                EditorGUILayout.EndHorizontal();

                // delay
                SerializedProperty delayProp = customFloatProp.FindPropertyRelative("delay");
                if (delayProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(delayProp);
                    EditorGUILayout.EndHorizontal();
                }

                // duration
                SerializedProperty durationProp = customFloatProp.FindPropertyRelative("duration");
                if (durationProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(durationProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control mode
                SerializedProperty modeProp = customFloatProp.FindPropertyRelative("mode");
                if (modeProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(modeProp);
                    EditorGUILayout.EndHorizontal();

                    // isLoop, color, gradient
                    if (modeProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant)
                    {
                        SerializedProperty floatProp = customFloatProp.FindPropertyRelative("value");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(60);
                        EditorGUILayout.PropertyField(floatProp);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        SerializedProperty loopProp = customFloatProp.FindPropertyRelative("isLoop");
                        SerializedProperty curveProp = customFloatProp.FindPropertyRelative("curve");

                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(60);
                            EditorGUILayout.PropertyField(loopProp);
                            EditorGUILayout.EndHorizontal();
                        }

                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(60);
                            EditorGUILayout.PropertyField(curveProp);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }

        private void DrawVector4PropList()
        {
            GUIStyle stageBoxStyle = GUI.skin.box;
            EditorGUILayout.BeginVertical(stageBoxStyle);
            {
                Rect rect = EditorGUILayout.GetControlRect(true);

                GUIContent label = new GUIContent("添加Vector4属性");
                label.image = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                float labelWidth = EditorGUIUtility.labelWidth - (4 + EditorGUI.indentLevel * 15);
                Rect r = rect; r.width = labelWidth;
                EditorGUI.LabelField(rect, label);

                _isVector4PropFoldout = EditorGUI.Foldout(rect, _isVector4PropFoldout, GUIContent.none, true);
                if (_isVector4PropFoldout)
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
            }
            EditorGUILayout.EndVertical();
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
                SerializedProperty idProp = customVector4Prop.FindPropertyRelative("id");
                if (idProp != null)
                {
                    idProp.intValue = EditorGUILayout.IntPopup(idProp.intValue, FX_Const.vector4DisplayedOptions, FX_Const.vector4OptionValues);
                }
                EditorGUILayout.EndHorizontal();

                // delay
                SerializedProperty delayProp = customVector4Prop.FindPropertyRelative("delay");
                if (delayProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(delayProp);
                    EditorGUILayout.EndHorizontal();
                }

                // duration
                SerializedProperty durationProp = customVector4Prop.FindPropertyRelative("duration");
                if (durationProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(durationProp);
                    EditorGUILayout.EndHorizontal();
                }

                // isLoop
                SerializedProperty loopProp = customVector4Prop.FindPropertyRelative("isLoop");
                if (loopProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(loopProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control modeX
                SerializedProperty modeXProp = customVector4Prop.FindPropertyRelative("modeX");
                if (modeXProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(modeXProp);
                    EditorGUILayout.EndHorizontal();

                    // color, gradient
                    SerializedProperty valueXProp = modeXProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant ? customVector4Prop.FindPropertyRelative("valueX") : customVector4Prop.FindPropertyRelative("curveX");

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(valueXProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control modeY
                SerializedProperty modeYProp = customVector4Prop.FindPropertyRelative("modeY");
                if (modeYProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(modeYProp);
                    EditorGUILayout.EndHorizontal();

                    // color, gradient
                    SerializedProperty valueYProp = modeYProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant ? customVector4Prop.FindPropertyRelative("valueY") : customVector4Prop.FindPropertyRelative("curveY");

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(valueYProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control modeZ
                SerializedProperty modeZProp = customVector4Prop.FindPropertyRelative("modeZ");
                if (modeZProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(modeZProp);
                    EditorGUILayout.EndHorizontal();

                    // color, gradient
                    SerializedProperty valueZProp = modeZProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant ? customVector4Prop.FindPropertyRelative("valueZ") : customVector4Prop.FindPropertyRelative("curveZ");

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(valueZProp);
                    EditorGUILayout.EndHorizontal();
                }

                // control modeW
                SerializedProperty modeWProp = customVector4Prop.FindPropertyRelative("modeW");
                if (modeWProp != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(modeWProp);
                    EditorGUILayout.EndHorizontal();

                    // color, gradient
                    SerializedProperty valueWProp = modeWProp.enumValueIndex == (int)FX_CustomPropertiesTransfer.ControlMode.Constant ? customVector4Prop.FindPropertyRelative("valueW") : customVector4Prop.FindPropertyRelative("curveW");

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(60);
                    EditorGUILayout.PropertyField(valueWProp);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawUVProp()
        {
            GUIStyle stageBoxStyle = GUI.skin.box;
            EditorGUILayout.BeginVertical(stageBoxStyle);
            {
                Rect rect = EditorGUILayout.GetControlRect(true);

                GUIContent label = new GUIContent("编辑UV动画");
                label.image = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                float labelWidth = EditorGUIUtility.labelWidth - (4 + EditorGUI.indentLevel * 15);
                Rect r = rect; r.width = labelWidth;
                EditorGUI.LabelField(rect, label);

                _isUVProp = EditorGUI.Foldout(rect, _isUVProp, GUIContent.none, true);
                if (_isUVProp)
                {
                    SerializedProperty isActiveProp = m_CustomUVProp.FindPropertyRelative("Active");
                    EditorGUILayout.PropertyField(isActiveProp);

                    if (isActiveProp.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("");
                        EditorGUILayout.IntPopup(0, new string[] { "_MainTexPropertyBlockST" }, new int[] { 0 });
                        EditorGUILayout.EndHorizontal();

                        SerializedProperty delayProp = m_CustomUVProp.FindPropertyRelative("delay");
                        EditorGUILayout.PropertyField(delayProp);

                        SerializedProperty durationProp = m_CustomUVProp.FindPropertyRelative("duration");
                        EditorGUILayout.PropertyField(durationProp);

                        SerializedProperty speedProp = m_CustomUVProp.FindPropertyRelative("speed");
                        EditorGUILayout.PropertyField(speedProp);

                        SerializedProperty startOffsetProp = m_CustomUVProp.FindPropertyRelative("startOffset");
                        EditorGUILayout.PropertyField(startOffsetProp);

                        SerializedProperty tileScaleProp = m_CustomUVProp.FindPropertyRelative("tileScale");
                        EditorGUILayout.PropertyField(tileScaleProp);

                        SerializedProperty isLoopProp = m_CustomUVProp.FindPropertyRelative("isLoop");
                        EditorGUILayout.PropertyField(isLoopProp);

                        SerializedProperty isLoopResetProp = m_CustomUVProp.FindPropertyRelative("isLoopReset");
                        EditorGUILayout.PropertyField(isLoopResetProp);

                        SerializedProperty curveProp = m_CustomUVProp.FindPropertyRelative("curve");
                        EditorGUILayout.PropertyField(curveProp);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}
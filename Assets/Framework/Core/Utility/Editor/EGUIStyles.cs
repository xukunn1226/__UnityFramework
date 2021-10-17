﻿using UnityEngine;
using System.Collections;

namespace Framework.Core.Editor
{
    public class EGUIStyles
    {
        private static GUIStyle m_ToolbarButton1;
        private static GUIStyle m_ToolbarDropDown1;
        private static GUIStyle m_ToolbarPopup1;
        private static GUIStyle m_Foldout1;
        private static GUIStyle m_SelectGrid1;
        private static GUIStyle m_Button1;
        private static GUIStyle m_Button2;
        private static GUIStyle m_Button3;
        private static GUIStyle m_Button4;
        private static GUIStyle m_Label1;
        private static GUIStyle m_Label2;
        private static GUIStyle m_TitleTextStyle = null;
        private static GUIStyle m_FieldToggleStyle = null;
        private static GUIStyle m_ConditionStyle = null;
        private static GUIStyle m_ButtonStyle = null;

        public static GUIStyle ToolbarButton1
        {
            get
            {
                if (m_ToolbarButton1 == null)
                {
                    m_ToolbarButton1 = new GUIStyle(UnityEditor.EditorStyles.toolbarButton);
                    m_ToolbarButton1.fixedHeight = 30;
                    m_ToolbarButton1.fontSize = 20;
                    m_ToolbarButton1.alignment = TextAnchor.MiddleCenter;
                }
                return m_ToolbarButton1;
            }
        }

        public static GUIStyle ToolbarDropDown1
        {
            get
            {
                if (m_ToolbarDropDown1 == null)
                {
                    m_ToolbarDropDown1 = new GUIStyle(UnityEditor.EditorStyles.toolbarDropDown);
                    m_ToolbarDropDown1.fixedHeight = 24;
                    m_ToolbarDropDown1.fontSize = 16;
                    m_ToolbarDropDown1.alignment = TextAnchor.MiddleLeft;
                }
                return m_ToolbarDropDown1;
            }
        }

        public static GUIStyle ToolbarPopup1
        {
            get
            {
                if (m_ToolbarPopup1 == null)
                {
                    m_ToolbarPopup1 = new GUIStyle(UnityEditor.EditorStyles.toolbarPopup);
                    m_ToolbarPopup1.fixedHeight = 24;
                    m_ToolbarPopup1.fontSize = 16;
                    m_ToolbarPopup1.alignment = TextAnchor.MiddleLeft;
                }
                return m_ToolbarPopup1;
            }
        }

        public static GUIStyle Foldout1
        {
            get
            {
                if (m_Foldout1 == null)
                {
                    m_Foldout1 = new GUIStyle(UnityEditor.EditorStyles.foldout);
                    m_Foldout1.fixedHeight = 24;
                    m_Foldout1.fontSize = 16;
                    m_Foldout1.alignment = TextAnchor.MiddleLeft;
                }
                return m_Foldout1;
            }
        }

        public static GUIStyle Button1
        {
            get
            {
                if (m_Button1 == null)
                {
                    m_Button1 = new GUIStyle("button");
                    m_Button1.alignment = TextAnchor.MiddleCenter;
                    m_Button1.fontSize = 24;
                }
                return m_Button1;
            }
        }

        public static GUIStyle Button2
        {
            get
            {
                if (m_Button2 == null)
                {
                    m_Button2 = new GUIStyle(UnityEditor.EditorStyles.toolbarButton);
                    m_Button2.fixedHeight = 24;
                    m_Button2.fontSize = 16;
                    m_Button2.alignment = TextAnchor.MiddleCenter;
                }
                return m_Button2;
            }
        }

        public static GUIStyle Button3
        {
            get
            {
                if (m_Button3 == null)
                {
                    m_Button3 = new GUIStyle("button");
                    m_Button3.alignment = TextAnchor.MiddleCenter;
                    m_Button3.fontSize = 20;
                }
                return m_Button3;
            }
        }

        public static GUIStyle Button4
        {
            get
            {
                if (m_Button4 == null)
                {
                    m_Button4 = new GUIStyle("button");
                    m_Button4.alignment = TextAnchor.MiddleLeft;
                    m_Button4.fontSize = 16;
                }
                return m_Button4;
            }
        }

        public static GUIStyle Label1
        {
            get
            {
                if (m_Label1 == null)
                {
                    m_Label1 = new GUIStyle(UnityEditor.EditorStyles.label);
                    m_Label1.fixedHeight = 20;
                    m_Label1.fontSize = 10;
                    m_Label1.alignment = TextAnchor.MiddleCenter;
                }
                return m_Label1;
            }
        }

        public static GUIStyle Label2
        {
            get
            {
                if (m_Label2 == null)
                {
                    m_Label2 = new GUIStyle(UnityEditor.EditorStyles.label);
                    m_Label2.fixedHeight = 20;
                    m_Label2.fontSize = 12;
                    m_Label2.alignment = TextAnchor.MiddleLeft;
                }
                return m_Label2;
            }
        }

        public static GUIStyle SelectGrid1
        {
            get
            {
                if (m_SelectGrid1 == null)
                {
                    m_SelectGrid1 = new GUIStyle(UnityEditor.EditorStyles.toolbarButton);
                    m_SelectGrid1.fixedHeight = 24;
                    m_SelectGrid1.fontSize = 16;
                    m_SelectGrid1.alignment = TextAnchor.MiddleLeft;
                }
                return m_SelectGrid1;
            }
        }



        public static GUIStyle TitleTextStyle
        {
            get
            {
                if (m_TitleTextStyle == null)
                {
                    m_TitleTextStyle = new GUIStyle(GUI.skin.label);
                    m_TitleTextStyle.alignment = TextAnchor.MiddleCenter;
                    m_TitleTextStyle.fontSize = 24;
                }
                return m_TitleTextStyle;
            }
        }

        public static GUIStyle ConditionTypeStyle
        {
            get
            {
                if (m_ConditionStyle == null)
                {
                    m_ConditionStyle = new GUIStyle(GUI.skin.label);
                    m_ConditionStyle.fontStyle = FontStyle.Bold;
                }
                return m_ConditionStyle;
            }
        }

        public static GUIStyle FieldToggleStyle
        {
            get
            {
                if (m_FieldToggleStyle == null)
                {
                    m_FieldToggleStyle = new GUIStyle(GUI.skin.toggle);
                    m_FieldToggleStyle.fontSize = 24;
                }
                return m_FieldToggleStyle;
            }
        }

        public static GUIStyle ButtonStyle
        {
            get
            {
                if (m_ButtonStyle == null)
                {
                    m_ButtonStyle = new GUIStyle(GUI.skin.button);
                    m_ButtonStyle.alignment = TextAnchor.MiddleCenter;
                    m_ButtonStyle.fontSize = 20;
                }
                return m_ButtonStyle;
            }
        }
    }
}
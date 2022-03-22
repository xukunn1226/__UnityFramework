using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Presets;

namespace Framework.Core.Editor
{
    public class MyWindowSettings : ScriptableObject
    {
        [SerializeField]
        string m_SomeSettings;

        public void Init(MyEditorWindow window)
        {
            m_SomeSettings = window.someSettings;
        }

        public void ApplySettings(MyEditorWindow window)
        {
            window.someSettings = m_SomeSettings;
            window.Repaint();
        }
    }
}
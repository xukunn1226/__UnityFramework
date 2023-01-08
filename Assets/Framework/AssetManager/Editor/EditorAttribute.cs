using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace Framework.AssetManagement.AssetEditorWindow
{
    public class DisplayNameAttribute : Attribute
    {
        public string DisplayName;

        public DisplayNameAttribute(string name)
        {
            this.DisplayName = name;
        }
    }

    public static class EditorAttribute
    {
        internal static T GetAttribute<T>(Type type) where T : Attribute
        {
            return (T)type.GetCustomAttribute(typeof(T), false);
        }

        internal static T GetAttribute<T>(MethodInfo methodInfo) where T : Attribute
        {
            return (T)methodInfo.GetCustomAttribute(typeof(T), false);
        }

        internal static T GetAttribute<T>(FieldInfo field) where T : Attribute
        {
            return (T)field.GetCustomAttribute(typeof(T), false);
        }
    }
}
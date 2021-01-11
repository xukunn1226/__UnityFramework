using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Framework.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FileConfigAttribute : Attribute
    {
        public string Filename;

        public FileConfigAttribute(string filename)
        {
            this.Filename = filename;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyConfigAttribute : Attribute
    {
    }

    [FileConfig("_actor")]
    public class Actor
    {
        [PropertyConfig] private float   m_Float;
        [PropertyConfig] public int      m_Int;
        [PropertyConfig] public string   m_Str;
        [PropertyConfig] public object   Obj { get; }

        public void PrintAttribute()
        {
            FileConfigAttribute[] attr = (FileConfigAttribute[])GetType().GetCustomAttributes(typeof(FileConfigAttribute), false);
        }

        public void LoadConfig()
        {

        }
    }

    public class Ghost : Actor
    {

    }

    public class TestAttributes
    {
        public void Test()
        {
            PrintAuthorInfo(typeof(Actor));
            TestAssignableFrom();
        }

        private static void PrintAuthorInfo(System.Type t)
        {
            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(t);  // reflection

            foreach (System.Attribute attr in attrs)
            {
                if (attr is FileConfigAttribute)
                {
                    FileConfigAttribute a = (FileConfigAttribute)attr;
                    Debug.Log($"   {a.Filename}");
                }
            }
        }

        public static void TestAssignableFrom()
        {
            TestAttributes aa = new TestAttributes();
            Assembly assembly = aa.GetType().Assembly;
            Type[] types = assembly.GetTypes();
            Type type = typeof(Actor);
            for (int i = 0; i < types.Length; i++)
            {
                Debug.Log($"{types[i].Namespace}    {types[i].Name}     {types[i].FullName}");
                if (type.IsAssignableFrom(types[i]) && !types[i].IsInterface)
                {
                    Debug.Log($"{types[i].Name}");
                }
            }
        }
    }
}
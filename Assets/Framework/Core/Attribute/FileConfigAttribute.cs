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
        [PropertyConfig] static public float s_value;
        [PropertyConfig] static private int s_int;

        [PropertyConfig] private float   m_Float = 1.23f;
        [PropertyConfig] protected int      m_Int = 2;
        [PropertyConfig] public string   m_Str = "122332223dsfsd";


        [PropertyConfig] private object   Obj1 { get; }
        [PropertyConfig] static public string s_staticStr { get; }

        public void PrintAttribute()
        {
            FileConfigAttribute[] attr = (FileConfigAttribute[])GetType().GetCustomAttributes(typeof(FileConfigAttribute), false);
        }

        public void LoadConfig()
        {
            // [Framework.Core].[Actor]
            string ns = GetType().Namespace;
            string cn = GetType().Name;

            Type type = GetType();
            Attribute fileConfigAttr = type.GetCustomAttribute(typeof(FileConfigAttribute), true);
            if (fileConfigAttr == null)
                return;

            foreach(var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                Attribute propAttr = prop.GetCustomAttribute(typeof(PropertyConfigAttribute));
                if (propAttr == null) continue;

                Debug.Log($"{prop.Name}     {prop.PropertyType}     {prop.GetValue(this)}");
            }

            Debug.Log("--------------------------");

            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                Attribute propAttr = field.GetCustomAttribute(typeof(PropertyConfigAttribute));
                if (propAttr == null) continue;

                Debug.Log($"{field.Name}     {field.FieldType}     {field.GetValue(this)}");
            }
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
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

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    abstract public class PropertyConfigAttribute : Attribute
    {
    }

    public class IntPropertyConfigAttribute : PropertyConfigAttribute
    { }    
    public class FloatPropertyConfigAttribute : PropertyConfigAttribute
    { }
    public class StringPropertyConfigAttribute : PropertyConfigAttribute
    { }
}
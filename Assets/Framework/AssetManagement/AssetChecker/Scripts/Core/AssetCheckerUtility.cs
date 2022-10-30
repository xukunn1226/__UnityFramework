using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Framework.AssetManagement.AssetChecker
{
    static public class AssetCheckerUtility
    {
        static public string TrimProjectFolder(string path)
        {
            if(path.StartsWith(UnityEngine.Application.dataPath, System.StringComparison.CurrentCultureIgnoreCase))
            {
                return path.Substring(UnityEngine.Application.dataPath.Length - 6);
            }
            return path;
        }

        public enum UnityType
        {
            Invalid,
            Object,
            AnimationClip,
            AudioClip,
            ComputerShader,
            Font,
            GUISkin,
            Material,
            Mesh,
            Model,
            PhysicMaterial,
            Prefab,
            Shader,
            Sprite,
            Texture,
            VideoClip,
        }

        static public UnityType GetUnityType(string value)
        {
            UnityType result;
            if(!Enum.TryParse<UnityType>(value, true, out result))
            {
                return UnityType.Invalid;
            }
            return result;
        }

        static public Type GetUnityObjectType(UnityType type)
        {
            switch(type)
            {
                case UnityType.Object:
                    return typeof(UnityEngine.Object);
                case UnityType.AnimationClip:
                    return typeof(UnityEngine.AnimationClip);
                case UnityType.AudioClip:
                    return typeof(UnityEngine.AudioClip);
                case UnityType.ComputerShader:
                    return typeof(UnityEngine.ComputeShader);
                case UnityType.Font:
                    return typeof(UnityEngine.Font);
                case UnityType.GUISkin:
                    return typeof(UnityEngine.GUISkin);
                case UnityType.Material:
                    return typeof(UnityEngine.Material);
                case UnityType.Mesh:
                    return typeof(UnityEngine.Mesh);
                case UnityType.Model:
                    return typeof(UnityEngine.GameObject);
                case UnityType.PhysicMaterial:
                    return typeof(UnityEngine.PhysicMaterial);
                case UnityType.Prefab:
                    return typeof(UnityEngine.GameObject);
                case UnityType.Shader:
                    return typeof(UnityEngine.Shader);
                case UnityType.Sprite:
                    return typeof(UnityEngine.Sprite);
                case UnityType.Texture:
                    return typeof(UnityEngine.Texture);
                case UnityType.VideoClip:
                    return typeof(UnityEngine.Video.VideoClip);
            }   
            return null;
        }
    }

    public class BaseFuncComponentParam
    {
        public string ComponentTypeName = "";
        public string ComponentParamJson = "";
    }

    public class BaseFuncComponentParam<T> : BaseFuncComponentParam
    {
        static public T CreateComponent(BaseFuncComponentParam param)
        {
            var subTypes = TypeCache.GetTypesDerivedFrom(typeof(T));
            foreach (var subType in subTypes)
            {
                if (subType.Name == param.ComponentTypeName)
                {
                    var component = Activator.CreateInstance(subType);
                    if (component != null)
                    {
                        var paramJson = param.ComponentParamJson;
                        if (!string.IsNullOrEmpty(paramJson))
                        {
                            var paramObj = JsonUtility.FromJson(paramJson, subType);
                            if (paramObj != null)
                            {
                                var paramType = paramObj.GetType();
                                var fields = paramType.GetFields();
                                foreach (var field in fields)
                                {
                                    var value = field.GetValue(paramObj);
                                    field.SetValue(component, value);
                                }
                            }
                        }
                    }

                    return (T)component;
                }
            }

            return default(T);
        }

        static public BaseFuncComponentParam CreateParam(T component)
        {
            var param = new BaseFuncComponentParam();
            param.ComponentTypeName = component.GetType().Name;
            param.ComponentParamJson = EditorJsonUtility.ToJson(component);
            return param;
        }
    }
}
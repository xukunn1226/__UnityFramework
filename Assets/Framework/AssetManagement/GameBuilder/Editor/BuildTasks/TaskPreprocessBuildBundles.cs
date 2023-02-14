using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework.AssetManagement.AssetEditorWindow
{
    /// <summary>
    /// 执行自定义的构建任务（构建资源包前执行）
    /// </summary>
    [TaskAttribute("Step0. 执行自定义的构建任务（构建资源包前执行）")]
    public class TaskPreprocessBuildBundles : IGameBuildTask
    {
        private Dictionary<string, System.Type> m_CacheBuildTypes = new Dictionary<string, System.Type>();
        private Dictionary<string, IPreprocessBuildBundle> m_CacheBuildInstance = new Dictionary<string, IPreprocessBuildBundle>();

        void IGameBuildTask.Run(Framework.AssetManagement.AssetEditorWindow.BuildContext context)
        {
            CacheBuildTypes();

            try
            {
                foreach (var pairValue in m_CacheBuildTypes)
                {
                    IPreprocessBuildBundle processor = GetPreprocessBuildBundleInstance(pairValue.Value);
                    processor.Run(context);
                }
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private void CacheBuildTypes()
        {
            m_CacheBuildTypes.Clear();

            List<Type> types = EditorTools.GetAssignableTypes(typeof(IPreprocessBuildBundle));
            foreach(var type in types)
            {
                if(m_CacheBuildTypes.ContainsKey(type.Name) == false)
                {
                    m_CacheBuildTypes.Add(type.Name, type);
                }
            }
        }

        IPreprocessBuildBundle GetPreprocessBuildBundleInstance(Type type)
        {
            if(m_CacheBuildInstance.TryGetValue(type.Name, out var instance))
                return instance;

            if(m_CacheBuildTypes.TryGetValue(type.Name, out var t))
            {
                instance = (IPreprocessBuildBundle)Activator.CreateInstance(t);
                m_CacheBuildInstance.Add(type.Name, instance);
                return instance;
            }
            else
            {
                throw new Exception($"should never get here");
            }
        }
    }
}
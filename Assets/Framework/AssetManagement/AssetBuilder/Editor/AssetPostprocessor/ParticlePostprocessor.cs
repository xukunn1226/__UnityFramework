using UnityEditor;
using System.Reflection;
using System;
using UnityEngine;

namespace Framework.AssetManagement.AssetBuilder
{
    public class ParticlePostprocessor : AssetPostprocessor
    {
        [MenuItem("Tools/Misc/CheckFailParticleCulling", false)]
        static public void CheckFailParticleCulling()
        {
            Assembly ass = typeof(UnityEditor.Editor).Assembly;
            Type t = ass.GetType("UnityEditor.ParticleSystemUI");
            MethodInfo init = t.GetMethod("Init");
            FieldInfo m_SupportsCullingText = t.GetField("m_SupportsCullingText", BindingFlags.Instance | BindingFlags.NonPublic);
            object particleSystemUI = ass.CreateInstance("UnityEditor.ParticleSystemUI");

            foreach (var obj in Selection.objects)
            {
                if (obj is GameObject)
                {
                    ParticleSystem[] particles = (obj as GameObject).GetComponentsInChildren<ParticleSystem>(true);
                    string result = "";
                    foreach (ParticleSystem particle in particles)
                    {
                        init.Invoke(particleSystemUI, new object[] { null, new ParticleSystem[] { particle } });
                        string subResult = m_SupportsCullingText.GetValue(particleSystemUI) as string;
                        if (subResult != null)
                        {
                            result += particle.name + " (" + subResult.Replace("\n","") + ")\n";
                        }
                    }
                    if (result != "")
                    {
                        Debug.Log(AssetDatabase.GetAssetPath(obj) + "\n" + result,obj);
                    }
                }
            }
        }
    }
}

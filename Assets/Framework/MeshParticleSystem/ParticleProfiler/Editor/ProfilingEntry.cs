using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MeshParticleSystem.Profiler
{
    [InitializeOnLoad]
    static public class ProfilingEntry
    {
        private const string ProfilingEventKey = "91D1BD0E-2AD2-4930-A98C-9CDE8793907D";

        [MenuItem("GameObject/Effects/Particle Profiler", false, 1000)]
        static private void Test()
        {
            if(EditorApplication.isPlaying)
                return;

            EditorApplication.isPlaying = true;

            EditorPrefs.SetBool(ProfilingEventKey, true);
        }

        static ProfilingEntry()
        {
            EditorApplication.playModeStateChanged += PlayModeState;
        }

        private static void PlayModeState(PlayModeStateChange state)
        {
            if(!EditorPrefs.GetBool(ProfilingEventKey))
                return;

            EditorPrefs.DeleteKey(ProfilingEventKey);

            if(state != PlayModeStateChange.EnteredPlayMode)
                return;
                
            ParticleProfiler.ProfilerData profiler;
            ShowOverdraw.OverdrawData overdraw;
            BeginTest(Selection.activeGameObject, out profiler, out overdraw);
        }

        static public GameObject BeginTest(GameObject prefab, out ParticleProfiler.ProfilerData profilerData, out ShowOverdraw.OverdrawData overdrawData)
        {
            profilerData = null;
            overdrawData = null;

            if(prefab.GetComponentsInChildren<ParticleSystemRenderer>(true).Length == 0 &&
            prefab.GetComponentsInChildren<FX_Component>(true).Length == 0)
            {
                Debug.LogWarning("Not found any ParticleSystem");
                return null;
            }

            GameObject inst = prefab;
            if(EditorUtility.IsPersistent(prefab))
            {
                inst = Object.Instantiate(prefab);
            }

            ParticleProfiler particleProfiler = inst.GetComponent<ParticleProfiler>();
            if(particleProfiler == null)
            {
                particleProfiler = inst.AddComponent<ParticleProfiler>();
                profilerData = particleProfiler.m_Data;
            }

            ShowOverdraw overdraw = inst.GetComponent<ShowOverdraw>();
            if(overdraw == null)
            {
                overdraw = inst.AddComponent<ShowOverdraw>();
                overdrawData = overdraw.m_Data;
            }
            return inst;
        }
    }
}
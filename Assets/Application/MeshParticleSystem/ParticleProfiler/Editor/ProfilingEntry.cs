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

        [MenuItem("GameObject/特效/Particle Profiler", false, 1000)]
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
            // Debug.Log($"{state}     {EditorApplication.isPlaying}");

            if(!EditorPrefs.GetBool(ProfilingEventKey))
                return;

            EditorPrefs.DeleteKey(ProfilingEventKey);

            if(state != PlayModeStateChange.EnteredPlayMode)
                return;
                
            GameObject ps = Selection.activeGameObject;
            var particleSystemRenderer = ps.GetComponentsInChildren<ParticleSystemRenderer>(true);
            if(particleSystemRenderer.Length == 0)
            {
                Debug.LogWarning("Not found any ParticleSystem");
                return;
            }

            if(ps.GetComponent<ShowOverdraw>() == null)
                ps.AddComponent<ShowOverdraw>();
            if(ps.GetComponent<ParticleProfiler>() == null)
                ps.AddComponent<ParticleProfiler>();
        }
    }
}
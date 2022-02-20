using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using System;
using System.Diagnostics;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    public class AsyncLoaderManager : SingletonMono<AsyncLoaderManager>
    {
        struct LoaderTrack
        {
            public string                               assetPath;
            public PrefabLoaderAsync                    prefabLoader;
            public AssetLoaderAsync<UnityEngine.Object> assetLoader;
            public Action<GameObject, System.Object>    actionsForPrefabLoader;
            public Action<UnityEngine.Object, System.Object> actionsForAssetLoader;
            public System.Object                        userData;            
        }
        private LinkedList<LoaderTrack> m_Requests      = new LinkedList<LoaderTrack>();

        private Stopwatch               m_sw            = new Stopwatch();
        private const int               m_MinTimeSlice  = 1;
        private const int               m_MaxTimeSlice  = 15;
        private int                     m_TimeSlice     = 8;

        public int timeSlice
        {
            get { return m_TimeSlice; }
            set
            {
                m_TimeSlice = Mathf.Clamp(m_TimeSlice, m_MinTimeSlice, m_MaxTimeSlice);
            }
        }

        private void Update()
        {
            if(m_Requests.Count == 0)
                return;

            m_sw.Restart();
            while(true)
            {
                while(m_Requests.Count > 0)
                {
                    if(m_sw.ElapsedMilliseconds > m_TimeSlice)
                    {
                        // UnityEngine.Debug.LogWarning($"============   {m_sw.ElapsedMilliseconds}    {m_sw.Elapsed.Milliseconds}");
                        return;     // 一帧最多分配固定时间片用于加载资源
                    }

                    LoaderTrack track = m_Requests.First.Value;
                    if(track.prefabLoader != null && !track.prefabLoader.MoveNext())
                    { // complete
                        try
                        {
                            track.actionsForPrefabLoader?.Invoke(track.prefabLoader.asset, track.userData);      // asset可能为null，当资源加载失败时
                        }
                        catch(Exception e)
                        {
                            UnityEngine.Debug.LogError(e.Message);
                        }
                        m_Requests.RemoveFirst();
                    }
                    if(track.assetLoader != null && !track.assetLoader.MoveNext())
                    {
                        try
                        {
                            track.actionsForAssetLoader?.Invoke(track.assetLoader.asset, track.userData);
                        }
                        catch(Exception e)
                        {
                            UnityEngine.Debug.LogError(e.Message);
                        }
                        m_Requests.RemoveFirst();
                    }
                }
                break;
            }
        }

        public void AsyncLoadPrefab(string assetPath, Action<GameObject, System.Object> cb, System.Object userData = null)
        {
            UnityEngine.Debug.Assert(Instance != null);
            UnityEngine.Debug.Assert(cb != null);

            LoaderTrack track = new LoaderTrack();
            track.assetPath = assetPath;
            track.prefabLoader = AssetManager.InstantiatePrefabAsync(assetPath);
            track.assetLoader = null;
            track.actionsForPrefabLoader = cb;
            track.actionsForAssetLoader = null;
            track.userData = userData;
            m_Requests.AddLast(track);
        }

        public AssetLoaderAsync<UnityEngine.Object> AsyncLoadAsset(string assetPath, Action<UnityEngine.Object, System.Object> cb, System.Object userData = null)
        {
            UnityEngine.Debug.Assert(Instance != null);
            UnityEngine.Debug.Assert(cb != null);

            LoaderTrack track = new LoaderTrack();
            track.assetPath = assetPath;
            track.prefabLoader = null;
            track.assetLoader = AssetManager.LoadAssetAsync<UnityEngine.Object>(assetPath);
            track.actionsForPrefabLoader = null;
            track.actionsForAssetLoader = cb;
            track.userData = userData;
            m_Requests.AddLast(track);
            return track.assetLoader;
        }
    }
}
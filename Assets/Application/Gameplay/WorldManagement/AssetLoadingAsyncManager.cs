using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Core;
using System;

namespace Application.Runtime
{
    public class AssetLoadingAsyncManager : SingletonMono<AssetLoadingAsyncManager>
    {
        static private int      s_Id = 0;
        static private Dictionary<int, List<AssetRequest>> s_AssetLoadingList = new Dictionary<int, List<AssetRequest>>();

        static public void AddInstance(ILoader loader)
        {
            int id = s_Id++;
#if UNITY_EDITOR

            if(s_AssetLoadingList.ContainsKey(id))
                throw new System.ArgumentException($"AssetLoadingAsyncManager.AddInstance: the ILoader {id} has exist");
#endif

            loader.loaderId = id;
            s_AssetLoadingList.Add(id, null);
        }

        static public void RemoveInstance(ILoader loader)
        {
#if UNITY_EDITOR
            if(!s_AssetLoadingList.ContainsKey(loader.loaderId))
                throw new System.ArgumentException($"AssetLoadingAsyncManager.RemoveInstance: the ILoader {loader.loaderId} has not exit");
#endif
            s_AssetLoadingList.Remove(loader.loaderId);
        }

        static public void SendAysncLoading(int loaderId, AssetRequest request)
        {
            List<AssetRequest> requests;
            if(!s_AssetLoadingList.TryGetValue(loaderId, out requests))
            {
                throw new System.ArgumentException($"can't find ILoader {loaderId}");
            }

            if(requests == null)
            {
                requests = new List<AssetRequest>();
            }
            requests.Add(request);
        }

        void Update()
        {
            
        }
    }

    public class AssetRequest
    {
        public IEnumerator  func;
        public Action       onFinished;
        public AssetRequest(IEnumerator func, Action onFinished)
        {
            this.func = func;
            this.onFinished = onFinished;
        }
    }
}
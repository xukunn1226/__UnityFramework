using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class AssetOperationHandle : OperationHandleBase
    {
        private System.Action<AssetOperationHandle> m_Callback;

        internal AssetOperationHandle(ProviderBase provider) : base(provider)
        { }

        public void Release()
        {
            this.ReleaseInternal();
        }

        internal override void InvokeCallback()
        {
            m_Callback?.Invoke(this);
        }

        public Object assetObject { get { return isValid ? provider.assetObject : null; } }

        public event System.Action<AssetOperationHandle> Completed
        {
            add
            {
                if (!isValid)
                    throw new System.Exception($"AssetOperationHandle is invalid");
                
                if (provider.isDone)
                    value.Invoke(this);
                else
                    m_Callback += value;
            }
            remove
            {
                if (!isValid)
                    throw new System.Exception($"AssetOperationHandle is invalid");
                m_Callback -= value;
            }
        }

        public void WaitForAsyncComplete()
        {
            if (!isValid)
                return;
            provider.WaitForAsyncComplete();
        }

        public GameObject Instantiate(Transform parent = null)
        {
            return InstantiateSyncInternal(Vector3.zero, Quaternion.identity, parent);
        }

        public GameObject Instantiate(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return InstantiateSyncInternal(position, rotation, parent);
        }

        private GameObject InstantiateSyncInternal(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (!isValid || provider.assetObject == null)
                return null;
            return UnityEngine.Object.Instantiate(provider.assetObject as GameObject, position, rotation, parent);
        }
    }
}
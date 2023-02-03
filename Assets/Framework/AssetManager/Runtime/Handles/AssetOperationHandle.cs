using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class AssetOperationHandle : OperationHandleBase
    {
        private GameObject m_Inst;
        private System.Action<AssetOperationHandle> m_Callback;

        internal AssetOperationHandle(ProviderBase provider) : base(provider)
        { }

        public void Release()
        {
            if (m_Inst != null)
            {
                Object.Destroy(m_Inst);
                m_Inst = null;
            }
            ClearCallback();
            this.ReleaseInternal();
        }

        internal override void InvokeCallback()
        {
            m_Callback?.Invoke(this);
        }

        public void ClearCallback()
        {
            m_Callback = null;
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
            m_Inst = InstantiateSyncInternal(parent);
            return m_Inst;
        }

        public GameObject Instantiate(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            m_Inst = InstantiateSyncInternal(position, rotation, parent);
            return m_Inst;
        }

        private GameObject InstantiateSyncInternal(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (!isValid || provider.assetObject == null)
                return null;
            return UnityEngine.Object.Instantiate(provider.assetObject as GameObject, position, rotation, parent);
        }

        private GameObject InstantiateSyncInternal(Transform parent)
        {
            if (!isValid || provider.assetObject == null)
                return null;
            return UnityEngine.Object.Instantiate(provider.assetObject as GameObject, parent);
        }
    }
}
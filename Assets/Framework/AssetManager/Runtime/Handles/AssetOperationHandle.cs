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
            this.ReleaseInternal();
        }

        internal override void InvokeCallback()
        {
            m_Callback?.Invoke(this);
        }

        internal override void ClearCallback()
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
            m_Inst = InstantiateSyncInternal(Vector3.zero, Quaternion.identity, parent);
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

        public InstantiateOperation InstantiateAsync(Transform parent = null)
        {
            return InstantiateAsyncInternal(Vector3.zero, Quaternion.identity, parent);
        }

        public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return InstantiateAsyncInternal(position, rotation, parent);
        }

        private InstantiateOperation InstantiateAsyncInternal(Vector3 position, Quaternion rotation, Transform parent)
        {
            InstantiateOperation operation = new InstantiateOperation(this, position, rotation, parent);
            AsyncOperationSystem.StartOperation(operation);
            return operation;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// 资源提供者
    /// </summary>
    public abstract class ProviderBase
    {
        public AssetSystem      assetSystem     { get; private set; }
        public string           providerGUID    { get; private set; }
        public AssetInfo        assetInfo       { get; private set; }
        public Object           assetObject     { get; private set; }
        public Scene            sceneObject     { get; private set; }
        public EProviderStatus  status          { get; protected set; } = EProviderStatus.None;
        public string           lastError       { get; private set; }
        public float            progress        { get; protected set; }
        public int              refCount        { get; protected set; }
        public bool             isDestroyed     { get; private set; }
        public bool             isDone          { get { return status == EProviderStatus.Succeed || status == EProviderStatus.Failed; } }
        public bool             canDestroy      { get { return isDone ? refCount <= 0 : false; } }
        public string           bundlePath      { get; protected set; }     // 不同的provider，bundlePath对应的含义不一样

        protected bool          m_RequestAsyncComplete;
        private Dictionary<int, OperationHandleBase> m_Handlers = new Dictionary<int, OperationHandleBase>();

        protected ProviderBase() { }
        public ProviderBase(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo)
        {
            this.assetSystem = assetSystem;
            this.providerGUID = providerGUID;
            this.assetInfo = assetInfo;
        }

        public bool IsSceneProvider()
        {
            return true;
        }

        public abstract void Update();

        public virtual void Destroy()
        {
            isDestroyed = true;
        }

        public T CreateHandle<T>() where T : OperationHandleBase
        {
            ++refCount;

            OperationHandleBase handle = null;
            if(typeof(T) == typeof(AssetOperationHandle))
            {
                handle = new AssetOperationHandle(this);
            }
            else
            {
                throw new System.NotImplementedException();
            }

            m_Handlers.Add(handle.id, handle);
            return (T)handle;
        }

        public void ReleaseHandle(OperationHandleBase handle)
        {
            if (refCount <= 0)
                Debug.LogWarning($"Asset provider ref count is already less than zero.");

            if (!m_Handlers.Remove(handle.id))
                throw new System.Exception($"How to get here!");

            --refCount;
        }

        public void WaitForAsyncComplete()
        {
            m_RequestAsyncComplete = true;

            Update();

            if (!isDone)
                Debug.LogWarning($"WaitForAsyncComplete failed to loading: {assetInfo.assetPath}");
        }

        protected void InvokeCompletion()
        {
            progress = 1;

            // TODO: 思考如何防止外部逻辑在回调中创建或释放资源句柄
            foreach(var handler in m_Handlers)
            {
                if (handler.Value.isValid)
                {
                    handler.Value.InvokeCallback();
                }
            }

            if(m_TaskCompletionSource != null)
            {
                m_TaskCompletionSource.TrySetResult(null);
            }
        }

        public Task Task
        {
            get
            {
                if (m_TaskCompletionSource == null)
                {
                    m_TaskCompletionSource = new TaskCompletionSource<object>();
                    if (isDone)
                        m_TaskCompletionSource.SetResult(null);
                }
                return m_TaskCompletionSource.Task;
            }
        }

        private TaskCompletionSource<object> m_TaskCompletionSource;
    }
}
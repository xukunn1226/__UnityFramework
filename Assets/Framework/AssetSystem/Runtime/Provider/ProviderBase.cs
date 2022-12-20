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
    internal abstract class ProviderBase
    {
        public AssetSystem      assetSystem             { get; private set; }
        public string           providerGUID            { get; private set; }       // 资源提供者唯一标识符
        public AssetInfo        assetInfo               { get; private set; }
        public Object           assetObject             { get; protected set; }
        public Scene            sceneObject             { get; protected set; }
        public EProviderStatus  status                  { get; protected set; } = EProviderStatus.None;
        public string           lastError               { get; protected set; }
        public float            progress                { get; protected set; }
        public int              refCount                { get; protected set; }
        public bool             isDestroyed             { get; private set; }
        public bool             isDone                  { get { return status == EProviderStatus.Succeed || status == EProviderStatus.Failed; } }
        public bool             canDestroy              { get { return isDone ? refCount <= 0 : false; } }
        public string           rawFilePath             { get; protected set; }     // 原生文件路径
        protected bool          requestAsyncComplete    { get; private set; }
        static private bool     s_Guarded;                                          // 防止外部逻辑在回调中创建或释放句柄

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
            if (this is BundleSceneProvider || this is DatabaseSceneProvider)
                return true;
            else
                return false;
        }

        public abstract void Update();

        public virtual void Destroy()
        {
            isDestroyed = true;
        }

        public T CreateHandle<T>() where T : OperationHandleBase
        {
            if (s_Guarded)
                throw new System.Exception($"Should never get here, can't CreateHandle from InvokeCallback");

            ++refCount;

            OperationHandleBase handle = null;            
            if (typeof(T) == typeof(AssetOperationHandle))
                handle = new AssetOperationHandle(this);
            else if (typeof(T) == typeof(SceneOperationHandle))
                handle = new SceneOperationHandle(this);
            else if (typeof(T) == typeof(RawFileOperationHandle))
                handle = new RawFileOperationHandle(this);
            else
                throw new System.NotImplementedException();

            m_Handlers.Add(handle.id, handle);
            return (T)handle;
        }

        public void ReleaseHandle(OperationHandleBase handle)
        {
            if (s_Guarded)
                throw new System.Exception($"Should never get here, can't ReleaseHandle from InvokeCallback");

            if (refCount <= 0)
                Debug.LogWarning($"Asset provider ref count is already less than zero.");

            if (!m_Handlers.Remove(handle.id))
                throw new System.Exception($"How to get here!");

            --refCount;
        }

        public virtual void WaitForAsyncComplete()
        {
            requestAsyncComplete = true;

            Update();

            if (!isDone)
                Debug.LogWarning($"WaitForAsyncComplete failed to loading: {assetInfo.assetPath}");
        }

        protected void InvokeCompletion()
        {
            progress = 1;

            foreach(var handler in m_Handlers)
            {
                if (handler.Value.isValid)
                {
                    s_Guarded = true;
                    handler.Value.InvokeCallback();
                    s_Guarded = false;
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
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
        public Object[]         allAssetObjects         { get; protected set; }
        public Scene            sceneObject             { get; protected set; }
        public GameObject       gameObject              { get; protected set; }
        public Transform        parent                  { get; protected set; }
        public Vector3          position                { get; protected set; }
        public Quaternion       rotation                { get; protected set; }
        public EProviderStatus  wasStatus               { get; protected set; } = EProviderStatus.None;     // 上一帧的状态
        public EProviderStatus  status                  { get { return m_Status; } protected set { wasStatus = m_Status; m_Status = value; } }
        private EProviderStatus m_Status                = EProviderStatus.None;
        public string           lastError               { get; protected set; }
        public float            progress                { get; protected set; }
        public int              refCount                { get; protected set; }
        public bool             isDestroyed             { get; private set; }
        public bool             isDone                  { get { return status == EProviderStatus.Succeed || status == EProviderStatus.Failed; } }
        public bool             canDestroy              { get { return isDone ? refCount <= 0 : false; } }
        public string           rawFilePath             { get; protected set; }     // 原生文件路径
        protected bool          requestAsyncComplete    { get; private set; }
        public bool             isTriggerLoadingRequest { get; protected set; }     // 当前帧是否触发了加载请求

        private IndexedSet<OperationHandleBase> m_Handlers = new IndexedSet<OperationHandleBase>();

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

        /// <summary>
        /// 创建句柄，除了PrefabOperationHandle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        internal T CreateHandle<T>() where T : OperationHandleBase
        {
            ++refCount;

            OperationHandleBase handle = null;
            if (typeof(T) == typeof(AssetOperationHandle))
                handle = new AssetOperationHandle(this);
            else if (typeof(T) == typeof(SceneOperationHandle))
                handle = new SceneOperationHandle(this);
            else if (typeof(T) == typeof(RawFileOperationHandle))
                handle = new RawFileOperationHandle(this);
            else if (typeof(T) == typeof(SubAssetsOperationHandle))
                handle = new SubAssetsOperationHandle(this);
            else
                throw new System.NotImplementedException();

            m_Handlers.AddUnique(handle);
            AddDebugStackTrace(handle);

            ConditionalResetStatus();
            
            return (T)handle;
        }

        /// <summary>
        /// 创建适配PrefabOperationHandle的句柄
        /// </summary>
        /// <returns></returns>
        internal PrefabOperationHandle CreateHandle(Transform parent, Vector3 pos, Quaternion rot)
        {
            ++refCount;

            PrefabOperationHandle handle = new PrefabOperationHandle(this);
            m_Handlers.AddUnique(handle);
            AddDebugStackTrace(handle);
            this.parent = parent;
            this.position = pos;
            this.rotation = rot;

            ConditionalResetStatus();

            return handle;
        }

        /// <summary>
        /// 在资源已加载成功情况下，如有其他加载请求则重置状态，强制沦陷一遍以达到下一帧触发回调的效果（模拟异步）
        /// </summary>
        protected virtual void ConditionalResetStatus()
        {            
            if(status == EProviderStatus.Succeed)
            {
                status = EProviderStatus.None;
            }
        }

        public void ReleaseHandle(OperationHandleBase handle)
        {
            if (refCount <= 0)
                Debug.LogWarning($"Asset provider ref count is already less than zero.");

            if(!m_Handlers.Remove(handle))
                throw new System.Exception($"How to get here!");

            RemoveDebugStackTrace(handle);
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

            for(int i = m_Handlers.Count - 1; i >= 0; --i)
            {
                OperationHandleBase handle = m_Handlers[i];
                if(handle.isValid)
                {
                    try
                    {
                        handle.InvokeCallback();
                        handle.ClearCallback();     // 触发完回调，自动回收cb
                    }
                    catch(System.Exception e)
                    {
                        Debug.LogException(e);
                    }
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



        public string   spawnScene;
        public string   spawnTime;
        public long     loadingTime;

        [System.Diagnostics.Conditional("DEBUG")]
        public void InitSpawnDebugInfo()
        {
            spawnScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            spawnTime = SpawnTimeToString(UnityEngine.Time.realtimeSinceStartup);
        }

        private string SpawnTimeToString(float spawnTime)
        {
            float h = UnityEngine.Mathf.FloorToInt(spawnTime / 3600f);
            float m = UnityEngine.Mathf.FloorToInt(spawnTime / 60f - h * 60f);
            float s = UnityEngine.Mathf.FloorToInt(spawnTime - m * 60f - h * 3600f);
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        private bool                            m_isRecording;
        private System.Diagnostics.Stopwatch    m_LoadingTimeWatch;

        [System.Diagnostics.Conditional("DEBUG")]
        protected void DebugLoadingTime()
        {
            if (m_isRecording == false)
            {
                m_isRecording = true;
                m_LoadingTimeWatch = System.Diagnostics.Stopwatch.StartNew();
            }

            if (m_LoadingTimeWatch != null)
            {
                if (isDone)
                {
                    loadingTime = m_LoadingTimeWatch.ElapsedMilliseconds;
                    m_LoadingTimeWatch = null;
                }
            }
        }

        private Dictionary<OperationHandleBase, string> m_StackTraces;
        
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        protected void AddDebugStackTrace(OperationHandleBase handle)
        {
            if (m_StackTraces == null)
                m_StackTraces = new Dictionary<OperationHandleBase, string>();
            m_StackTraces.Add(handle, GetStackTrace());
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        protected void RemoveDebugStackTrace(OperationHandleBase handle)
        {
            if (m_StackTraces == null)
                return;
            m_StackTraces.Remove(handle);
        }

        static public string GetStackTrace()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            var stackTrace = new System.Diagnostics.StackTrace(true);
            string filterdName = "ResponseWrite,ResponseWriteError,";
            for(int i = stackTrace.GetFrames().Length - 1; i >= 1; --i)
            {
                var frame = stackTrace.GetFrame(i);
                if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == frame.GetILOffset())
                    break;
                
                string methodName = frame.GetMethod().Name;
                if (filterdName.Contains(methodName))
                    continue;

                sb.AppendLine($"{methodName} (at file: {frame.GetFileName()} line: {frame.GetFileLineNumber()} column: {frame.GetFileColumnNumber()})");
            }

            return sb.ToString();
        }

        private List<string> GetStackTraceInfos()
        {
            List<string> infos = new List<string>();
            if (m_StackTraces == null)
                return infos;

            foreach(var pair in m_StackTraces)
            {
                infos.Add(pair.Value);
            }
            return infos;
        }
        
        [System.Diagnostics.Conditional("DEBUG")]
        public void GetProviderDebugInfo(ref DebugProviderInfo info)
        {
            info.AssetPath          = assetInfo.assetPath;
            info.SpawnScene         = spawnScene;
            info.SpawnTime          = spawnTime;
            info.LoadingTime        = loadingTime;
            info.RefCount           = refCount;
            info.Status             = status.ToString();
            info.DependBundleInfos  = new List<DebugBundleInfo>();
            info.StackTraces        = GetStackTraceInfos();

            if(this is BundleProvider)
            {
                ((BundleProvider)this).GetBundleDebugInfos(info.DependBundleInfos);
            }
        }
    }
}
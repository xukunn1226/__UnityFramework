using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
    /// 操作句柄
    /// 功能：获取各种加载状态及数据，对Provider的封装，隐藏上层对Provider的感知
    /// </summary>
    public abstract class OperationHandleBase : IEnumerator
    {
        public bool isValid
        {
            get
            {
                if (provider != null && !provider.isDestroyed)
                {
                    return true;
                }
                else
                {
                    if (provider == null)
                    {
                        Debug.LogWarning($"Operation handle is release: {assetInfo.assetPath}");
                    }
                    else if (provider.isDestroyed)
                    {
                        Debug.LogWarning($"Provider is destroyed: {assetInfo.assetPath}");
                    }
                    return false;
                }
            }
        }

        internal int            id          { get; private set; }
        internal ProviderBase   provider    { get; private set; }
        public AssetInfo        assetInfo   { get; private set; }
        public bool             isDone      { get { return isValid ? provider.isDone : false; } }
        public float            progress    { get { return isValid ? provider.progress : 0; } }
        public string           lastError   { get { return isValid ? provider.lastError : string.Empty; } }
        
        public EOperationStatus status
        { 
            get 
            {
                if (!isValid)
                    return EOperationStatus.None;
                if (provider.status == EProviderStatus.Succeed)
                    return EOperationStatus.Succeed;
                else if (provider.status == EProviderStatus.Failed)
                    return EOperationStatus.Failed;
                else
                    return EOperationStatus.None;
            } 
        }

        internal OperationHandleBase(ProviderBase provider)
        {
            this.id = MakeUniqueID();
            this.provider = provider;
            assetInfo = provider.assetInfo;
        }

        internal abstract void InvokeCallback();

        static private int s_HanderID = 0;
        static private int MakeUniqueID()
        {
            return s_HanderID++;
        }

        internal void ReleaseInternal()
        {
            if (!isValid)
                return;
            provider.ReleaseHandle(this);
            provider = null;
        }

        #region 异步操作相关
        /// <summary>
        /// 异步操作任务
        /// </summary>
        public System.Threading.Tasks.Task Task
        {
            get { return provider.Task; }
        }

        // 协程相关
        bool IEnumerator.MoveNext()
        {
#if UNITY_EDITOR
            return !isDone;                                     // 异步结束前如果执行了provider.Destroy，将导致协程无法退出
#else
            return provider != null && !provider.isDone;        // 真机模式下更安全
#endif
        }
        void IEnumerator.Reset()
        {
        }
        object IEnumerator.Current
        {
            get { return provider; }
        }
#endregion
    }
}
using System.Collections;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
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

        internal ProviderBase   provider    { get; private set; }
        public AssetInfo        assetInfo   { get; private set; }
        public bool             isDone      { get { return isValid ? provider.isDone : false; } }
        public float            progress    { get { return isValid ? provider.progress : 0; } }
        public string           lastError   { get { return isValid ? provider.lastError : string.Empty; } }

        internal OperationHandleBase(ProviderBase provider)
        {
            this.provider = provider;
            assetInfo = provider.assetInfo;
        }

        public abstract void InvokeCallback();

        protected void ReleaseInternal()
        {
            if (!isValid)
                return;
            provider.ReleaseHandle(this);
            provider = null;
        }

        #region �첽�������
        /// <summary>
        /// �첽��������
        /// </summary>
        public System.Threading.Tasks.Task Task
        {
            get { return provider.Task; }
        }

        // Э�����
        bool IEnumerator.MoveNext()
        {
            return !isDone;
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
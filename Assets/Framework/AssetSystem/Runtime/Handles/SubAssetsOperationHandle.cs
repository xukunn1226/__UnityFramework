using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public sealed class SubAssetsOperationHandle : OperationHandleBase
    {
        private System.Action<SubAssetsOperationHandle> m_Callback;

        internal SubAssetsOperationHandle(ProviderBase provider) : base(provider)
        { }

        public void Release()
        {
            ReleaseInternal();
        }

        internal override void InvokeCallback()
        {
            m_Callback?.Invoke(this);
        }

        public UnityEngine.Object[] allAssetObjects { get { return isValid ? provider.allAssetObjects : null; } }

        public event System.Action<SubAssetsOperationHandle> Completed
        {
            add
            {
                if (!isValid)
                    throw new System.Exception($"SubAssetsOperationHandle is invalid");

                if (provider.isDone)
                    value.Invoke(this);
                else
                    m_Callback += value;
            }
            remove
            {
                if (!isValid)
                    throw new System.Exception($"SubAssetsOperationHandle is invalid");
                m_Callback -= value;
            }
        }

        public void WaitForAsyncComplete()
        {
            if (!isValid)
                return;
            provider.WaitForAsyncComplete();
        }

        /// <summary>
		/// 获取子资源对象
		/// </summary>
		/// <typeparam name="TObject">子资源对象类型</typeparam>
		/// <param name="assetName">子资源对象名称</param>
		public TObject GetSubAssetObject<TObject>(string assetName) where TObject : UnityEngine.Object
        {
            if (!isValid)
                return null;

            foreach (var assetObject in provider.allAssetObjects)
            {
                if (assetObject.name == assetName)
                    return assetObject as TObject;
            }

            Debug.LogWarning($"Not found sub asset object : {assetName}");
            return null;
        }

        /// <summary>
        /// 获取所有的子资源对象集合
        /// </summary>
        /// <typeparam name="TObject">子资源对象类型</typeparam>
        public TObject[] GetSubAssetObjects<TObject>() where TObject : UnityEngine.Object
        {
            if (!isValid)
                return null;

            List<TObject> ret = new List<TObject>(provider.allAssetObjects.Length);
            foreach (var assetObject in provider.allAssetObjects)
            {
                var retObject = assetObject as TObject;
                if (retObject != null)
                    ret.Add(retObject);
                else
                    Debug.LogWarning($"The type conversion failed : {assetObject.name}");
            }
            return ret.ToArray();
        }
    }
}
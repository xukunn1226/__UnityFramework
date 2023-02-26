using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    internal sealed class DatabasePrefabProvider : ProviderBase
    {
        private int m_DelayedFrameCount;

        public DatabasePrefabProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo) : base(assetSystem, providerGUID, assetInfo)
        {
            m_DelayedFrameCount = Mathf.Max(1, AssetManagerSettings.DelayedFrameNumInEditorSimulateMode);
        }

        public override void Update()
        {
#if UNITY_EDITOR
            if (isDone)
                return;

            if (status == EProviderStatus.None)
            {
                // 检测资源文件是否存在
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetInfo.assetPath);
                if (string.IsNullOrEmpty(guid))
                {
                    status = EProviderStatus.Failed;
                    lastError = $"Not found asset : {assetInfo.assetPath}";
                    Debug.LogError(lastError);
                    InvokeCompletion();
                    return;
                }

                if (requestAsyncComplete)
                { // 立即结束异步模拟，进入下个流程
                    status = EProviderStatus.Loading;
                }
                else
                { // 模拟延迟几帧加载
                    if (m_DelayedFrameCount <= 0)
                    {
                        status = EProviderStatus.Loading;
                    }
                    else
                    {
                        --m_DelayedFrameCount;
                        return;
                    }
                }
            }

            // 1. 加载资源对象
            if (status == EProviderStatus.Loading)
            {
                if (assetInfo.assetType == null)
                    assetObject = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetInfo.assetPath);
                else
                    assetObject = UnityEditor.AssetDatabase.LoadAssetAtPath(assetInfo.assetPath, assetInfo.assetType);
                status = EProviderStatus.Checking;
            }

            // 2. 检测加载结果
            if (status == EProviderStatus.Checking)
            {
                if(assetObject == null)
                {
                    status = EProviderStatus.Failed;
                    if (assetInfo.assetType == null)
                        lastError = $"Failed to load prefab object : {assetInfo.assetPath} AssetType : null";
                    else
                        lastError = $"Failed to load prefab object : {assetInfo.assetPath} AssetType : {assetInfo.assetType}";
                    InvokeCompletion();
                    return;
                }
                status = EProviderStatus.Instantiate;
            }

            // 3. 实例化
            if(status == EProviderStatus.Instantiate)
            {
                gameObject = UnityEngine.Object.Instantiate<GameObject>(assetObject as GameObject, position, rotation, parent);

                status = gameObject != null ? EProviderStatus.Succeed : EProviderStatus.Failed;
                if (status == EProviderStatus.Failed)
                {
                    lastError = $"Failed to instantiate asset: {assetInfo.assetPath} AssetType : {assetInfo.assetType}";
                    Debug.LogError(lastError);
                }
                InvokeCompletion();
            }
#endif
        }
    }
}
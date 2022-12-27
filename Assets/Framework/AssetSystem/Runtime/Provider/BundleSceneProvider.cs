using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
    internal sealed class BundleSceneProvider : BundleProvider
    {
        public LoadSceneMode    sceneMode       { get; private set; }
        public string           sceneName       { get; private set; }
        public bool             activateOnLoad  { get; private set; }
        public int              priority        { get; private set; }
        private AsyncOperation  m_AsyncOp;

#pragma warning disable CS0628 // ���ܷ��������������µı�����Ա
        protected BundleSceneProvider() { }
#pragma warning restore CS0628 // ���ܷ��������������µı�����Ա
        public BundleSceneProvider(AssetSystem assetSystem, string providerGUID, AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad, int priority) : base(assetSystem, providerGUID, assetInfo)
        {
            this.sceneMode = sceneMode;
            this.sceneName = System.IO.Path.GetFileNameWithoutExtension(assetInfo.assetPath);
            this.activateOnLoad = activateOnLoad;
            this.priority = priority;
        }

        public override void WaitForAsyncComplete()
        {
            throw new System.Exception($"Unsupport to sync completion: {sceneName}");
        }

        public override void Update()
        {
            if (isDone)
                return;

            // ��ʼ
            if (status == EProviderStatus.None)
                status = EProviderStatus.CheckBundle;

            // �����Դ��
            if(status == EProviderStatus.CheckBundle)
            {
                // �ȴ���Դ���������
                if (!mainBundleLoader.isDone)
                    return;
                if (dependBundleLoader != null && !dependBundleLoader.IsDone())
                    return;

                if (dependBundleLoader != null && !dependBundleLoader.IsSucceed())
                {
                    status = EProviderStatus.Failed;
                    lastError = dependBundleLoader.GetLastError();
                    InvokeCompletion();
                    return;
                }

                if (mainBundleLoader.status != EBundleLoadStatus.Succeed)
                {
                    status = EProviderStatus.Failed;
                    lastError = mainBundleLoader.lastError;
                    InvokeCompletion();
                    return;
                }

                status = EProviderStatus.Loading;
            }

            // ���س���
            if(status == EProviderStatus.Loading)
            {
                // ���������ڷ���NULL
                m_AsyncOp = SceneManager.LoadSceneAsync(sceneName, sceneMode);
                if(m_AsyncOp != null)
                {
                    m_AsyncOp.allowSceneActivation = true;
                    m_AsyncOp.priority = priority;
                    sceneObject = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);         // TODO: ��̬���س��������buildsetting list��
                    status = EProviderStatus.Checking;
                }
                else
                {
                    status = EProviderStatus.Failed;
                    lastError = $"Failed to load scene: {assetInfo.assetPath}";
                    Debug.LogError(lastError);
                    InvokeCompletion();
                }
            }

            if(status == EProviderStatus.Checking)
            {
                progress = m_AsyncOp.progress;
                if(m_AsyncOp.isDone)
                {
                    if (sceneObject.IsValid() && activateOnLoad)
                        SceneManager.SetActiveScene(sceneObject);

                    status = sceneObject.IsValid() ? EProviderStatus.Succeed : EProviderStatus.Failed;
                    if (status == EProviderStatus.Failed)
                    {
                        lastError = $"The load scene is invalid : {assetInfo.assetPath}";
                        Debug.LogError(lastError);
                    }
                    InvokeCompletion();
                }
            }
        }
    }
}
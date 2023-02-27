using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
    public class SceneOperationHandle : OperationHandleBase
    {
        private System.Action<SceneOperationHandle> m_Callback;

        internal SceneOperationHandle(ProviderBase provider) : base(provider)
        { }

        internal override void InvokeCallback()
        {
            m_Callback?.Invoke(this);
        }

        public Scene sceneObject { get { return isValid ? provider.sceneObject : default(Scene); } }

        public event System.Action<SceneOperationHandle> Completed
        {
            add
            {
                if (!isValid)
                    throw new System.Exception($"SceneOperationHandle is invalid");

                if (provider.isDone)
                    value.Invoke(this);
                else
                    m_Callback += value;
            }
            remove
            {
                if (!isValid)
                    throw new System.Exception($"SceneOperationHandle is invalid");
                m_Callback -= value;
            }
        }
        
        internal override void ClearCallback()
        {
            m_Callback = null;
        }

        public bool ActivateScene()
        {
            if (!isValid)
                return false;

            if (sceneObject.IsValid() && sceneObject.isLoaded)
            {
                return SceneManager.SetActiveScene(sceneObject);
            }
            else
            {
                Debug.LogWarning($"Scene is invalid or not loaded : {sceneObject.name}");
                return false;
            }
        }

        public bool IsMainScene()
        {
            if (!isValid)
                return false;

            if (provider is DatabaseSceneProvider)
            {
                var temp = provider as DatabaseSceneProvider;
                return temp.sceneMode == LoadSceneMode.Single;
            }
            else if (provider is BundleSceneProvider)
            {
                var temp = provider as BundleSceneProvider;
                return temp.sceneMode == LoadSceneMode.Single;
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        /// <summary>
        /// 异步卸载子场景
        /// </summary>
        /// <returns></returns>
        public UnloadSceneOperation UnloadAsync()
        {
            // 如果句柄无效
            if (!isValid)
            {
                string error = $"{nameof(SceneOperationHandle)} is invalid.";
                var operation = new UnloadSceneOperation(error);
                AsyncOperationSystem.StartOperation(operation);
                return operation;
            }

            // 如果是主场景
            if (IsMainScene())
            {
                string error = $"Cannot unload main scene. Use {nameof(AssetManagerEx.LoadSceneAsync)} method to change the main scene !";
                Debug.LogError(error);
                var operation = new UnloadSceneOperation(error);
                AsyncOperationSystem.StartOperation(operation);
                return operation;
            }

            // 卸载子场景
            Scene sceneObj = sceneObject;       // 这里复制一份数据，UnloadSubScene将进行数据清理，使得sceneObject失效
            provider.assetSystem.UnloadSubScene(provider);
            {                
                var operation = new UnloadSceneOperation(sceneObj);
                AsyncOperationSystem.StartOperation(operation);
                return operation;
            }
        }
    }
}
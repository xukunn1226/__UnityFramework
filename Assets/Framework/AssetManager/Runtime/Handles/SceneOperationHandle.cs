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
        /// �첽ж���ӳ���
        /// </summary>
        /// <returns></returns>
        public UnloadSceneOperation UnloadAsync()
        {
            // ��������Ч
            if (!isValid)
            {
                string error = $"{nameof(SceneOperationHandle)} is invalid.";
                var operation = new UnloadSceneOperation(error);
                AsyncOperationSystem.StartOperation(operation);
                return operation;
            }

            // �����������
            if (IsMainScene())
            {
                string error = $"Cannot unload main scene. Use {nameof(AssetManagerEx.LoadSceneAsync)} method to change the main scene !";
                Debug.LogError(error);
                var operation = new UnloadSceneOperation(error);
                AsyncOperationSystem.StartOperation(operation);
                return operation;
            }

            // ж���ӳ���
            Scene sceneObj = sceneObject;       // ���︴��һ�����ݣ�UnloadSubScene��������������ʹ��sceneObjectʧЧ
            provider.assetSystem.UnloadSubScene(provider);
            {                
                var operation = new UnloadSceneOperation(sceneObj);
                AsyncOperationSystem.StartOperation(operation);
                return operation;
            }
        }
    }
}
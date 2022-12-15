using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.AssetManagement.Runtime
{
    public class SceneOperationHandle : OperationHandleBase
    {
        private System.Action<SceneOperationHandle> m_Callback;

        public SceneOperationHandle(ProviderBase provider) : base(provider)
        { }

        public override void InvokeCallback()
        {
            m_Callback?.Invoke(this);
        }

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
            return true;
        }

        public void UnloadAsync()
        {

        }
    }
}
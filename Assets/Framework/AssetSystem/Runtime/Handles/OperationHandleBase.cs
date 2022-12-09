using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class OperationHandleBase : IEnumerator
    {
        internal ProviderBase provider { get; private set; }

        internal OperationHandleBase(ProviderBase provider)
        {
            this.provider = provider;
        }

        public bool isDone
        {
            get
            {
                return true;
            }
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
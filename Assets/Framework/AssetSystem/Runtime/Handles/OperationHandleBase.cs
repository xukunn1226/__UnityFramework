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
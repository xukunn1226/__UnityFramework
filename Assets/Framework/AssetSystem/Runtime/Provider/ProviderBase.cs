using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    internal abstract class ProviderBase
    {
        /// <summary>
		/// ��ǰ�ļ���״̬
		/// </summary>
		public Defines.ELoadStatus Status { protected set; get; } = Defines.ELoadStatus.None;

        /// <summary>
		/// �Ƿ���ϣ��ɹ���ʧ�ܣ�
		/// </summary>
		public bool IsDone
        {
            get
            {
                return Status == Defines.ELoadStatus.Succeed || Status == Defines.ELoadStatus.Failed;
            }
        }

        public Task Task
        {
            get
            {
                if (_taskCompletionSource == null)
                {
                    _taskCompletionSource = new TaskCompletionSource<object>();
                    if (IsDone)
                        _taskCompletionSource.SetResult(null);
                }
                return _taskCompletionSource.Task;
            }
        }

        private TaskCompletionSource<object> _taskCompletionSource;
    }
}
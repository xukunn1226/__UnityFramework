using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Framework.AssetManagement.Runtime
{
    /// <summary>
	/// 同步其它线程里的回调到主线程里
	/// </summary>
    internal sealed class ThreadSyncContext : SynchronizationContext
    {
        private readonly ConcurrentQueue<Action> m_SafeQueue = new ConcurrentQueue<Action>();

        /// <summary>
        /// 更新同步队列
        /// </summary>
        public void Update()
        {
            while (true)
            {
                if (m_SafeQueue.TryDequeue(out Action action) == false)
                    return;
                action.Invoke();
            }
        }

        /// <summary>
        /// 向同步队列里投递一个回调方法
        /// </summary>
        public override void Post(SendOrPostCallback callback, object state)
        {
            Action action = new Action(() => { callback(state); });
            m_SafeQueue.Enqueue(action);
        }
    }
}
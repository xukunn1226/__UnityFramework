using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Framework.AssetManagement.Runtime
{
	public class AsyncOperationSystem
	{
		private static readonly List<AsyncOperationBase> s_Operations = new List<AsyncOperationBase>(100);

		// ��ʱ�����
		private static Stopwatch	s_Watch;
		private static long			s_FrameTime;

		public static long		maxTimeSlice	{ get; set; } = long.MaxValue;		// �첽��������Сʱ��Ƭ��
		public static bool		isBusy			{ get { return s_Watch.ElapsedMilliseconds - s_FrameTime >= maxTimeSlice; } }     // �������Ƿ�æ

		public static void Initialize()
		{
			s_Watch = Stopwatch.StartNew();
		}

		public static void Update()
		{
			s_FrameTime = s_Watch.ElapsedMilliseconds;

			int removedCount = 0;
			for (int i = s_Operations.Count - 1; i >= 0; i--)
			{
				// TODO: ���ֱ�����ʽ����ƽ������β����Ԫ�ػ��ʱ��Ƭ�Ļ������
				if (isBusy)
					break;

				var operation = s_Operations[i];
				operation.Update();
				if (operation.isDone)
				{
					// swap
					int lastBusy = s_Operations.Count - removedCount - 1;
					if(lastBusy != i)
                    {
						AsyncOperationBase op = s_Operations[lastBusy];
						s_Operations[lastBusy] = s_Operations[i];
						s_Operations[i] = op;
					}
					++removedCount;
				}
			}

			if(removedCount > 0)
            {
				for(int i = 0; i < removedCount; ++i)
                {
                    s_Operations[s_Operations.Count - i - 1].Finish();
                }
				s_Operations.RemoveRange(s_Operations.Count - removedCount, removedCount);
            }
		}

		public static void Destroy()
		{
			s_Operations.Clear();
			s_Watch = null;
			s_FrameTime = 0;
			maxTimeSlice = long.MaxValue;
		}

		public static void StartOperation(AsyncOperationBase operationBase)
		{
#if UNITY_EDITOR
			if(s_Operations.IndexOf(operationBase) != -1)
            {
				UnityEngine.Debug.LogError($"StartOperation: operationBase has already exist!");
				return;
            }
#endif
			s_Operations.Add(operationBase);
			operationBase.Start();
		}
	}
}
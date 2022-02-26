using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

public class TestJob : MonoBehaviour
{
    public int DataCount;

    private NativeArray<float3> m_JobDatas;

    private NativeArray<float> m_JobResults;

    private Vector3[] m_NormalDatas;
    
    private float[] m_NormalResults;
    

// Job adding two floating point values together
    [BurstCompile]
    public struct MyParallelJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> data;
        public NativeArray<float> result;

        public void Execute(int i)
        {
            float3 item = data[i];
            result[i] = Mathf.Sqrt(item.x * item.x + item.y * item.y + item.z * item.z);
        }
    }

    private void OnEnable()
    {
        m_JobDatas = new NativeArray<float3>(DataCount, Allocator.TempJob);
        m_JobResults = new NativeArray<float>(DataCount,Allocator.TempJob);
        
        m_NormalDatas = new Vector3[DataCount];
        m_NormalResults = new float[DataCount];
        
        for (int i = 0; i < DataCount; i++)
        {
            m_JobDatas[i] = new float3(1, 1, 1);
            m_NormalDatas[i] = new Vector3(1, 1, 1);
        }
    }

    private JobHandle handle;

    private void CreateJob()
    {
        Profiler.BeginSample("CreateJob");

        //Job部分
        MyParallelJob jobData = new MyParallelJob();
        jobData.data = m_JobDatas;
        jobData.result = m_JobResults;

        // Schedule the job with one Execute per index in the results array and only 1 item per processing batch
        handle = jobData.Schedule(DataCount, 64);

        Profiler.EndSample();
    }

    private void CompleteJob()
    {
        Profiler.BeginSample("CompleteJob");
        handle.Complete();        
        Profiler.EndSample();
        UnityEngine.Debug.Log($"-----   {m_JobResults[0]}");
    }

    // Update is called once per frame
    void Update()
    {        
        // CreateJob();
        // CompleteJob();

        // if(Input.GetKeyDown(KeyCode.A))
        // {
        //     UnityEngine.Debug.Log("start A");
            
        //     CreateJob();
        // }

        // if(Input.GetKeyDown(KeyCode.B))
        // {
        //     UnityEngine.Debug.Log("start B");
            
        //     CreateJob();
        //     JobHandle.ScheduleBatchedJobs();
        // }

        // if(Input.GetKeyDown(KeyCode.Space))
        // {
        //     UnityEngine.Debug.Log("complete");
            
        //     CompleteJob();
        // }



        // //Job部分
        // MyParallelJob jobData = new MyParallelJob();
        // jobData.data = m_JobDatas;
        // jobData.result = m_JobResults;

        // // Schedule the job with one Execute per index in the results array and only 1 item per processing batch
        // handle = jobData.Schedule(DataCount, 64);

        // // JobHandle.ScheduleBatchedJobs();

        // // Wait for the job to complete
        // handle.Complete();
        



        // Profiler.BeginSample("NormalCalculate");
        
        // //正常数据运算
        // for(var i = 0; i < DataCount; i++)
        // {
        //     var item = m_NormalDatas[i];
        //     m_NormalResults[i] = Mathf.Sqrt(item.x * item.x + item.y * item.y + item.z * item.z);
        // }
        
        // Profiler.EndSample();
    }

    public void OnDisable()
    {
        m_JobDatas.Dispose();
        m_JobResults.Dispose();
        m_NormalDatas = null;
        m_NormalResults = null;
    }
}
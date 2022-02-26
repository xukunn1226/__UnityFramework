using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public struct JobDataAndHandle
{
    public NativeArray<float3> data;
    JobHandle handle { get; }
}

[BurstCompile]
public struct MyJobTest : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> data;
    public NativeArray<float> result;

    public void Execute(int i)
    {
        float3 item = data[i];
        result[i] = Mathf.Sqrt(item.x * item.x + item.y * item.y + item.z * item.z);
    }
}

public class JobSystem
{
    private List<JobDataAndHandle> resultsAndHandles = new List<JobDataAndHandle>();

    public void ScheduleJob(IJobParallelFor job)
    {

    }

    public void CompleteJob()
    {

    }
}
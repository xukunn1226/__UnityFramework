using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

namespace AnimationInstancingModule.Runtime
{
    [BurstCompile]
    public struct AttachmentTransformJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float4x4> localToWorldMatrix;
        [ReadOnly]
        public NativeArray<float4x4> frameMatrix;
        public NativeArray<float4x4> worldMatrix;

        public void Execute(int index)
        {
            worldMatrix[index] = localToWorldMatrix[index] * frameMatrix[index];
        }
    }

    public class AttachmentTransform
    {
        private List<HandledResult> scheduledJobs = new List<HandledResult>();

        public void Update()
        {
            if(scheduledJobs.Count > 0)
            {

            }
        }

        public void ScheduleJob(float4x4[] localToWorldMatrix, float4x4[] frameMatrix)
        {            
            HandledResult newHandledResult = new HandledResult();

            newHandledResult.localToWorldMatrix = new NativeArray<float4x4>(localToWorldMatrix, Allocator.TempJob);
            newHandledResult.frameMatrix = new NativeArray<float4x4>(frameMatrix, Allocator.TempJob);
            newHandledResult.worldMatrix = new NativeArray<float4x4>(localToWorldMatrix.Length, Allocator.TempJob);

            AttachmentTransformJob job = new AttachmentTransformJob
            {
                localToWorldMatrix = newHandledResult.localToWorldMatrix,
                frameMatrix = newHandledResult.frameMatrix,
                worldMatrix = newHandledResult.worldMatrix
            };

            JobHandle handle = job.Schedule(1, 1);
            newHandledResult.handle = handle;
            scheduledJobs.Add(newHandledResult);
        }

        public void CompleteJob(HandledResult handle)
        {

        }
    }

    public struct HandledResult
    {
        public JobHandle handle;
        public NativeArray<float4x4> localToWorldMatrix;
        public NativeArray<float4x4> frameMatrix;
        public NativeArray<float4x4> worldMatrix;
    }
}
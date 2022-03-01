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
    public struct AttachmentTransformJob : IJob
    {
        [ReadOnly]
        public float4x4 localToWorldMatrix;
        [ReadOnly]
        public float4x4 frameMatrix;
        public NativeArray<float4x4> worldMatrix;

        public void Execute()
        {
            worldMatrix[0] = localToWorldMatrix * frameMatrix;
        }
    }

    public class AttachmentTransform
    {
        private List<HandledResult> scheduledJobs = new List<HandledResult>();

        public void ScheduleJob(Matrix4x4 localToWorldMatrix, Matrix4x4 frameMatrix, IAttachmentToInstancing[] attachments)
        {            
            HandledResult newHandledResult = new HandledResult
            {
                worldMatrix = new NativeArray<float4x4>(1, Allocator.TempJob),
                attachments = attachments
            };

            AttachmentTransformJob job = new AttachmentTransformJob
            {
                localToWorldMatrix = (float4x4)localToWorldMatrix,
                frameMatrix = (float4x4)frameMatrix,
                worldMatrix = newHandledResult.worldMatrix,
            };

            JobHandle handle = job.Schedule();
            newHandledResult.jobHandle = handle;
            scheduledJobs.Add(newHandledResult);
        }
        
        public void CompleteJobs()
        {
            if(scheduledJobs.Count > 0)
            {
                for(int i = 0; i < scheduledJobs.Count; ++i)
                {
                    CompleteJob(scheduledJobs[i]);
                }
                scheduledJobs.Clear();
            }
        }

        private void CompleteJob(HandledResult handle)
        {
            handle.jobHandle.Complete();

            // assign value to application
            Vector3 newPosition = ((Matrix4x4)handle.worldMatrix[0]).MultiplyPoint3x4(Vector3.zero);
            Quaternion newRotation = ((Matrix4x4)handle.worldMatrix[0]).rotation;
            for(int i = 0; i < handle.attachments.Length; ++i)
            {
                if(handle.attachments[i] == null) continue;

                handle.attachments[i].SetPositionAndRotation(newPosition, newRotation);
            }

            handle.worldMatrix.Dispose();
        }
    }

    public struct HandledResult
    {
        public JobHandle jobHandle;
        public NativeArray<float4x4> worldMatrix;
        public IAttachmentToInstancing[] attachments;
    }
}
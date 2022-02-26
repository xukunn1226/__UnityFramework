//
// Boids - Flocking behavior simulation.
//
// Copyright (C) 2014 Keijiro Takahashi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using UnityEngine;
using Unity.Mathematics;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using System;
using Unity.Collections.LowLevel.Unsafe;

[System.Serializable]
public struct boolean
{
    public byte boolValue;

    public boolean(bool value)
    {
        boolValue = (byte)(value ? 1 : 0);
    }

    public static implicit operator bool(boolean value)
    {
        return value.boolValue == 1;
    }

    public static implicit operator boolean(bool value)
    {
        return new boolean(value);
    }

    public override string ToString()
    {
        if (boolValue == 1)
            return "true";

        return "false";
    }
}

public class BoidController : MonoBehaviour
{
    public bool UseBurst = true;

    public GameObject boidPrefab;

    public int spawnCount = 10;

    public float spawnRadius = 4.0f;

    public Material mat;
    public Mesh mesh;

    [Range(0.1f, 20.0f)]
    public float velocity = 6.0f;

    [Range(0.0f, 0.9f)]
    public float velocityVariation = 0.5f;

    [Range(0.1f, 20.0f)]
    public float rotationCoeff = 4.0f;

    [Range(0.1f, 10.0f)]
    public float neighborDist = 2.0f;


    [Range(0.1f, 10.0f)]
    public float avoidDist = 2.0f;

    public LayerMask searchLayer;
    public LayerMask avoidLayer;

    private Transform[] m_boids;
    private NativeArray<HitResult> m_raycastAvoidHits;
    private NativeArray<boolean> m_forwardObstacles;
    private NativeArray<Vector3> m_forwardObstaclePositions;
    private NativeArray<Vector3> m_boidPositions;
    private NativeArray<Vector3> m_boidVelocities;
    private NativeArray<Quaternion> m_boidRotations;
    private TransformAccessArray m_boidsTransformArray;
    private TransformJob m_transJob;
    private UpdateArrayJob m_updateArrayJob;
    private ResetCastStatesJob m_resetCastResultJob;
    private JobHandle m_JobHandle;
    private JobHandle m_updateDataJobHandle;
    private JobHandle m_resetCastResultJobHandle;
    private bool prevBurstSettings;
    [SerializeField]
    private WanderController m_wanderer;

    private float[] m_angles;

    public struct HitResult
    {
        public boolean HasAvoidPath;
        public float Distance;
        public Vector3 Direction;
    }

    public struct RayForward
    {
        public float Angle;
        public Vector3 Axis;
    }

    void Start()
    {
        m_boidPositions = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        m_boidRotations = new NativeArray<Quaternion>(spawnCount, Allocator.Persistent);
        m_boidVelocities = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        m_forwardObstaclePositions = new NativeArray<Vector3>(spawnCount, Allocator.Persistent);
        m_raycastAvoidHits = new NativeArray<HitResult>(spawnCount, Allocator.Persistent);
        m_forwardObstacles = new NativeArray<boolean>(spawnCount, Allocator.Persistent);
        m_boids = new Transform[spawnCount];
        for (int i = 0; i < spawnCount; i++)
        {
            m_boids[i] = Spawn().transform;
        }

        m_boidsTransformArray = new TransformAccessArray(m_boids);
        prevBurstSettings = UseBurst;
        for (int i = 0; i < m_boidRotations.Length; i++)
        {
            m_boidPositions[i] = UnityEngine.Random.onUnitSphere;
            m_boidRotations[i] = Quaternion.identity;
        }
        var rayNum = 6;
        var angle = 180 / rayNum;
        m_angles = new float[rayNum];
        for (int i = 0; i < rayNum / 2; i++)
        {
            m_angles[i] = (-i - 1) * angle;
        }

        for (int i = 0; i < rayNum / 2; i++)
        {
            m_angles[rayNum / 2 + i] = (i + 1) * angle;
        }
    }

    void OnDestroy()
    {
        m_boidPositions.Dispose();
        m_boidRotations.Dispose();
        m_boidVelocities.Dispose();
        m_raycastAvoidHits.Dispose();
        m_forwardObstaclePositions.Dispose();
        m_forwardObstacles.Dispose();
        m_boidsTransformArray.Dispose();
    }

    public GameObject Spawn()
    {
        var spawnPos = transform.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
        return Spawn(spawnPos);
    }

    public GameObject Spawn(Vector3 position)
    {
        var rotation = Quaternion.Slerp(transform.rotation, UnityEngine.Random.rotation, 0.3f);
        var boid = Instantiate(boidPrefab, position, rotation) as GameObject;
        boid.GetComponent<BoidBehaviour>().controller = this;
        return boid;
    }

    private void RayCastFoward()
    {
        var m_raycastResults = new NativeArray<RaycastHit>(spawnCount, Allocator.TempJob);
        var m_raycastCommands = new NativeArray<SpherecastCommand>(spawnCount, Allocator.TempJob);
        for (int i = 0; i < m_raycastCommands.Length; i++)
        {
            m_raycastCommands[i] = new SpherecastCommand(m_boidPositions[i], 1, m_boidRotations[i] * Vector3.forward, avoidDist, avoidLayer);
        }
        var raycastJobHandle = SpherecastCommand.ScheduleBatch(m_raycastCommands, m_raycastResults, 32, default(JobHandle));
        raycastJobHandle.Complete();

        for (int i = 0; i < m_raycastResults.Length; i++)
        {
            if (m_raycastResults[i].collider != null)
            {
                m_forwardObstacles[i] = true;
                m_forwardObstaclePositions[i] = m_raycastResults[i].point;
            }
        }
        m_raycastResults.Dispose();
        m_raycastCommands.Dispose();
    }

    private void RayCastDir(float[] angles, RayForward rayForward, Vector3 rotateAxis, Vector3 forwardAxis, Color color)
    {
        var m_raycastResults = new NativeArray<RaycastHit>(spawnCount * angles.Length, Allocator.TempJob);
        var m_raycastCommands = new NativeArray<RaycastCommand>(spawnCount * angles.Length, Allocator.TempJob);
        var angleIndex = 0;
        for (int i = 0; i < m_raycastCommands.Length; i++)
        {
            var boidIndex = i / angles.Length;
            var axis = Quaternion.AngleAxis(rayForward.Angle, m_boidRotations[boidIndex] * rayForward.Axis) * m_boidRotations[boidIndex] * forwardAxis;
            var forward = Quaternion.AngleAxis(angles[angleIndex], m_boidRotations[boidIndex] * rotateAxis) * axis;
            //Debug.DrawRay(m_boidPositions[boidIndex], forward * avoidDist, color, 0);
            m_raycastCommands[i] = new RaycastCommand(m_boidPositions[boidIndex], forward, avoidDist, avoidLayer);
            angleIndex = i % angles.Length == 0 ? 0 : angleIndex + 1;
        }
        var raycastJobHandle = RaycastCommand.ScheduleBatch(m_raycastCommands, m_raycastResults, 32, default(JobHandle));
        raycastJobHandle.Complete();
        angleIndex = 0;
        for (int i = 0; i < m_raycastResults.Length; i++)
        {
            var boidIndex = i / angles.Length;
            if (m_forwardObstacles[boidIndex] && m_raycastResults[i].collider == null && !m_raycastAvoidHits[boidIndex].HasAvoidPath)
            {
                var dir = Quaternion.AngleAxis(angles[angleIndex], m_boidRotations[boidIndex] * rotateAxis) * Quaternion.AngleAxis(rayForward.Angle, m_boidRotations[boidIndex] * rayForward.Axis) * m_boidRotations[boidIndex] * forwardAxis;
                m_raycastAvoidHits[boidIndex] = new HitResult
                {
                    HasAvoidPath = true,
                    Distance = avoidDist,
                    Direction = dir,
                };
                //Debug.DrawRay(m_boidPositions[boidIndex], dir * avoidDist, Color.red, 0.2f);
            }
            angleIndex = i % angles.Length == 0 ? 0 : angleIndex + 1;
        }
        m_raycastResults.Dispose();
        m_raycastCommands.Dispose();
    }

    private void Update()
    {
        if (UseBurst)
        {
            for (int i = 0; i < m_forwardObstacles.Length; i++)
            {
                m_forwardObstacles[i] = false;
                m_forwardObstaclePositions[i] = Vector3.zero;
            }

            //====== OBSTACLE AVOIDENCE START

            RayCastFoward();

            var rayForwardUp = new RayForward
            {
                Angle = -45,
                Axis = Vector3.right
            };
            var rayForwardDown = new RayForward
            {
                Angle = 45,
                Axis = Vector3.right
            };
            var rayForward = new RayForward
            {
                Angle = 0,
                Axis = Vector3.right
            };
            var rayForwardVert = new RayForward
            {
                Angle = 0,
                Axis = Vector3.right
            };
            //RayCastDir(angles, rayForwardUp, Vector3.up, Vector3.forward, Color.cyan);
            //RayCastDir(angles, rayForwardDown, Vector3.up, Vector3.forward, Color.magenta);
            RayCastDir(m_angles, rayForward, Vector3.up, Vector3.forward, Color.green);
            RayCastDir(m_angles, rayForwardVert, Vector3.right, Vector3.forward, Color.blue);
            //RayCastDir(angles, Vector3.up, Quaternion.AngleAxis(-45, Vector3.right) * Vector3.forward);

            m_resetCastResultJob = new ResetCastStatesJob()
            {
                AvoidResult = m_raycastAvoidHits
            };
            // ====== OBSTACLE AVOIDENCE END

            m_transJob = new TransformJob()
            {
                FaceObstaclePoints = m_forwardObstaclePositions,
                FaceObstacles = m_forwardObstacles,
                AvoidHits = m_raycastAvoidHits,
                BoidVelocities = m_boidVelocities,
                BoidPositions = m_boidPositions,
                BoidRotations = m_boidRotations,
                ControllerFoward = transform.forward,
                ControllerPosition = transform.position,
                RotationCoeff = rotationCoeff,
                DeltaTime = Time.deltaTime,
                NeighborDist = neighborDist,
                Speed = velocity,
            };

            m_updateArrayJob = new UpdateArrayJob()
            {
                BoidPositions = m_boidPositions,
                BoidRotations = m_boidRotations,
            };
            m_updateDataJobHandle = m_updateArrayJob.Schedule(m_boidsTransformArray, m_resetCastResultJobHandle);
            m_JobHandle = m_transJob.Schedule(m_boidsTransformArray, m_updateDataJobHandle);
            m_resetCastResultJobHandle = m_resetCastResultJob.Schedule(m_raycastAvoidHits.Length, 32, m_JobHandle);
        }

        if (prevBurstSettings != UseBurst)
        {
            prevBurstSettings = UseBurst;
            foreach (var boid in m_boids)
            {
                boid.GetComponent<BoidBehaviour>().EnableLegacyMovement = !UseBurst;
            }
        }
    }

    public void LateUpdate()
    {
        if (UseBurst)
        {
            m_resetCastResultJobHandle.Complete();
        }
    }

    [BurstCompile]
    public struct TransformJob : IJobParallelForTransform
    {
        public NativeArray<Vector3> BoidVelocities;
        [ReadOnly]
        public NativeArray<boolean> FaceObstacles;
        [ReadOnly]
        public NativeArray<Vector3> FaceObstaclePoints;
        public NativeArray<HitResult> AvoidHits;
        [ReadOnly]
        public NativeArray<Vector3> BoidPositions;
        [ReadOnly]
        public NativeArray<Quaternion> BoidRotations;
        [ReadOnly]
        public Vector3 ControllerFoward;
        [ReadOnly]
        public Vector3 ControllerPosition;
        [ReadOnly]
        public float RotationCoeff;
        [ReadOnly]
        public float DeltaTime;
        [ReadOnly]
        public float NeighborDist;
        [ReadOnly]
        public float Speed;

        Vector3 GetSeparationVector(Vector3 current, Vector3 targetPos)
        {
            var diff = current - targetPos;
            var diffLen = diff.magnitude;
            var scaler = Mathf.Clamp01(1.0f - diffLen / NeighborDist);
            return diff * (scaler / diffLen);
        }

        float GetAvoidSeparationVector(Vector3 current, Vector3 targetPos, float avoidDistance)
        {
            var diff = current - targetPos;
            var diffLen = diff.sqrMagnitude;
            var optFactor = avoidDistance * avoidDistance;
            return (optFactor / diffLen);
        }

        public void Execute(int index, TransformAccess trans)
        {
            var noise = Mathf.PerlinNoise(DeltaTime + index * 0.01f, index) * 2.0f - 1.0f;
            var Speed = this.Speed * (1.0f + noise * 0.5f);
            var currentPosition = BoidPositions[index];
            var currentRotation = BoidRotations[index];

            var separation = Vector3.zero;
            var alignment = ControllerFoward;
            var cohesion = ControllerPosition;
            var neighborCount = 0;
            for (int i = 0; i < BoidPositions.Length; i++)
            {
                if (index == i)
                {
                    neighborCount++;
                    continue;
                }
                if ((BoidPositions[i] - BoidPositions[index]).sqrMagnitude <= (NeighborDist) * (NeighborDist))
                {
                    separation += GetSeparationVector(BoidPositions[index], BoidPositions[i]);
                    alignment += (BoidRotations[i] * Vector3.forward);
                    cohesion += BoidPositions[i];
                    neighborCount++;
                }
            }

            if (FaceObstacles[index])
            {
                if (AvoidHits[index].HasAvoidPath)
                {
                    separation += AvoidHits[index].Direction * GetAvoidSeparationVector(BoidPositions[index], FaceObstaclePoints[index], AvoidHits[index].Distance);
                }
                else
                {
                    separation += (BoidPositions[index] - FaceObstaclePoints[index]) * GetAvoidSeparationVector(BoidPositions[index], FaceObstaclePoints[index], AvoidHits[index].Distance);
                }
            }
            var avg = 1.0f / Mathf.Max(1, neighborCount);
            alignment *= avg;
            cohesion *= avg;
            cohesion = (cohesion - currentPosition).normalized;

            var direction = alignment + cohesion + separation;
            var accel = alignment * 10f + cohesion * 30f + separation * 35f;
            var vel = BoidVelocities[index] + accel * DeltaTime;
            vel = Vector3.ClampMagnitude(vel, Speed);
            BoidVelocities[index] = vel;

            var rotation = Quaternion.FromToRotation(Vector3.forward, direction.normalized);
            if (rotation != currentRotation)
            {
                var ip = Mathf.Exp(-RotationCoeff * DeltaTime);
                rotation = Quaternion.Slerp(rotation, currentRotation, ip);
            }
            trans.rotation = rotation;
            trans.position = BoidPositions[index] + (vel * DeltaTime);

        }
    }


    [BurstCompile]
    public struct UpdateArrayJob : IJobParallelForTransform
    {
        [WriteOnly]
        public NativeArray<Vector3> BoidPositions;
        [WriteOnly]
        public NativeArray<Quaternion> BoidRotations;

        public void Execute(int index, TransformAccess trans)
        {
            BoidPositions[index] = trans.position;
            BoidRotations[index] = trans.rotation;
        }
    }

    [BurstCompile]
    public struct ResetCastStatesJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<HitResult> AvoidResult;

        public void Execute(int index)
        {
            AvoidResult[index] = new HitResult();
        }
    }

}


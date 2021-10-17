using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Runtime
{
    public class LocomotionAgent : ZComp
    {
        public delegate void onEnterViewHandler(Vector3 startPosition, Vector3 startRotation);
        public event onEnterViewHandler     onEnterView;
        public event Action                 onLeaveView;
        public event Action                 onStopped;

        public int      id                  { get; private set; }
        public Vector3  startPosition       { get; set; }
        public Vector3  startRotation       { get; set; }
        public Vector3  direction           { get; private set; }
        public Vector3  position            { get; private set; }
        public Vector3  rotation            { get; private set; }       // euler angles
        public Vector3  destination         { get; private set; }
        public float    speed               { get; set; }   = 1;
        public float    angularSpeed        { get; set; }   = 300;      // deg/s
        public bool     updateRotation      { get; set; }               = true;
        public float    remainingDistance   { get; private set; }
        public float    stoppingDistance    { get; set; }
        private bool    m_isReached;

        public LocomotionAgent(ZActor actor) : base(actor) {}

        public override void Prepare(IData data)
        {
            base.Prepare(data);
        }

        public override void Start()
        {
            base.Start();
            id = LocomotionManager.AddInstance(this);
            EnterView(startPosition, startRotation);
        }

        public override void Destroy()
        {
            LeaveView();
            LocomotionManager.RemoveInstance(this);
            base.Destroy();
        }

        public void SetDestination(Vector3 targetPos)
        {
            destination = targetPos;
            m_isReached = false;
        }

        public void EnterView(Vector3 startPosition, Vector3 startRotation)
        {
            position = startPosition;
            rotation = startRotation;
            destination = startPosition;
            onEnterView?.Invoke(startPosition, startRotation);
        }

        public void LeaveView()
        {
            onLeaveView?.Invoke();
        }

        private void Stop()
        {
            onStopped?.Invoke();
        }

        public void OnUpdate(float deltaTime)
        {
            if(m_isReached)
                return;
            
            direction = (destination - position).normalized;
            remainingDistance = Vector3.Distance(destination, position);
            if(remainingDistance < stoppingDistance + 0.001f)
            {                
                m_isReached = true;
                Stop();
            }
            else
            {
                float delta = Mathf.Min(remainingDistance, speed * deltaTime);
                position += direction * delta;

                if(updateRotation)
                {
                    Quaternion from = Quaternion.Euler(rotation);
                    Quaternion to = Quaternion.LookRotation(direction);
                    Quaternion cur = Quaternion.RotateTowards(from, to, angularSpeed * deltaTime);
                    rotation = cur.eulerAngles;

                    // rotation = Vector3.RotateTowards(rotation, direction, angularSpeed * Mathf.Deg2Rad * deltaTime, 0);                    
                }
            }
        }
    }
}
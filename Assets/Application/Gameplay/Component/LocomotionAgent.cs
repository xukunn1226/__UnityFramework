using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Runtime
{
    public class LocomotionAgent : ZComp
    {
        public delegate void onEnterViewHandler(Vector3 startPoint);
        public event onEnterViewHandler     onEnterView;
        public event Action                 onLeaveView;
        public event Action                 onStopped;

        public int      id                  { get; private set; }
        public Vector3  startPoint          { get; set; }
        public Vector3  position            { get; private set; }
        public Vector3  destination         { get; private set; }
        public float    speed               { get; set; }
        public float    angularSpeed        { get; set; }               // deg/s
        public bool     updateRotation      { get; set; }
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
            EnterView(startPoint);
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

        public void EnterView(Vector3 startPoint)
        {
            position = startPoint;
            onEnterView?.Invoke(startPoint);
        }

        public void LeaveView()
        {
            onLeaveView?.Invoke();
        }

        public void OnUpdate()
        {
            if(m_isReached)
                return;
        }
    }
}
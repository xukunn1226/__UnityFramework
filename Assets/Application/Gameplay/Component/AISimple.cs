using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class AISimple : ZComp
    {
        public int                      id              { get; private set; }
        private TestRenderableProfile   m_Renderer;
        private LocomotionAgent         m_Agent;

        public enum State
        {
            idle,
            walk,
            run,
            attack,
            max,
        }
        public State                    state           { get; private set; }
        private float                   m_Time;
        private float                   m_ElapsedTime;

        public AISimple(ZActor actor) : base(actor) {}

        public override void Prepare(IData data)
        {
            base.Prepare(data);

            m_Renderer = actor.GetComponent<TestRenderableProfile>();
            m_Agent = actor.GetComponent<LocomotionAgent>();

            m_Agent.onEnterView += OnEnterView;
            m_Agent.onLeaveView += OnLeaveView;
            m_Agent.onStopped += OnStopped;
        }

        public override void Start()
        {
            base.Start();
            id = AISimpleManager.AddInstance(this);
        }

        public override void Destroy()
        {
            m_Agent.onEnterView -= OnEnterView;
            m_Agent.onLeaveView -= OnLeaveView;
            m_Agent.onStopped -= OnStopped;

            AISimpleManager.RemoveInstance(this);
            base.Destroy();
        }

        private void OnEnterView(Vector3 startPosition, Vector3 startRotation)
        {
            RandomCommand();
        }

        private void OnLeaveView()
        {

        }

        private void OnStopped()
        {
            RandomCommand();
        }

        public void OnUpdate(float deltaTime)
        {
            switch(state)
            {
                case State.idle:
                    m_ElapsedTime += deltaTime;
                    if(m_ElapsedTime > m_Time)
                    {
                        RandomCommand();
                    }
                    break;
                case State.attack:
                    m_ElapsedTime += deltaTime;
                    if(m_ElapsedTime > m_Time)
                    {
                        RandomCommand();
                    }
                    break;
            }

            if (m_Renderer.root != null)
            {
                m_Renderer.root.transform.position = m_Agent.position;
                m_Renderer.root.transform.eulerAngles = m_Agent.rotation;
            }
        }

        private void RandomCommand()
        {
            State st = (State)Random.Range(0, (int)State.max);
            switch(st)
            {
                case State.idle:
                    state = st;
                    m_Time = Random.Range(4.0f, 7.0f);
                    m_ElapsedTime = 0;
                    m_Renderer.PlayAnimation("idle");
                    // Debug.Log($"state: {state}  time: {m_Time}");
                    break;
                case State.walk:
                    state = st;
                    m_Agent.SetDestination(new Vector3(Random.Range(-1.0f, 1.0f) * 30, 0, Random.Range(-1.0f, 1.0f) * 30));
                    m_Agent.speed = 1;
                    m_Agent.angularSpeed = 300;
                    m_Renderer.PlayAnimation("walk");
                    // Debug.Log($"state: {state}  destination: {m_Agent.destination}");
                    break;
                case State.run:
                    state = st;
                    m_Agent.SetDestination(new Vector3(Random.Range(-1.0f, 1.0f) * 50, 0, Random.Range(-1.0f, 1.0f) * 50));
                    m_Agent.speed = 6.5f;
                    m_Agent.angularSpeed = 450;
                    m_Renderer.PlayAnimation("run");
                    // Debug.Log($"state: {state}  destination: {m_Agent.destination}");
                    break;
                case State.attack:
                    state = st;
                    m_Time = Random.Range(6.0f, 9.0f);
                    m_ElapsedTime = 0;
                    m_Renderer.PlayAnimation("attack");
                    // Debug.Log($"state: {state}  time: {m_Time}");
                    break;
            }
        }
    }
}
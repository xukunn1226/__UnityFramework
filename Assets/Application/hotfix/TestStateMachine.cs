using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Application.HotFix
{
    public enum Life
    {
        Born,
        Young,
        Man,
    }

    public class Born : IState<Life>
    {
        public Life Id { get { return Life.Born; } }

        public void OnEnter(IState<Life> prevState)
        { }

        public void OnLeave(IState<Life> nextState)
        { }

        public void OnUpdate(float deltaTime)
        { }
    }


    public class Young : IState<Life>
    {
        public Life Id { get { return Life.Young; } }

        public void OnEnter(IState<Life> prevState)
        { }

        public void OnLeave(IState<Life> nextState)
        { }

        public void OnUpdate(float deltaTime)
        { }
    }


    public class Man : IState<Life>
    {
        public Life Id { get { return Life.Man; } }

        public void OnEnter(IState<Life> prevState)
        { }

        public void OnLeave(IState<Life> nextState)
        { }

        public void OnUpdate(float deltaTime)
        { }
    }

    public class TestStateMachine
    {
        // A Test behaves as an ordinary method
        public void TestStateMachineSimplePasses()
        {
            StateMachine<Life> m_StateMachine = new StateMachine<Life>();
            m_StateMachine.AddState(new Born());
            m_StateMachine.AddState(new Young());
            m_StateMachine.AddState(new Man());

            m_StateMachine.GetState(Life.Young);
            m_StateMachine.SwitchTo(Life.Man);



            StateMachine<Life> m_SimpleStateMachine = new StateMachine<Life>();
            m_SimpleStateMachine.AddState(new SimpleState<Life>(Life.Born, OnEnterState, OnUpdateState, OnLeaveState));
            m_SimpleStateMachine.AddState(new SimpleState<Life>(Life.Young, OnEnterState, OnUpdateState, OnLeaveState));
            m_SimpleStateMachine.AddState(new SimpleState<Life>(Life.Man, OnEnterState, OnUpdateState, OnLeaveState));

            m_SimpleStateMachine.SwitchTo(Life.Man);
            m_SimpleStateMachine.SwitchTo(Life.Born);
        }

        void OnEnterState(SimpleState<Life> curState, SimpleState<Life> prevState)
        {
        }

        void OnLeaveState(SimpleState<Life> curState, SimpleState<Life> nextState)
        {
        }

        void OnUpdateState(SimpleState<Life> curState, float deltaTime)
        {
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        public IEnumerator TestStateMachineWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;

public class DebugStateMachine : MonoBehaviour
{
    private StateMachine<Life> m_StateMachine;

    private StateMachine<Life> m_SimpleStateMachine;

    void Start()
    {
        m_StateMachine = new StateMachine<Life>();
        m_StateMachine.AddState(new Born());
        m_StateMachine.AddState(new Young());
        m_StateMachine.AddState(new Man());

        m_StateMachine.GetState(Life.Young);
        m_StateMachine.SwitchTo(Life.Man);

        // simple state machine
        m_SimpleStateMachine = new StateMachine<Life>();
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

    // Update is called once per frame
    void Update()
    {
        m_StateMachine.Update(Time.deltaTime);
        m_SimpleStateMachine.Update(Time.deltaTime);
    }
}

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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class IDisposableTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        using (IDisposeBase dbase = new IDisposeBase())
        {
            dbase.PrintIt();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


public class IDisposeBase : IDisposable
{
    private bool m_Disposed = false;

    ~IDisposeBase()
    {
        UnityEngine.Debug.Log("~IDisposeBase");
        Dispose(false);
    }

    public void Dispose()
    {
        UnityEngine.Debug.Log("Dispose");
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void PrintIt()
    {
        UnityEngine.Debug.Log("Print It");
    }

    protected virtual void Dispose(bool shouldDisposeManagedReources)
    {
        if (m_Disposed)
            return;

        if (shouldDisposeManagedReources)
        {
            // TODO:释放那些实现IDisposable接口的托管对象
        }

        //TODO:释放非托管资源，设置对象为null

        m_Disposed = true;
    }

    public void MethodForPublic()
    {
        if (m_Disposed)
            throw new Exception("object has been disposed!");
        // do the normal things
    }
}

public class IDisposeDerived : IDisposeBase
{
    private bool m_Disposed = false;

    protected override void Dispose(bool shouldDisposeManagedReources)
    {
        if (m_Disposed)
            return;

        if (shouldDisposeManagedReources)
        {
            // dispose managed resources
        }
        // dispose unmanaged resources

        // call the base's dispose method
        base.Dispose(shouldDisposeManagedReources);

        // set the flag
        m_Disposed = true;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Core;

public class TestLog : MonoBehaviour
{
    private FileLogOutput m_FileOutput;

    private void Awake()
    {
        GameDebug.Init();
        m_FileOutput = new FileLogOutput();
        GameDebug.RegisterOutputDevice(m_FileOutput);
    }

    private void OnDestroy()
    {
        GameDebug.UnregisterOutputDevice(m_FileOutput);
        GameDebug.Shutdown();
    }

    // Start is called before the first frame update
    void Start()
    {
        GameDebug.LogError("{0}  {1}", "sfsd", "3223");

        GameDebug.Log("{0}  {1}", "11111111我们1", "ABCCCC");

        GameDebug.Log("{0}  {1}", "122222222221我们1", "ABCCCC");

        GameDebug.LogError("45454====888822222222");

        GameDebug.Flush();
    }
}
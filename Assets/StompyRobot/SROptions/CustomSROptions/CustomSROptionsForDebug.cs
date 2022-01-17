using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;
using System.Diagnostics;
#if !DISABLE_SRDEBUGGER
using SRDebugger;
using SRDebugger.Services;
#endif
using SRF;
using SRF.Service;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Application.Runtime;
using Framework.Core;


public partial class SROptions
{
    [Category("TestSkill")] // Options will be grouped by category
    public bool SkillDebug
    {
        get;
        set;
    }

    [Category("TestSkill")] // Options will be grouped by category
    public float SkillDebugDelayTime
    {
        get;
        set;
    }

    [Category("TestDissolve")]
    public void ChangeDissolveShader_Origin()
    {
        SetDissovleShader(0);
    }

    [Category("TestDissolve")]
    public void ChangeDissolveShader_1()
    {
        SetDissovleShader(1);
    }

    [Category("TestDissolve")]
    public void ChangeDissolveShader_2()
    {
        SetDissovleShader(2);
    }

    private void SetDissovleShader(int index)
    {
        Debug.Log($"SetDissovleShader to [{index}]");
    }

    [Category("TestDissolve")] // Options will be grouped by category
    public bool ToggleProbe
    {
        get;
        set;
    }

    [Category("Resolution")]
    public void SetResolution_1()
    {
        SetResolution(1.0f);
    }

    [Category("Resolution")]
    public void SetResolution_08()
    {
        SetResolution(0.8f);
    }

    [Category("Resolution")]
    public void SetResolution_06()
    {
        SetResolution(0.6f);
    }    

    private void SetResolution(float scale)
    {
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainLoop : MonoBehaviour
{
    void Awake()
    {
        if(Launcher.Instance == null)
            throw new System.Exception("MainLoop: Launcher.Instance == null");

        Launcher.Instance.Disable();        // 结束Launcher流程
    }
    
    public void Restart()
    {
        
    }

    public void Reconnect()
    {

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MainLoop))]
public class MainLoop_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Restart"))
        {
            ((MainLoop)target).Restart();
        }

        if(GUILayout.Button("Reconnect"))
        {
            ((MainLoop)target).Reconnect();
        }
    }
}
#endif
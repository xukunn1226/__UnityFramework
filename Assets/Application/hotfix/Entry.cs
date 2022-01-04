using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application.Runtime;
using Application.Logic;

namespace Logic
{
    static public class Entry
    {
        static public void Start()
        {
            Debug.Log("Entry.Start======");
            CodeLoader.Instance.Update += Update;
            CodeLoader.Instance.OnApplicationQuit += OnApplicationQuit;
            CodeLoader.Instance.OnApplicationFocus += OnApplicationFocus;
            CodeLoader.Instance.OnDestroy += OnDestroy;

            GameModeManager.Instance.SwitchTo(GameState.World);
        }

        static public void OnDestroy()
        {
            SingletonBase.DestroyAll();
        }

        static public void Update()
        {
            // Debug.Log("Update");
            SingletonBase.Update(Time.deltaTime);
        }

        static public void OnApplicationFocus(bool isFocus)
        {}

        static public void OnApplicationQuit()
        {

        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Runtime.Tests
{
    public class TestEventManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            EventManager.AddEventListener<int>("OnClick", OnFoo1);
            EventManager.AddEventListener<int>("OnClick", OnFoo1);

            // Action<int> ff = OnFoo1;
            // Delegate[] m1 = ff.GetInvocationList();
            // ff += OnFoo2;
            // Delegate[] m2 = ff.GetInvocationList();
            // ff.Invoke(1);
            
        }

        void OnGUI()
        {
            if(GUI.Button(new Rect(100, 100, 200, 60), "Trigger"))
            {
                EventManager.Dispatch<int>("OnClick", 1);
            }
        }

        private void OnFoo1(int data)
        {
            Debug.Log($"OnFoo1:     {Time.frameCount}");
            // EventManager.RemoveEventListener<int>("OnClick", OnFoo1);
        }

        private void OnFoo2(int data)
        {
            Debug.Log($"OnFoo2:     {Time.frameCount}");
        }
    }
}
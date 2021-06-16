using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Runtime.Tests
{
    public class TestEventManager : MonoBehaviour
    {
        public TestPlayer m_Player;

        void Start()
        {
            m_Player = new TestPlayer();
            m_Player.Start();
        }

        void OnGUI()
        {
            if(GUI.Button(new Rect(100, 100, 200, 60), "Trigger"))
            {
                EventManager.Allocate<EventArgs_HP>().Set(HPEvent.HPChange, 2).Dispatch();
            }
            if(GUI.Button(new Rect(100, 300, 200, 60), "Remove Listener"))
            {
                EventManager.RemoveEventListener(HPEvent.HPChange, m_Player.OnFoo1);
            }
            if(GUI.Button(new Rect(100, 500, 200, 60), "Destroy Player && GC"))
            {
                if(m_Player != null)
                {
                    // Destroy(m_Player.gameObject);
                    m_Player = null;
                }
                Resources.UnloadUnusedAssets();
                System.GC.Collect();
            }
        }
    }
}
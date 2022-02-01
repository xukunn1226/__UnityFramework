using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace StarterAssets
{
    public class DungeonStartup : MonoBehaviour
    {
        private MyPlayerLogic m_PlayerLogic;

        void OnGUI()
        {
            if(GUI.Button(new Rect(100, 100, 200, 120), "spawn player"))
            {
                m_PlayerLogic = MyPlayerLogic.Create(1);

                // bind Player to camera follow target
                CinemachineVirtualCamera vc = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
                vc.m_Follow = m_PlayerLogic.playerCameraRoot;
            }
            // if(GUI.Button(new Rect(100, 300, 200, 120), "destroy player"))
            // {
            //     MyPlayerLogic.Destroy(m_PlayerLogic);
            // }
        }

        void Update()
        {
            m_PlayerLogic?.Update(Time.deltaTime);
        }
    }
}
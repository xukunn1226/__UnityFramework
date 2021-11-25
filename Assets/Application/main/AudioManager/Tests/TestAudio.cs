using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

namespace Application.Runtime
{
    public class TestAudio : MonoBehaviour
    {
        public EventReference m_Test1;
        public EventReference m_Test2;
        public FMOD.Studio.EventInstance m_Test2Inst;
        
        void Start()
        {

        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                Test1();
            }
            if(Input.GetKeyDown(KeyCode.S))
            {
                StopTest1();
            }
            if(Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }
        }

        void Test1()
        {
            AudioManager.PlayBGM(m_Test1, 3);
        }
        void StopTest1()
        {
            AudioManager.StopBGM();
        }

        void Test2()
        {
            m_Test2Inst = AudioManager.Play2D(m_Test2, true);
        }

        void StopTest2()
        {
            AudioManager.Stop(m_Test2Inst);
        }

        void Restart()
        {
            AudioManager.Restart(m_Test2Inst);
        }
    }
}
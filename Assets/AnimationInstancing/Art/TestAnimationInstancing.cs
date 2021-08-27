using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancingModule.Runtime
{
    public class TestAnimationInstancing : MonoBehaviour
    {
        private AnimationInstancing inst;

        // Start is called before the first frame update
        void Awake()
        {
            inst = GetComponent<AnimationInstancing>();
            inst.OnAnimationBegin += OnAnimationBegin;
            inst.OnAnimationEnd += OnAnimationEnd;
            inst.OnAnimationEvent += OnAnimationEvent;
            inst.onOverridePropertyBlock += OnOverridePropertyBlock;
        }

        private void OnDestroy()
        {            
            inst.OnAnimationBegin -= OnAnimationBegin;
            inst.OnAnimationEnd -= OnAnimationEnd;
            inst.OnAnimationEvent -= OnAnimationEvent;
            inst.onOverridePropertyBlock -= OnOverridePropertyBlock;
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log("F1");
                inst.PlayAnimation("attack02");
            }

            if(Input.GetKeyDown(KeyCode.F2))
            {
                Debug.Log("F2");
                inst.PlayAnimation("attack03");
            }
        }

        private void OnAnimationBegin(string aniName)
        {
            Debug.Log($"------ OnAnimationBegin: {aniName}");
        }

        private void OnAnimationEnd(string aniName)
        {
            Debug.Log($"OnAnimationEnd ------------- {aniName}");
        }

        private void OnAnimationEvent(string aniName, string evtName, AnimationEvent evt)
        {
            Debug.Log($"OnAnimationEvent: {aniName}     {evtName}     {evt.stringParameter}");
        }

        public void OnOverridePropertyBlock(int materialIndex, MaterialPropertyBlock block)
        {
            // int ii = 0;
        }
    }
}
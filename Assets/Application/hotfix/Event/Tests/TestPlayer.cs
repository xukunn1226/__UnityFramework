using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.HotFix
{
    public class TestPlayer : MonoBehaviour
    {
        // Start is called before the first frame update
        public void AddListener()
        {            
            EventManager.AddEventListener(HPEvent.HPChange, OnFoo1);
            EventManager.AddEventListener(HPEvent.HPChange, OnFoo2);
        }

        public void RemoveListener()
        {
            EventManager.RemoveEventListener(HPEvent.HPChange, OnFoo1);
            EventManager.RemoveEventListener(HPEvent.HPChange, OnFoo2);
        }

        public void OnFoo1(EventArgs args)
        {
            Debug.Log($"OnFoo1:     {Time.frameCount}   {((EventArgs_HP)args).value}");
            // EventManager.RemoveEventListener(HPEvent.HPChange, OnFoo1);
        }

        public void OnFoo2(EventArgs args)
        {
            Debug.Log($"OnFoo2:     {Time.frameCount}");
        }
    }    
}
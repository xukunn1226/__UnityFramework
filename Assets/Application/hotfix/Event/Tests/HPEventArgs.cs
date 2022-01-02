using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{    
    public enum HPEvent
    {
        HPChange,
    }

    public class EventArgs_HP : EventArgs
    {
        public int value { get; private set; }

        public EventArgs_HP Set(HPEvent type, int value)
        {
            base.Set(type);
            this.value = value;
            return this;
        }
    }
}
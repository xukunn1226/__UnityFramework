using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.Logic
{
    public class EventArgs
    {
        public Enum eventType { get; protected set; }
        public virtual EventArgs Set(Enum type)
        {
            eventType = type;
            return this;
        }
    }
}
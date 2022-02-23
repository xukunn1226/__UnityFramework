using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Application.Logic
{
    static public class UIHelper
    {
        public static void AddEventTriggerListener(this EventTrigger eventTrigger, EventTriggerType triggerType, UnityAction<BaseEventData> action)
        {
            EventTrigger.Entry entry = eventTrigger.triggers.Find((trigger) => trigger.eventID == triggerType);
            if (entry == null)
            {
                entry = new EventTrigger.Entry() { eventID = triggerType };
                eventTrigger.triggers.Add(entry);
            }
            entry.callback.AddListener(action);
        }
    }
}
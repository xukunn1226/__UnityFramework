using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class LocomotionManager
    {
        static private int                              s_Id            = 0;
        static private Dictionary<int, LocomotionAgent> s_AgentList     = new Dictionary<int, LocomotionAgent>();

        static public int AddInstance(LocomotionAgent agent)
        {
            #if UNITY_EDITOR
            if(s_AgentList.ContainsValue(agent))
                throw new System.ArgumentException("Failed to add locomotion agent: LocomotionAgent has exist");
            #endif

            s_AgentList.Add(s_Id, agent);
            return s_Id++;
        }

        static public void RemoveInstance(LocomotionAgent agent)
        {
            #if UNITY_EDITOR
            if(!s_AgentList.ContainsKey(agent.id))
                throw new System.ArgumentException($"Failed to remove locomotion agent: ID {agent.id} has not exist");
            #endif

            s_AgentList.Remove(agent.id);
        }
    }
}
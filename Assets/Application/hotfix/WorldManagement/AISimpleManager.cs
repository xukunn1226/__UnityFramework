using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
{
    public class AISimpleManager : Singleton<AISimpleManager>
    {
        private int                                 s_Id            = 0;
        private Dictionary<int, AISimple>           s_AgentList     = new Dictionary<int, AISimple>();

        public int AddInstance(AISimple agent)
        {
            #if UNITY_EDITOR
            if(s_AgentList.ContainsValue(agent))
                throw new System.ArgumentException("Failed to add locomotion agent: LocomotionAgent has exist");
            #endif

            s_AgentList.Add(s_Id, agent);
            return s_Id++;
        }

        public void RemoveInstance(AISimple agent)
        {
            #if UNITY_EDITOR
            if(!s_AgentList.ContainsKey(agent.id))
                throw new System.ArgumentException($"Failed to remove locomotion agent: ID {agent.id} has not exist");
            #endif

            s_AgentList.Remove(agent.id);
        }

        protected override void OnUpdate(float deltaTime)
        {
            UnityEngine.Profiling.Profiler.BeginSample("AISimpleManager.OnUpdate");
            foreach(var item in s_AgentList)
            {
                item.Value.OnUpdate(deltaTime);
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class TestActorManager
    {
        static private Dictionary<int, TestActor> s_Actors = new Dictionary<int, TestActor>();
        static private string[] s_AssetPathList = new string[] {"assets/res/players/zombie_01_variant.prefab",
                                                                "assets/res/players/zombie_02_variant.prefab"};

        static public void CreateActor()
        {
            TestActor actor = new TestActor();
            actor.id = s_Actors.Count;
            actor.name = "Actor_" + actor.id;
            actor.Start();
            s_Actors.Add(actor.id, actor);
        }

        static public void DestroyActor(int id)
        {
            TestActor actor;
            if(!s_Actors.TryGetValue(id, out actor))
            {
                throw new System.ArgumentException($"DestroyActor: can't find the actor [{id}]");
            }
            actor.Destroy();
            s_Actors.Remove(id);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Logic
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
            actor.startPosition = new Vector3(Random.Range(-1.0f, 1.0f) * 30, 0, Random.Range(-1.0f, 1.0f) * 30);
            actor.startRotation = new Vector3(0, Random.Range(0, 360), 0);
            actor.Prepare();
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

        static public void DestroyRandom()
        {
            DestroyActor(s_Actors.Count - 1);
        }

        static public void DestroyAll()
        {
            foreach(var item in s_Actors)
            {
                item.Value.Destroy();
            }
            s_Actors.Clear();
        }
    }
}
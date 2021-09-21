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
            actor.minViewLayer = ViewLayer.ViewLayer_0;
            actor.maxViewLayer = ViewLayer.ViewLayer_2;
            actor.visible = true;
            actor.assetPath = s_AssetPathList[Random.Range(0, 2)];
            actor.symbolAssetPath = "assets/res/players/symbol.prefab";
            actor.Init();
            s_Actors.Add(actor.id, actor);
            ViewLayerManager.AddInstance(actor);
            // AssetLoadingAsyncManager.AddInstance(actor);
        }

        static public void DestroyActor(int id)
        {
            TestActor actor;
            if(!s_Actors.TryGetValue(id, out actor))
            {
                throw new System.ArgumentException($"DestroyActor: can't find the actor [{id}]");
            }
            actor.Uninit();
            s_Actors.Remove(id);
            ViewLayerManager.RemoveInstance(actor);
            // AssetLoadingAsyncManager.RemoveInstance(actor);
        }
    }
}
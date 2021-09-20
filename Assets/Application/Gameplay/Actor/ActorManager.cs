using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class ActorManager
    {
        static private Dictionary<int, Actor> s_Actors = new Dictionary<int, Actor>();
        static private string[] s_AssetPathList = new string[] {"assets/res/players/zombie_01_variant.prefab",
                                                                "assets/res/players/zombie_02_variant.prefab"};

        static public void CreateActor(int id)
        {
            Actor actor = new Actor();
            actor.id = id;
            actor.name = "Actor_" + id;
            actor.minViewLayer = ViewLayer.ViewLayer_0;
            actor.maxViewLayer = ViewLayer.ViewLayer_2;
            actor.visible = true;
            actor.assetPath = s_AssetPathList[Random.Range(0, 2)];
            s_Actors.Add(id, actor);

            ViewLayerManager.AddInstance(actor);
        }

        static public void DestroyActor(int id)
        {
            Actor actor;
            if(!s_Actors.TryGetValue(id, out actor))
            {
                throw new System.ArgumentException($"DestroyActor: can't find the actor [{id}]");
            }
            s_Actors.Remove(id);
            ViewLayerManager.RemoveInstance(actor);
        }
    }

    public interface IEntity
    {
        string  name    { get; set; }
        int     id      { get; set; }
    }

    public class Actor : IEntity, IViewLayer
    {
        public string       name            { get; set; }
        public int          id              { get; set; }
        public ViewLayer    minViewLayer    { get; set; } = ViewLayer.ViewLayer_0;
        public ViewLayer    maxViewLayer    { get; set; } = ViewLayer.ViewLayer_1;
        public bool         visible         { get; set; }
        public int          viewId          { get; set; }
        public string       assetPath       { get; set; }

        public void OnViewUpdate(ViewLayer layer, float alpha) {}
        public void OnEnter(ViewLayer prevLayer, ViewLayer curLayer) {}
        public void OnLeave(ViewLayer curLayer, ViewLayer nextLayer) {}
    }
}
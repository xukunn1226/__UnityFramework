using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class ActorManager
    {
        static private Dictionary<int, Actor> s_Actors = new Dictionary<int, Actor>();

        static public void CreateActor(int id)
        {
            Actor actor = new Actor();
            actor.id = id;
            actor.name = "Actor_" + id;
            actor.minViewLayer = ViewLayer.ViewLayer_0;
            actor.maxViewLayer = ViewLayer.ViewLayer_2;
            actor.visible = true;
            s_Actors.Add(id, actor);
        }
    }

    public interface IEntity
    {
        string  name    { get; set; }
        int     id      { get; set; }
    }

    public class Actor : IEntity, IViewLayer
    {
        public string       name            { get; set; } = "Actor";
        public int          id              { get; set; }
        public ViewLayer    minViewLayer    { get; set; } = ViewLayer.ViewLayer_0;
        public ViewLayer    maxViewLayer    { get; set; } = ViewLayer.ViewLayer_1;
        public bool         visible         { get; set; }
        public int          viewId          { get; set; }

        public void OnViewUpdate(ViewLayer layer, float alpha) {}
        public void OnEnter(ViewLayer prevLayer, ViewLayer curLayer) {}
        public void OnLeave(ViewLayer curLayer, ViewLayer nextLayer) {}
    }
}
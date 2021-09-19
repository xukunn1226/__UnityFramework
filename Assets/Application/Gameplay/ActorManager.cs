using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class ActorManager
    {
        static private List<Actor> s_Actors = new List<Actor>();

        static public void CreateActor()
        {
            Actor actor = new Actor();
            s_Actors.Add(actor);
        }
    }

    public interface IEntity
    {
        string name { get; set; }
    }

    public class Actor : IEntity, IViewActor
    {
        public string       name            { get; set; } = "Actor";
        public ViewLayer    minViewLayer    { get; set; } = ViewLayer.ViewLayer_0;
        public ViewLayer    maxViewLayer    { get; set; } = ViewLayer.ViewLayer_1;
        public bool         visible         { get; set; }
        public int          viewId          { get; set; }

        public void OnViewUpdate(ViewLayer layer, float alpha) {}
        public void OnEnter(ViewLayer prevLayer, ViewLayer curLayer) {}
        public void OnLeave(ViewLayer curLayer, ViewLayer nextLayer) {}
    }
}
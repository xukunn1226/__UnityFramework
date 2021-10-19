using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class ViewLayerManager : Singleton<ViewLayerManager>
    {
        private int                                     s_Id            = 0;
        private Dictionary<int, ViewLayerComponent>[]   s_ViewActorList;                                // [ViewLayer][]
        private ViewLayer                               s_PrevLayer     = ViewLayer.ViewLayer_Invalid;
        private ViewLayer                               s_CurLayer      = ViewLayer.ViewLayer_Invalid;

        public int AddInstance(ViewLayerComponent actor)
        {
            int id = s_Id++;
            for(int layer = (int)actor.minViewLayer; layer <= (int)actor.maxViewLayer; ++layer)
            {
#if UNITY_EDITOR
                if(s_ViewActorList[layer].ContainsValue(actor))
                    throw new System.ArgumentException("Failed to AddInstance: ViewActor has exist");
#endif                
                s_ViewActorList[layer].Add(id, actor); 
            }
            
            actor.OnEnter(s_PrevLayer, s_CurLayer);                         // s_PrevLayer可能为Invalid，因为可能没有切换至其他区间
            return id;
        }

        public void RemoveInstance(ViewLayerComponent actor)
        {
#if UNITY_EDITOR
            if(s_ViewActorList == null)
                throw new System.ArgumentException("RemoveInstance: s_ViewActorList == null");
#endif
            actor.OnLeave(s_CurLayer, ViewLayer.ViewLayer_Invalid);
            for(int layer = (int)actor.minViewLayer; layer <= (int)actor.maxViewLayer; ++layer)
            {
#if UNITY_EDITOR
                if(!s_ViewActorList[layer].ContainsKey(actor.id))
                    throw new System.ArgumentException($"Failed to RemoveInstance: ID {actor.id} has not exist");
#endif                
                s_ViewActorList[layer].Remove(actor.id);
            }
        }
        
        protected override void Init()
        {
            if(s_ViewActorList == null)
            {
                int countOfLayer = (int)ViewLayer.ViewLayer_Max;
                s_ViewActorList = new Dictionary<int, ViewLayerComponent>[countOfLayer];
                for(int i = 0; i < countOfLayer; ++i)
                {
                    s_ViewActorList[i] = new Dictionary<int, ViewLayerComponent>();
                }
            }
        } 

        protected override void OnUpdate(float deltaTime)
        {
            if(WorldPlayerController.Instance == null)
                return;

            if(s_ViewActorList == null)
            { // 尚未有对象加入
                return;
            }

            ViewLayer layer = ((WorldPlayerController)PlayerController.Instance).cameraViewLayer;
            float alpha = ((WorldPlayerController)PlayerController.Instance).cameraViewLayerAlpha;

            Dictionary<int, ViewLayerComponent> dict;
            if(s_CurLayer != layer)
            {
                // fire OnLeave event
                if(s_CurLayer != ViewLayer.ViewLayer_Invalid)
                {
                    dict = s_ViewActorList[(int)s_CurLayer];
                    foreach (var item in dict)
                    {
                        item.Value.OnLeave(s_CurLayer, layer);
                    }
                }

                s_PrevLayer = s_CurLayer;
                s_CurLayer = layer;

                // fire OnEnter event
                dict = s_ViewActorList[(int)s_CurLayer];
                foreach(var item in dict)
                {
                    item.Value.OnEnter(s_PrevLayer, s_CurLayer);
                }
            }
            else
            {
                Debug.Assert(s_CurLayer != ViewLayer.ViewLayer_Invalid);
                dict = s_ViewActorList[(int)s_CurLayer];
                foreach(var item in dict)
                {
                    item.Value.OnViewUpdate(s_CurLayer, alpha);
                }
            }
        }
    }
}
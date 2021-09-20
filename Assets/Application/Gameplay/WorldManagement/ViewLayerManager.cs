using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class ViewLayerManager
    {
        static private int                              s_Id            = 0;
        static private Dictionary<int, IViewLayer>[]    s_ViewActorList;                                // [ViewLayer][]
        static private ViewLayer                        s_PrevLayer     = ViewLayer.ViewLayer_Invalid;
        static private ViewLayer                        s_CurLayer      = ViewLayer.ViewLayer_Invalid;

        static private void Init()
        {
            if(s_ViewActorList == null)
            {
                int countOfLayer = (int)ViewLayer.ViewLayer_Max;
                s_ViewActorList = new Dictionary<int, IViewLayer>[countOfLayer];
                for(int i = 0; i < countOfLayer; ++i)
                {
                    s_ViewActorList[i] = new Dictionary<int, IViewLayer>();
                }
            }
        }

        static public void AddInstance(IViewLayer actor)
        {
            Init();

            for(int layer = (int)actor.minViewLayer; layer <= (int)actor.maxViewLayer; ++layer)
            {
#if UNITY_EDITOR
                if(s_ViewActorList[layer].ContainsValue(actor))
                    throw new System.ArgumentException("Failed to AddInstance: ViewActor has exist");
#endif                
                actor.viewId = s_Id;
                s_ViewActorList[layer].Add(s_Id, actor); 
            }
            ++s_Id;
            
            Debug.Assert(s_CurLayer != ViewLayer.ViewLayer_Invalid);        // 如果Update执行，则s_CurLayer必定处于有效区间
            actor.OnEnter(s_PrevLayer, s_CurLayer);                         // s_PrevLayer可能为Invalid，因为可能没有切换至其他区间
        }

        static public void RemoveInstance(IViewLayer actor)
        {
#if UNITY_EDITOR
            if(s_ViewActorList == null)
                throw new System.ArgumentException("RemoveInstance: s_ViewActorList == null");
#endif            
            for(int layer = (int)actor.minViewLayer; layer <= (int)actor.maxViewLayer; ++layer)
            {
#if UNITY_EDITOR
                if(!s_ViewActorList[layer].ContainsKey(actor.viewId))
                    throw new System.ArgumentException($"Failed to RemoveInstance: ID {actor.viewId} has not exist");
#endif
                actor.OnLeave(s_CurLayer, ViewLayer.ViewLayer_Invalid);
                s_ViewActorList[layer].Remove(actor.viewId);
            }
        }

        static public void Update(ViewLayer layer, float alpha)
        {
            if(s_ViewActorList == null)
            { // 尚未有对象加入
                return;
            }

            Dictionary<int, IViewLayer> dict;
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
                    if(item.Value.visible)
                        item.Value.OnViewUpdate(s_CurLayer, alpha);
                }
            }
        }
    }
}
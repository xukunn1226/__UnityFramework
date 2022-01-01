using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class ViewLayerManager : Singleton<ViewLayerManager>
    {
        private int                                     m_Id            = 0;
        private Dictionary<int, ViewLayerComponent>[]   m_ViewActorList;                                // [ViewLayer][]
        private ViewLayer                               m_PrevLayer     = ViewLayer.ViewLayer_Invalid;
        private ViewLayer                               m_CurLayer      = ViewLayer.ViewLayer_Invalid;
        private ViewLayer                               m_CameraViewLayer;
        private float                                   m_CameraViewLayerAlpha;

        public int AddInstance(ViewLayerComponent actor)
        {
            int id = m_Id++;
            for(int layer = (int)actor.minViewLayer; layer <= (int)actor.maxViewLayer; ++layer)
            {
#if UNITY_EDITOR
                if(m_ViewActorList[layer].ContainsValue(actor))
                    throw new System.ArgumentException("Failed to AddInstance: ViewActor has exist");
#endif                
                m_ViewActorList[layer].Add(id, actor); 
            }
            
            actor.OnEnter(m_PrevLayer, m_CurLayer);                         // s_PrevLayer可能为Invalid，因为可能没有切换至其他区间
            return id;
        }

        public void RemoveInstance(ViewLayerComponent actor)
        {
#if UNITY_EDITOR
            if(m_ViewActorList == null)
                throw new System.ArgumentException("RemoveInstance: s_ViewActorList == null");
#endif
            actor.OnLeave(m_CurLayer, ViewLayer.ViewLayer_Invalid);
            for(int layer = (int)actor.minViewLayer; layer <= (int)actor.maxViewLayer; ++layer)
            {
#if UNITY_EDITOR
                if(!m_ViewActorList[layer].ContainsKey(actor.id))
                    throw new System.ArgumentException($"Failed to RemoveInstance: ID {actor.id} has not exist");
#endif                
                m_ViewActorList[layer].Remove(actor.id);
            }
        }
        
        protected override void InternalInit()
        {
            if(m_ViewActorList == null)
            {
                int countOfLayer = (int)ViewLayer.ViewLayer_Max;
                m_ViewActorList = new Dictionary<int, ViewLayerComponent>[countOfLayer];
                for(int i = 0; i < countOfLayer; ++i)
                {
                    m_ViewActorList[i] = new Dictionary<int, ViewLayerComponent>();
                }
            }

            WorldPlayerController.onViewLayerUpdate += OnViewLayerUpdate;
        }

        protected override void OnDestroy()
        {
            WorldPlayerController.onViewLayerUpdate -= OnViewLayerUpdate;
            base.OnDestroy();
        }

        private void OnViewLayerUpdate(ViewLayer layer, float alpha)
        {
            m_CameraViewLayer = layer;
            m_CameraViewLayerAlpha = alpha;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if(WorldPlayerController.Instance == null)
                return;

            if(m_ViewActorList == null)
            { // 尚未有对象加入
                return;
            }

            // ViewLayer layer = ((WorldPlayerController)PlayerController.Instance).cameraViewLayer;
            // float alpha = ((WorldPlayerController)PlayerController.Instance).cameraViewLayerAlpha;

            Dictionary<int, ViewLayerComponent> dict;
            if(m_CurLayer != m_CameraViewLayer)
            {
                // fire OnLeave event
                if(m_CurLayer != ViewLayer.ViewLayer_Invalid)
                {
                    dict = m_ViewActorList[(int)m_CurLayer];
                    foreach (var item in dict)
                    {
                        item.Value.OnLeave(m_CurLayer, m_CameraViewLayer);
                    }
                }

                m_PrevLayer = m_CurLayer;
                m_CurLayer = m_CameraViewLayer;

                // fire OnEnter event
                dict = m_ViewActorList[(int)m_CurLayer];
                foreach(var item in dict)
                {
                    item.Value.OnEnter(m_PrevLayer, m_CurLayer);
                }
            }
            else
            {
                Debug.Assert(m_CurLayer != ViewLayer.ViewLayer_Invalid);
                dict = m_ViewActorList[(int)m_CurLayer];
                foreach(var item in dict)
                {
                    item.Value.OnViewUpdate(m_CurLayer, m_CameraViewLayerAlpha);
                }
            }
        }
    }
}
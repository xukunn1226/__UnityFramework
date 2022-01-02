using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimationInstancingModule.Runtime;
using Framework.AssetManagement.Runtime;
using Application.Runtime;

namespace Application.Logic
{
    /// <summary>
    /// 测试用例，设计显示规则如下：
    /// ViewLayer_0: AnimationInstancing.LOD0
    /// ViewLayer_1: AnimationInstancing.LOD1
    /// ViewLayer_2: Sphere
    /// <summary>
    public class TestActor : ZActor
    {
        public int                      id                  { get; set; }
        public Vector3                  startPosition;
        public Vector3                  startRotation;
        private ViewLayerComponent      m_ViewLayer;
        private TestRenderableProfile   m_RenderableProxy;
        private LocomotionAgent         m_LocomotionAgent;
        private AISimple                m_AI;

        public override void Prepare(IDataSource data = null)
        {
            base.Prepare(data);
            
            // 注意组件初始化的顺序（部分组件之间有依赖关系）
            m_ViewLayer = AddComponent<ViewLayerComponent>(data);            
            m_ViewLayer.minViewLayer = ViewLayer.ViewLayer_0;
            m_ViewLayer.maxViewLayer = ViewLayer.ViewLayer_2;

            // 移动
            m_LocomotionAgent = AddComponent<LocomotionAgent>(data);
            m_LocomotionAgent.startPosition = startPosition;
            m_LocomotionAgent.startRotation = startRotation;

            // 显示组件靠后挂载
            m_RenderableProxy = AddComponent<TestRenderableProfile>(data);

            m_AI = AddComponent<AISimple>(data);
        }

        public override void Start()
        {
            base.Start();

            m_ViewLayer.Start();
            m_LocomotionAgent.Start();
            m_RenderableProxy.Start();
            m_AI.Start();
        }
    }
}
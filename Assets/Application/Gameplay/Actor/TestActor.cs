using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimationInstancingModule.Runtime;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
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
        private ViewLayerComp           m_ViewLayer;
        private TestRenderableProfile   m_RenderableProxy;

        public override void Prepare(IData data = null)
        {
            base.Prepare(data);
            
            // 注意组件初始化的顺序（部分组件之间有依赖关系）
            m_ViewLayer = AddComponent<ViewLayerComp>(data);            
            m_ViewLayer.minViewLayer = ViewLayer.ViewLayer_0;
            m_ViewLayer.maxViewLayer = ViewLayer.ViewLayer_2;
            m_RenderableProxy = AddComponent<TestRenderableProfile>(data);
        }

        public override void Start()
        {
            base.Start();

            m_ViewLayer.Start();
            m_RenderableProxy.Start();
        }
    }
}
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

        public override void Init()
        {
            base.Init();

            // 注意add component的顺序
            m_ViewLayer = AddComponent<ViewLayerComp>();
            m_RenderableProxy = AddComponent<TestRenderableProfile>();
        }
    }
}
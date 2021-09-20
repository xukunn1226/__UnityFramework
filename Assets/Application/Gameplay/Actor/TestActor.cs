using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimationInstancingModule.Runtime;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    /// <summary>
    /// 测试用例，故显示规则如下：
    /// ViewLayer_0: LOD0
    /// ViewLayer_1: LOD1
    /// ViewLayer_2: Sphere
    /// <summary>
    public class TestActor : IEntity, IViewLayer
    {
        public string       name            { get; set; }
        public int          id              { get; set; }
        public ViewLayer    minViewLayer    { get; set; } = ViewLayer.ViewLayer_0;
        public ViewLayer    maxViewLayer    { get; set; } = ViewLayer.ViewLayer_1;
        public bool         visible         { get; set; }
        public int          viewId          { get; set; }
        public string       assetPath       { get; set; }
        public GameObject   display         { get; set; }           // the root gameobject of any display object
        private GameObjectLoaderAsync m_LoaderAsync;

        public void Init()
        {
            display = new GameObject();
        }

        public void Uninit()
        {
            if(display != null)
            {
                Object.Destroy(display);
            }
        }
        public void OnViewUpdate(ViewLayer layer, float alpha) { }
        public void OnEnter(ViewLayer prevLayer, ViewLayer curLayer)
        {
            // 请求当前层级的显示对象
            RequestLoad(Load());
        }

        public void OnLeave(ViewLayer curLayer, ViewLayer nextLayer)
        {
            RequestUnload(Unload);
        }

        private void RequestLoad(IEnumerator func)
        {

        }

        private void RequestUnload(System.Action evt)
        {

        }

        public IEnumerator Load()
        {
            m_LoaderAsync = ResourceManager.InstantiateAsync(assetPath);
            yield return m_LoaderAsync;
        }

        public void Unload()
        {

        }
    }
}
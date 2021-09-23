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
    public class TestActor : IEntity, IViewLayer
    {
        public string                   name                { get; set; }
        public int                      id                  { get; set; }
        public ViewLayer                minViewLayer        { get; set; } = ViewLayer.ViewLayer_0;
        public ViewLayer                maxViewLayer        { get; set; } = ViewLayer.ViewLayer_1;
        public bool                     visible             { get; set; }
        public int                      viewId              { get; set; }
        public string                   decorateAssetPath   { get; set; }
        public string                   symbolAssetPath     { get; set; }
        public GameObject               display             { get; set; }           // the root gameobject of any display object
        private RendererBase            m_DecorateRenderer;
        private RendererBase            m_SymbolRenderer;
        private RendererBase            m_CurRenderer;

        public void Init()
        {
#if UNITY_EDITOR            
            display = new GameObject(name);
#else
            display = new GameObject();
#endif            
            display.transform.position = new Vector3(Random.Range(-1.0f, 1.0f) * 50, 0, Random.Range(-1.0f, 1.0f) * 50);
            display.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        }

        public void Uninit()
        {
            m_DecorateRenderer?.Unload();
            m_SymbolRenderer?.Unload();

            if(display != null)
            {
                Object.Destroy(display);
            }
        }

        private RendererBase GetOrCreateDecorateRenderer(Transform parent)
        {
            if(m_DecorateRenderer == null && !string.IsNullOrEmpty(decorateAssetPath))
            {
                m_DecorateRenderer = new DecorateRenderer(decorateAssetPath);
                m_DecorateRenderer.Load();
                m_DecorateRenderer.renderer.transform.SetParent(parent, false);
            }
            return m_DecorateRenderer;
        }

        private RendererBase GetOrCreateSymbolRenderer(Transform parent)
        {
            if(m_SymbolRenderer == null && !string.IsNullOrEmpty(symbolAssetPath))
            {
                m_SymbolRenderer = new SymbolRenderer(symbolAssetPath);
                m_SymbolRenderer.Load();
                m_SymbolRenderer.renderer.transform.SetParent(parent, false);
            }
            return m_SymbolRenderer;
        }

        public void OnViewUpdate(ViewLayer layer, float alpha) { }
        public void OnEnter(ViewLayer prevLayer, ViewLayer curLayer)
        {
            if(prevLayer == ViewLayer.ViewLayer_Invalid && curLayer == ViewLayer.ViewLayer_Invalid)
                return;     // ViewLayerManager尚未Update时可能出现
            
            if(curLayer == ViewLayer.ViewLayer_0 || curLayer == ViewLayer.ViewLayer_1)
            { // 0与1层使用资源decorate
                m_CurRenderer = GetOrCreateDecorateRenderer(display.transform);
            }            
            else if(curLayer == ViewLayer.ViewLayer_2)
            { // 2层使用资源symbol
                m_CurRenderer = GetOrCreateSymbolRenderer(display.transform);
            }
            else
            {
                m_CurRenderer = null;
            }

            if(curLayer == ViewLayer.ViewLayer_0 || curLayer == ViewLayer.ViewLayer_1)
            {
                SetRendererVisible(true, false);
            }
            else if(curLayer == ViewLayer.ViewLayer_2)
            {
                SetRendererVisible(false, true);
            }
            else
            {
                SetRendererVisible(false, false);      
            }
        }

        public void OnLeave(ViewLayer curLayer, ViewLayer nextLayer)
        {
        }

        private void SetRendererVisible(bool decorateRendererVisible, bool symbolRendererVisible)
        {
            if(m_DecorateRenderer != null)
            {
                m_DecorateRenderer.enable = decorateRendererVisible;
            }
            if(m_SymbolRenderer != null)
            {
                m_SymbolRenderer.enable = symbolRendererVisible;
            }
        }

        // public IEnumerator LoadActorPrefabAsync()
        // {
        //     m_LoaderAsync = ResourceManager.InstantiateAsync(decorateAssetPath);
        //     yield return m_LoaderAsync;
        // }

        // private void OnActorPrefabLoadFinished()
        // {

        // }
    }
}
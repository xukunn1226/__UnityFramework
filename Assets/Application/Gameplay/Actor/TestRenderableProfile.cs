using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    /// <summary>
    /// 不同层级视角下角色表现方式
    /// <summary>
    public class TestRenderableProfile : ZComp
    {
        static private string[] s_AssetPathList = new string[] {"assets/res/players/zombie_01_variant.prefab",
                                                                "assets/res/players/zombie_02_variant.prefab"};

        public GameObject       root                { get; private set; }
        private Renderable      m_CurRenderer;
        private Renderable      m_DecorateRenderer;
        private Renderable      m_SymbolRenderer;
        private string          m_DecorateAssetPath;
        private string          m_SymbolAssetPath;

        public TestRenderableProfile(ZActor actor) : base(actor) {}

        public override void Start()
        {
            base.Start();

#if UNITY_EDITOR            
            root = new GameObject(name);
#else
            root = new GameObject();
#endif            
            root.transform.position = new Vector3(Random.Range(-1.0f, 1.0f) * 50, 0, Random.Range(-1.0f, 1.0f) * 50);
            root.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);

            m_DecorateAssetPath = s_AssetPathList[Random.Range(0, 2)];
            m_SymbolAssetPath = "assets/res/players/symbol.prefab";

            ViewLayerComp viewLayer = actor.GetComponent<ViewLayerComp>();
            // viewLayer.minViewLayer = ViewLayer.ViewLayer_0;
            // viewLayer.maxViewLayer = ViewLayer.ViewLayer_2;
            if(viewLayer == null)
            {
                Debug.LogError("TestRenderableProfile.Init: dependent on ViewLayerComp, but can't find it");
            }
            else
            {
                viewLayer.onEnter += OnEnter;
                viewLayer.onViewUpdate += OnViewUpdate;
            }
        }

        public override void Destroy()
        {
            ViewLayerComp viewLayer = actor.GetComponent<ViewLayerComp>();
            if(viewLayer == null)
            {
                Debug.LogError("TestRenderableProfile.Uninit: dependent on ViewLayerComp, but can't find it");
            }
            else
            {
                viewLayer.onEnter -= OnEnter;
                viewLayer.onViewUpdate -= OnViewUpdate;
            }
            base.Destroy();
        }

        public void OnViewUpdate(ViewLayer layer, float alpha) { }
        public void OnEnter(ViewLayer prevLayer, ViewLayer curLayer)
        {
            if(prevLayer == ViewLayer.ViewLayer_Invalid && curLayer == ViewLayer.ViewLayer_Invalid)
                return;     // ViewLayerManager尚未Update时可能出现
            
            if(curLayer == ViewLayer.ViewLayer_0 || curLayer == ViewLayer.ViewLayer_1)
            { // 0与1层使用资源decorate
                m_CurRenderer = GetOrCreateDecorateRenderer(root.transform);
            }            
            else if(curLayer == ViewLayer.ViewLayer_2)
            { // 2层使用资源symbol
                m_CurRenderer = GetOrCreateSymbolRenderer(root.transform);
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
        
        private Renderable GetOrCreateDecorateRenderer(Transform parent)
        {
            if(m_DecorateRenderer == null && !string.IsNullOrEmpty(m_DecorateAssetPath))
            {
                m_DecorateRenderer = actor.AddComponent<AnimationInstancingRenderable>();
                m_DecorateRenderer.Load(m_DecorateAssetPath);
                m_DecorateRenderer.renderer.transform.SetParent(parent, false);
            }
            return m_DecorateRenderer;
        }

        private Renderable GetOrCreateSymbolRenderer(Transform parent)
        {
            if(m_SymbolRenderer == null && !string.IsNullOrEmpty(m_SymbolAssetPath))
            {
                m_SymbolRenderer = actor.AddComponent<Renderable>();
                m_SymbolRenderer.Load(m_SymbolAssetPath);
                m_SymbolRenderer.renderer.transform.SetParent(parent, false);
            }
            return m_SymbolRenderer;
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
    }
}
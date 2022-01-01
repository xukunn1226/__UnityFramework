using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Application.Runtime;

namespace Application.HotFix
{
    /// <summary>
    /// 不同层级视角下角色表现方式
    /// <summary>
    public class TestRenderableProfile : ZComp
    {
        static private string[] s_AssetPathList = new string[] {"assets/res/players/zombie_01_variant.prefab",
                                                                "assets/res/players/zombie_02_variant.prefab"};

        public GameObject                       root                { get; private set; }
        public Renderable                       curRenderer         { get; private set; }
        private AnimationInstancingRenderable   m_DecorateRenderer;
        private Renderable                      m_SymbolRenderer;
        private string                          m_DecorateAssetPath;
        private string                          m_SymbolAssetPath;
        private LocomotionAgent                 m_Agent;

        public TestRenderableProfile(ZActor actor) : base(actor) {}

        public override void Prepare(IDataSource data)
        {
            base.Prepare(data);
            m_DecorateAssetPath = s_AssetPathList[Random.Range(0, s_AssetPathList.Length)];
            m_SymbolAssetPath = "assets/res/players/symbol.prefab";

            // prepare LocomotionAgent
            m_Agent = actor.GetComponent<LocomotionAgent>();
            if(m_Agent == null)
            {
                Debug.LogError("TestRenderableProfile.Prepare: can't find LocomotionAgent, plz check it");
            }
            else
            {
                m_Agent.onEnterView += OnEnterView;
            }

            // prepare ViewLayer
            ViewLayerComponent viewLayer = actor.GetComponent<ViewLayerComponent>();
            if(viewLayer == null)
            {
                Debug.LogError("TestRenderableProfile.Prepare: can't find ViewLayerComp, plz check it");
            }
            else
            {
                viewLayer.onViewUpdate += OnViewLayerUpdate;
            }

            m_DecorateRenderer = actor.AddComponent<AnimationInstancingRenderable>();
            m_SymbolRenderer = actor.AddComponent<Renderable>();
        }

        public override void Destroy()
        {
            ViewLayerComponent viewLayer = actor.GetComponent<ViewLayerComponent>();
            if(viewLayer == null)
            {
                Debug.LogError("TestRenderableProfile.Uninit: dependent on ViewLayerComp, but can't find it");
            }
            else
            {
                viewLayer.onViewUpdate -= OnViewLayerUpdate;
            }

            if(m_Agent != null)
                m_Agent.onEnterView -= OnEnterView;

            if(root != null)
                Object.Destroy(root);

            base.Destroy();
        }

        public void OnViewLayerUpdate(ViewLayer curLayer, float alpha)
        {
            if (curLayer == ViewLayer.ViewLayer_0 || curLayer == ViewLayer.ViewLayer_1)
            { // 0与1层使用资源decorate
                curRenderer = GetOrCreateDecorateRenderer(root != null ? root.transform : null);
            }
            else if (curLayer == ViewLayer.ViewLayer_2)
            { // 2层使用资源symbol
                curRenderer = GetOrCreateSymbolRenderer(root != null ? root.transform : null);
            }
            else
            {
                curRenderer = null;
            }

            // 因资源异步加载，此处从OnEnterLayer移至Update处理
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
            if(parent != null && m_DecorateRenderer.renderer == null && !string.IsNullOrEmpty(m_DecorateAssetPath))
            {
                m_DecorateRenderer.Load(m_DecorateAssetPath);
                m_DecorateRenderer.renderer.transform.SetParent(parent, false);
            }
            return m_DecorateRenderer;
        }

        private Renderable GetOrCreateSymbolRenderer(Transform parent)
        {
            if(parent != null && m_SymbolRenderer.renderer == null && !string.IsNullOrEmpty(m_SymbolAssetPath))
            {
                m_SymbolRenderer.Load(m_SymbolAssetPath);
                m_SymbolRenderer.renderer.transform.SetParent(parent, false);
            }
            return m_SymbolRenderer;
        }

        private void SetRendererVisible(bool decorateRendererVisible, bool symbolRendererVisible)
        {
            if(m_DecorateRenderer != null && m_DecorateRenderer.enable != decorateRendererVisible)
            {
                m_DecorateRenderer.enable = decorateRendererVisible;
            }
            if(m_SymbolRenderer != null && m_SymbolRenderer.enable != symbolRendererVisible)
            {
                m_SymbolRenderer.enable = symbolRendererVisible;
            }
        }

        public void PlayAnimation(string name, float transitionDuration = 0.2f, float playSpeed = 1)
        {
            m_DecorateRenderer.PlayAnimation(name, transitionDuration, playSpeed);
        }

        private void OnEnterView(Vector3 startPosition, Vector3 startRotation)
        {
#if UNITY_EDITOR            
            root = new GameObject(actor.name);
#else
            root = new GameObject();
#endif
            root.transform.position = startPosition;
            root.transform.eulerAngles = startRotation;
        }
    }
}
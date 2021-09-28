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

        public GameObject                       root                { get; private set; }
        public Renderable                       curRenderer         { get; private set; }
        private AnimationInstancingRenderable   m_DecorateRenderer;
        private Renderable                      m_SymbolRenderer;
        private string                          m_DecorateAssetPath;
        private string                          m_SymbolAssetPath;
        private LocomotionAgent                 m_Agent;

        public TestRenderableProfile(ZActor actor) : base(actor) {}

        public override void Prepare(IData data)
        {
            base.Prepare(data);
            m_DecorateAssetPath = s_AssetPathList[Random.Range(0, 2)];
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
            ViewLayerComp viewLayer = actor.GetComponent<ViewLayerComp>();
            if(viewLayer == null)
            {
                Debug.LogError("TestRenderableProfile.Prepare: can't find ViewLayerComp, plz check it");
            }
            else
            {
                viewLayer.onEnter += OnEnterLayer;
                viewLayer.onViewUpdate += OnLayerUpdate;
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
                viewLayer.onEnter -= OnEnterLayer;
                viewLayer.onViewUpdate -= OnLayerUpdate;
            }

            if(m_Agent != null)
                m_Agent.onEnterView -= OnEnterView;

            base.Destroy();
        }

        public void OnLayerUpdate(ViewLayer curLayer, float alpha)
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
        }

        public void OnEnterLayer(ViewLayer prevLayer, ViewLayer curLayer)
        {
            if(prevLayer == ViewLayer.ViewLayer_Invalid && curLayer == ViewLayer.ViewLayer_Invalid)
                return;     // ViewLayerManager尚未Update时可能出现

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
            if(parent != null && m_DecorateRenderer == null && !string.IsNullOrEmpty(m_DecorateAssetPath))
            {
                m_DecorateRenderer = actor.AddComponent<AnimationInstancingRenderable>();
                m_DecorateRenderer.Load(m_DecorateAssetPath);
                m_DecorateRenderer.renderer.transform.SetParent(parent, false);
            }
            return m_DecorateRenderer;
        }

        private Renderable GetOrCreateSymbolRenderer(Transform parent)
        {
            if(parent != null && m_SymbolRenderer == null && !string.IsNullOrEmpty(m_SymbolAssetPath))
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

        public void PlayAnimation(string name, float transitionDuration = 0)
        {
            m_DecorateRenderer?.PlayAnimation(name, transitionDuration);            
        }

        public void SetAnimationSpeed(float speed, float scale = 1)
        {
            m_DecorateRenderer?.SetAnimationSpeed(speed, scale);
        }

        private void OnEnterView(Vector3 startPosition, Vector3 startRotation)
        {
#if UNITY_EDITOR            
            root = new GameObject(name);
#else
            root = new GameObject();
#endif
            root.transform.position = startPosition;
            root.transform.eulerAngles = startRotation;
        }
    }
}
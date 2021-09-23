using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.Core;
using AnimationInstancingModule.Runtime;

namespace Application.Runtime
{
    public interface IRenderable
    {
        GameObject                  renderer    { get; }
        bool                        enable      { get; set; }
    }

    public abstract class RendererBase : IRenderable
    {
        public GameObject           renderer    { get; protected set; }
        public string               assetPath   { get; private set; }
        private GameObjectLoader    m_Loader;
        protected bool              m_Enable    = true;
        public virtual bool         enable
        {
            get { return m_Enable; }
            set
            {
                if(m_Enable != value)
                {
                    m_Enable = value;
                    renderer?.SetActive(value);
                }
            }
        }
        
        public RendererBase(string assetPath)
        {
            this.assetPath = assetPath;
        }
        
        public void Load()
        {
            if(m_Loader == null)
            {
                m_Loader = ResourceManager.Instantiate(assetPath);
                renderer = m_Loader.asset;
            }
        }

        public void Unload()
        {
            if(m_Loader != null)
            {
                ResourceManager.ReleaseInst(m_Loader);
                m_Loader = null;
            }
        }

        public virtual void PlayAnimation(string name, float transitionDuration = 0) {}
        public virtual void SetRendererSpeed(float speed) {}        // 显示对象的播放速度
    }

    public class DecorateRenderer : RendererBase
    {
        private AnimationInstancing m_Instancing;
        public AnimationInstancing instancing
        {
            get
            {
                if(m_Instancing == null)
                {
                    m_Instancing = renderer.GetComponent<AnimationInstancing>();
                    Debug.Assert(m_Instancing != null);
                }
                return m_Instancing;
            }
        }

        public DecorateRenderer(string assetPath) : base(assetPath)
        {}

        public override void PlayAnimation(string name, float transitionDuration = 0)
        {
            instancing?.PlayAnimation(name, transitionDuration);
        }

        public override void SetRendererSpeed(float speed)
        {
            if(instancing != null)
                instancing.speedScale = speed;
        }
    }

    public class SymbolRenderer : RendererBase
    {
        public SymbolRenderer(string assetPath) : base(assetPath)
        {}
    }
}
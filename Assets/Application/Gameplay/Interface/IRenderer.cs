using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{
    public interface IRenderer
    {
        GameObject          renderer    { get; }
        bool                enable      { get; set; }
    }

    public abstract class RendererBase : IRenderer
    {
        public GameObject   renderer    { get; protected set; }
        public string       assetPath   { get; private set; }
        protected bool      m_Enable    = true;
        public virtual bool enable
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

        public abstract void Load();
        public abstract void Unload();
    }

    public class DecorateRenderer : RendererBase
    {
        private GameObjectLoader    m_Loader;

        public DecorateRenderer(string assetPath) : base(assetPath)
        {}

        public override void Load()
        {
            if(m_Loader == null)
            {
                m_Loader = ResourceManager.Instantiate(assetPath);
                renderer = m_Loader.asset;
            }
        }

        public override void Unload()
        {
            if(m_Loader != null)
            {
                ResourceManager.ReleaseInst(m_Loader);
                m_Loader = null;
            }
        }
    }

    public class SymbolRenderer : RendererBase
    {
        private GameObjectLoader    m_Loader;

        public SymbolRenderer(string assetPath) : base(assetPath)
        {}

        public override void Load()
        {
            if(m_Loader == null)
            {
                m_Loader = ResourceManager.Instantiate(assetPath);
                renderer = m_Loader.asset;
            }
        }

        public override void Unload()
        {
            if(m_Loader != null)
            {
                ResourceManager.ReleaseInst(m_Loader);
                m_Loader = null;
            }
        }
    }
}
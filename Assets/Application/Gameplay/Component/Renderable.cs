using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace Application.Runtime
{    
    public class Renderable : ZComp
    {
        public GameObject           renderer    { get; private set; }
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

        private string              m_AssetPath;
        private GameObjectLoader    m_Loader;

        public Renderable(ZActor actor) : base(actor) {}

        public override void Destroy()
        {
            Unload();
            base.Destroy();
        }
        
        public virtual void Load(string assetPath)
        {
#if UNITY_EDITOR
            if(string.IsNullOrEmpty(assetPath))
                throw new System.ArgumentNullException("Renderable.Load");
#endif
            if(m_Loader == null && string.IsNullOrEmpty(m_AssetPath))
            {
                m_AssetPath = assetPath;
                m_Loader = ResourceManager.Instantiate(m_AssetPath);
                renderer = m_Loader.asset;
            }
            else
            {
                throw new System.ArgumentException($"can't call Load() again");
            }
        }

        public virtual void Unload()
        {
            if(m_Loader != null)
            {
                ResourceManager.ReleaseInst(m_Loader);
                m_Loader = null;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace Application.Logic
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
                    if(renderer != null)
                        renderer.SetActive(value);
                }
            }
        }

        private string              m_AssetPath;
        private PrefabOperationHandle m_Handle;

        public Renderable() {}
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
            if(m_Handle == null && string.IsNullOrEmpty(m_AssetPath))
            {
                m_AssetPath = assetPath;
                m_Handle = AssetManagerEx.LoadPrefab(m_AssetPath);
                renderer = m_Handle.gameObject;
            }
            else
            {
                throw new System.ArgumentException($"can't call Load() again");
            }
        }

        public virtual void Unload()
        {
            if(m_Handle != null)
            {
                m_Handle.Release();
                m_Handle = null;
            }
        }
    }
}
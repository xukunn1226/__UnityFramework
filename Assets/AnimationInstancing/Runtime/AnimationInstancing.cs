using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationInstancing : MonoBehaviour
    {
        public AnimationData            prototype;
        public List<RendererCache>      rendererCacheList   = new List<RendererCache>();

        public Transform                worldTransform      { get; private set; }
        public float                    speed               = 1.0f;
        public float                    radius              = 1.0f;
        private BoundingSphere          m_BoundingSphere;
        [NonSerialized] public int      layer;
        [NonSerialized] public bool     visible             = true;

        private void Awake()
        {
            worldTransform = GetComponent<Transform>();
            m_BoundingSphere = new BoundingSphere(worldTransform.position, radius);
            layer = gameObject.layer;

            // register
        }

        private void OnDestroy()
        {
            // unregister
        }

        private void OnEnable()
        {
            visible = true;
        }

        private void OnDisable()
        {
            visible = false;
        }
    }
}
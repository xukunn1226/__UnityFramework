using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Scale : MonoBehaviour
    {
        public bool             Addictive;

        public float            delay;
        public float            duration;
        public bool             Loop;

        public Vector3          target = Vector3.one;
        public AnimationCurve   curve = new AnimationCurve(FX_Const.defaultKeyFrames);

        private float           _duration;
        private float           _delay;
        private Vector3         _originalLocalScale;

        private void Awake()
        {
            _originalLocalScale = transform.localScale;         // 记录初始localScale
        }
        
        private void OnEnable()
        {
            transform.localScale = _originalLocalScale;         // 恢复初始localScale
            
            _duration = duration;
            _delay = delay;
        }

        void Update()
        {
            _delay -= Time.deltaTime;
            if (_delay <= 0)
            {
                _duration -= Time.deltaTime;
                float percent = 1;
                if (_duration >= 0)
                {
                    percent = 1 - (_duration / duration);
                }
                else
                {
                    if (Loop)
                    {
                        _duration += duration;
                        percent = 1 - (_duration / duration);
                    }
                }
                float value = Mathf.Clamp01(curve.Evaluate(percent));
                if (Addictive)
                    transform.localScale = Vector3.Lerp(_originalLocalScale, _originalLocalScale + target, value);
                else
                    transform.localScale = Vector3.Lerp(_originalLocalScale, target, value);
            }
        }
    }
}

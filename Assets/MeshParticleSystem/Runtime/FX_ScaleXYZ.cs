using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_ScaleXYZ : MonoBehaviour
    {
        public bool             Addictive;

        public float            delay;
        public float            duration;
        public bool             Loop;
        
        public Vector3          target = Vector3.one;
        public AnimationCurve   curveX = new AnimationCurve(FX_Const.defaultKeyFrames);
        public AnimationCurve   curveY = new AnimationCurve(FX_Const.defaultKeyFrames);
        public AnimationCurve   curveZ = new AnimationCurve(FX_Const.defaultKeyFrames);
        
        private float           _duration;
        private float           _delay;
        private Vector3         _orignalLocalScale;
        private Vector3         _localScale = Vector3.one;

        private void Awake()
        {
            _orignalLocalScale = transform.localScale;              // 记录初始localScale
        }

        private void OnEnable()
        {
            transform.localScale = _orignalLocalScale;              // 恢复初始localScale

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

                float valueX = Mathf.Clamp01(curveX.Evaluate(percent));
                float valueY = Mathf.Clamp01(curveY.Evaluate(percent));
                float valueZ = Mathf.Clamp01(curveZ.Evaluate(percent));
                
                if (Addictive)
                {
                    _localScale.x = Mathf.Lerp(_orignalLocalScale.x, _orignalLocalScale.x + target.x, valueX);
                    _localScale.y = Mathf.Lerp(_orignalLocalScale.y, _orignalLocalScale.y + target.y, valueY);
                    _localScale.z = Mathf.Lerp(_orignalLocalScale.z, _orignalLocalScale.z + target.z, valueZ);
                }
                else
                {
                    _localScale.x = Mathf.Lerp(_orignalLocalScale.x, target.x, valueX);
                    _localScale.y = Mathf.Lerp(_orignalLocalScale.y, target.y, valueY);
                    _localScale.z = Mathf.Lerp(_orignalLocalScale.z, target.z, valueZ);
                }

                transform.localScale = _localScale;
            }
        }
    }
}

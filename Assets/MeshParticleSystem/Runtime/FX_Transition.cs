using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Transition : MonoBehaviour
    {
        public bool             addictive = true;
        
        public float            delay;
        public float            duration;
        public bool             Loop;

        public Vector3          target;
        public bool             WorldSpace;        
        public AnimationCurve   curve = new AnimationCurve(FX_Const.defaultKeyFrames);
        
        private float           _duration;
        private float           _delay;
        private Vector3         _originalLocalPos;
        private Vector3         _originalWorldPos;
        private Vector3         _startLocalPos;
        private Vector3         _startWorldPos;

        private void Awake()
        {
            _originalLocalPos = transform.localPosition;
            _originalWorldPos = transform.position;
        }

        void OnEnable()
        {
            // 恢复初始位置
            transform.position = _originalWorldPos;
            transform.localPosition = _originalLocalPos;

            // 重复使用时需要先设置到正确位置，再active
            _startLocalPos = transform.localPosition;
            _startWorldPos = transform.position;

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
                        _duration = duration;
                        percent = 1 - (_duration / duration);
                    }
                }

                float value = curve.Evaluate(percent);

                if (WorldSpace)
                {
                    if (addictive)
                        transform.position = Vector3.Lerp(_startWorldPos, _startWorldPos + target, value);
                    else
                        transform.position = Vector3.Lerp(_startWorldPos, target, value);
                }
                else
                {
                    if (addictive)
                        transform.localPosition = Vector3.Lerp(_startLocalPos, _startLocalPos + target, value);
                    else
                        transform.localPosition = Vector3.Lerp(_startLocalPos, target, value);
                }
            }
        }
    }
}

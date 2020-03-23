using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshParticleSystem
{
    public class FX_Rotation : MonoBehaviour
    {
        public bool             Addictive;

        public float            delay;
        public float            duration;
        public bool             Loop;

        public Vector3          target;
        public bool             WorldSpace;
        public AnimationCurve   curve = new AnimationCurve(FX_Const.defaultKeyFrames);

        public bool             lateUpdate;

        private float           _duration;
        private float           _delay;
        private Vector3         _originalLocalEuler;
        private Vector3         _originalWorldEuler;
        private Vector3         _startLocalEuler;
        private Vector3         _startWorldEuler;
        private Vector3         _localEuler;
        private Vector3         _worldEuler;

        private void Awake()
        {
            _originalLocalEuler = transform.localEulerAngles;
            _originalWorldEuler = transform.eulerAngles;
        }

        private void OnEnable()
        {
            transform.eulerAngles = _originalWorldEuler;
            transform.localEulerAngles = _originalLocalEuler;

            // 重复使用时需要先设置到正确位置，再active
            _startLocalEuler = transform.localEulerAngles;
            _startWorldEuler = transform.eulerAngles;

            _duration = duration;
            _delay = delay;
        }

        void UpdateDataInternal()
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

                float value = curve.Evaluate(percent);
                if (WorldSpace)
                {
                    if (Addictive)
                        _worldEuler = Vector3.Lerp(_startWorldEuler, _startWorldEuler + target, value);
                    else
                        _worldEuler = Vector3.Lerp(_startWorldEuler, target, value);
                    transform.eulerAngles = _worldEuler;
                }
                else
                {
                    if (Addictive)
                        _localEuler = Vector3.Lerp(_startLocalEuler, _startLocalEuler + target, value);
                    else
                        _localEuler = Vector3.Lerp(_startLocalEuler, target, value);
                    transform.localEulerAngles = _localEuler;
                }
            }
        }

        void Update()
        {
            if (!lateUpdate)
            {
                UpdateDataInternal();
            }            
        }
        
        private void LateUpdate()
        {
            if (lateUpdate)
            {
                UpdateDataInternal();
            }
        }
    }
}

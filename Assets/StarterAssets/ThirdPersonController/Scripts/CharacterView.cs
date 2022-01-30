using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class CharacterView : MonoBehaviour
    {
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private bool _hasAnimator;

        // animation IDs
		private int _animIDSpeed;
		private int _animIDGrounded;
		private int _animIDJump;
		private int _animIDFreeFall;
		private int _animIDMotionSpeed;

        private void Awake()
		{
		}

        // Start is called before the first frame update
        void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();

			AssignAnimationIDs();
        }
        
		private void AssignAnimationIDs()
		{
			_animIDSpeed = Animator.StringToHash("Speed");
			_animIDGrounded = Animator.StringToHash("Grounded");
			_animIDJump = Animator.StringToHash("Jump");
			_animIDFreeFall = Animator.StringToHash("FreeFall");
			_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}

        // Update is called once per frame
        void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);
        }

        public void SetMotion(Vector3 motion)
        {
            if(_controller)
            {
                _controller.Move(motion);
            }
        }

        /// <summary>
        /// idle，walk，run的融合系数，通过控制此参数模拟出慢跑，小步跑等中间状态，甚至从idle到run的加速过程
        /// idle speed: 0
        /// walk speed: 2
        /// run speed: 5.335
        /// </summary>
        public void SetSpeed(float value)
        {
            if(_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, value);
            }
        }

        /// <summary>
        /// multiplier for Speed
        /// </summary>
        public void SetMotionSpeed(float value)
        {
            if(_hasAnimator)
            {
                _animator.SetFloat(_animIDMotionSpeed, value);
            }
        }

        /// <summary>
        /// 浮空状态到落地
        /// </summary>
        public void SetGrounded(bool grounded)
        {
            if(_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, grounded);
            }
        }

        /// <summary>
        /// 从移动状态到跳跃时的动画表现
        /// </summary>
        public void SetJump(bool jump)
        {
            if(_hasAnimator)
            {
                _animator.SetBool(_animIDJump, jump);
            }
        }

        /// <summary>
        /// 自由落体时的动画表现
        /// </summary>
        public void SetFreeFall(bool fall)
        {
            if(_hasAnimator)
            {
                _animator.SetBool(_animIDFreeFall, fall);
            }
        }
    }
}

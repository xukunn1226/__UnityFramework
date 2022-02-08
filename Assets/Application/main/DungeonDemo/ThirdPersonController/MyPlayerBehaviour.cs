using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public class MyPlayerBehaviour : MonoBehaviour
    {
        private Animator _animator;
        private CharacterController _controller;
        private bool _hasAnimator;

        [Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;

        // cinemachine
		private float _cinemachineTargetYaw;
		private float _cinemachineTargetPitch;

        // animation IDs
		private int _animIDSpeed;
		private int _animIDGrounded;
		private int _animIDJump;
		private int _animIDFreeFall;
		private int _animIDMotionSpeed;

        // Start is called before the first frame update
        void Awake()
        {
            _hasAnimator = TryGetComponent(out _animator);
			_controller = GetComponent<CharacterController>();

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

        private void LateUpdate()
		{
			CameraRotation();
		}

        private void CameraRotation()
		{
			// // if there is an input and camera position is not fixed
			// if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
			// {
			// 	_cinemachineTargetYaw += _input.look.x * Time.deltaTime;
			// 	_cinemachineTargetPitch += _input.look.y * Time.deltaTime;
			// }

			// // clamp our rotations so our values are limited 360 degrees
			// _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
			// _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			// // Cinemachine will follow this target
			// CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
		}

        public void SetMotion(Vector3 motion)
        {
            if(_controller)
            {
                _controller.Move(motion);
            }
        }

        public Vector3 velocity { get { return _controller.velocity; } }

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

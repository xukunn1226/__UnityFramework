using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;

namespace StarterAssets
{
    public class MyPlayerLogic
    {
        static public MyPlayerLogic Create(int id)
        {
            const string assetPath = "assets/starterassets/thirdpersoncontroller/prefabs/playerarmatureex.prefab";
            GameObject go = AssetManager.InstantiatePrefab(assetPath);
            MyPlayerLogic player = new MyPlayerLogic();
			player.id = id;
            player.Init(go);
			return player;
        }

		static public void Destroy(MyPlayerLogic player)
		{
			player?.Uninit();
		}

		public int id;

        [Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 2.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 5.335f;
		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 0.3f)]
		public float RotationSmoothTime = 0.12f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.50f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.28f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;


		// player
		private float _speed;
		private float _animationBlend;
		private float _targetRotation = 0.0f;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

        // input
        private Vector2 m_Move;
        private Vector2 m_Look;
        private bool m_Jump;
        private bool m_Sprint;



        private GameObject m_Player;
        private MyPlayerBehaviour m_PlayerBehaviour;
        private StarterAssetsInputs m_Input;
		public Transform playerCameraRoot => m_PlayerCameraRoot;
		private Transform m_PlayerCameraRoot;

        public void Init(GameObject go)
        {
            m_Player = go;
            m_PlayerBehaviour = m_Player.GetComponent<MyPlayerBehaviour>();
			m_Input = GameObject.FindObjectOfType<StarterAssetsInputs>();

			m_PlayerCameraRoot = m_Player.transform.Find("PlayerCameraRoot");

            // reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;

            GroundLayers = LayerMask.GetMask(new string[] {"Default", "Base"});

            m_Input.onMove += OnMove;
            m_Input.onLook += OnLook;
            m_Input.onJump += OnJump;
            m_Input.onSprint += OnSprint;
        }

        public void Uninit()
        {
            Object.Destroy(m_Player);
            m_Input.onMove -= OnMove;
            m_Input.onLook -= OnLook;
            m_Input.onJump -= OnJump;
            m_Input.onSprint -= OnSprint;
        }

        public void Update(float deltaTime)
        {
            JumpAndGravity();
			GroundedCheck();
			Move();
        }

        private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(m_Player.transform.position.x, m_Player.transform.position.y - GroundedOffset, m_Player.transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

			// update animator if using character
            m_PlayerBehaviour.SetGrounded(Grounded);
		}

        private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = m_Sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (m_Move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(m_PlayerBehaviour.velocity.x, 0.0f, m_PlayerBehaviour.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = m_Input.analogMovement ? m_Input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}
			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

			// normalise input direction
			Vector3 inputDirection = new Vector3(m_Input.move.x, 0.0f, m_Input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (m_Input.move != Vector2.zero)
			{
				_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
				float rotation = Mathf.SmoothDampAngle(m_Player.transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

				// rotate to face input direction relative to camera position
				m_Player.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}


			Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

			// move the player
            m_PlayerBehaviour.SetMotion(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			// update animator if using character
            m_PlayerBehaviour.SetSpeed(_animationBlend);
            m_PlayerBehaviour.SetMotionSpeed(inputMagnitude);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// update animator if using character
                m_PlayerBehaviour.SetJump(false);
                m_PlayerBehaviour.SetFreeFall(false);

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (m_Jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

					// update animator if using character
                    m_PlayerBehaviour.SetJump(true);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}
				else
				{
					// update animator if using character
                    m_PlayerBehaviour.SetFreeFall(true);
				}

				// if we are not grounded, do not jump
                m_Jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

        private void OnMove(Vector2 move)
        {
            m_Move = move;
        }

        private void OnLook(Vector2 look)
        {
            m_Look = look;
        }

        private void OnJump(bool jump)
        {
            m_Jump = jump;
        }

        private void OnSprint(bool sprint)
        {
            m_Sprint = sprint;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Application.Runtime;

namespace Application.Logic
{
    public class MyPlayerLogic
    {
        static public MyPlayerLogic Create(int id)
        {
            const string assetPath = "assets/res/dungeondemo/thirdpersoncontroller/prefabs/playerarmatureex.prefab";
			//GameObject go = AssetManager.InstantiatePrefab(assetPath);
			GameObject go = AssetManagerEx.LoadPrefab(assetPath).gameObject;
            MyPlayerLogic player = new MyPlayerLogic();
			player.id = id;
            player.Init(go);
			return player;
        }

		static public void Destroy(MyPlayerLogic player)
		{
			player?.Uninit();
		}

		public int 			id;

		public float 		MoveSpeed = 2.0f;				// walk时的速度
		public float 		SprintSpeed = 5.335f;			// sprint时的速度
		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 0.3f)]
		public float 		RotationSmoothTime = 0.12f;		// 转向速度
		public float 		SpeedChangeRate = 10.0f;		// 加速度
		public float 		JumpHeight = 1.2f;				// 最大跳跃高度		
		public float 		Gravity = -15.0f;				// 重力加速度
		public float 		JumpTimeout = 0.50f;			// 着地后可以再次跳跃的间隔时间		
		public float 		FallTimeout = 0.15f;			// 浮空状态多长时间判定为free fall
		public bool 		Grounded = true;				// 着地状态，自行计算，!= CharacterController.isGrounded
		public float 		GroundedOffset = -0.14f;		// 
		public float 		GroundedRadius = 0.28f;			// 判断是否落地的sphere radius，大小匹配CharacterController.radius
		public LayerMask 	GroundLayers;


		// player
		private float 		m_Speed;
		private float 		m_AnimationBlend;
		private float 		m_TargetRotation = 0.0f;
		private float 		m_RotationVelocity;
		private float 		m_VerticalVelocity;
		private float 		m_TerminalVelocity = 53.0f;

		// timeout deltatime
		private float 		m_JumpTimeoutDelta;
		private float 		m_FallTimeoutDelta;

        // input
        private Vector2 	m_Move;
        private Vector2 	m_Look;
        private bool 		m_Jump;
        private bool 		m_Sprint;



        private GameObject 			m_Player;
        private MyPlayerBehaviour 	m_PlayerBehaviour;
        private StarterAssetsInputs m_Input;
		public Transform 			playerCameraRoot => m_PlayerCameraRoot;
		private Transform 			m_PlayerCameraRoot;

        public void Init(GameObject go)
        {
            m_Player = go;
            m_PlayerBehaviour = m_Player.GetComponent<MyPlayerBehaviour>();
			m_Input = GameObject.FindObjectOfType<StarterAssetsInputs>();

			m_PlayerCameraRoot = m_Player.transform.Find("PlayerCameraRoot");

            // reset our timeouts on start
			m_JumpTimeoutDelta = JumpTimeout;
			m_FallTimeoutDelta = FallTimeout;

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
			Vector3 spherePosition = new Vector3(m_Player.transform.position.x, m_Player.transform.position.y - GroundedOffset, m_Player.transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

			// trigger player behaviour
            m_PlayerBehaviour.SetGrounded(Grounded);
		}

		public void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
			
			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(m_Player.transform.position.x, m_Player.transform.position.y - GroundedOffset, m_Player.transform.position.z), GroundedRadius);
		}

        private void Move()
		{
			// 水平速度值
			float currentHorizontalSpeed = new Vector3(m_PlayerBehaviour.velocity.x, 0.0f, m_PlayerBehaviour.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = m_Input.analogMovement ? m_Input.move.magnitude : 1f;
			// Debug.Log($"{inputMagnitude}	sprint: {m_Sprint}");

			float targetSpeed = (m_Sprint ? SprintSpeed : MoveSpeed) * inputMagnitude;
			if (m_Move == Vector2.zero) targetSpeed = 0.0f;

			// 与目标速度的差异值达到speedOffset执行加速、减速模拟
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// v = v0 + at
				m_Speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);

				m_Speed = Mathf.Round(m_Speed * 1000f) / 1000f;
			}
			else
			{
				m_Speed = targetSpeed;
			}
			m_AnimationBlend = Mathf.Lerp(m_AnimationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
	
			// rotation
			if (m_Input.move != Vector2.zero)
			{
				// normalise input direction
				Vector3 inputDirection = new Vector3(m_Input.move.x, 0.0f, m_Input.move.y).normalized;

				m_TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
				float rotation = Mathf.SmoothDampAngle(m_Player.transform.eulerAngles.y, m_TargetRotation, ref m_RotationVelocity, RotationSmoothTime);

				// rotate to face input direction relative to camera position
				m_Player.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}

			Vector3 targetDirection = Quaternion.Euler(0.0f, m_TargetRotation, 0.0f) * Vector3.forward;

			// move the player
            m_PlayerBehaviour.SetMotion(targetDirection.normalized * (m_Speed * Time.deltaTime) + new Vector3(0.0f, m_VerticalVelocity, 0.0f) * Time.deltaTime);

			// update animator if using character
            m_PlayerBehaviour.SetSpeed(m_AnimationBlend);			// 控制动画（idle, walk, sprint）的融合程度
            m_PlayerBehaviour.SetMotionSpeed(1);		// 控制动画的播放速度
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// 落地即重置浮空超时时间
				m_FallTimeoutDelta = FallTimeout;

				// trigger player behaviour
                m_PlayerBehaviour.SetJump(false);
                m_PlayerBehaviour.SetFreeFall(false);

				// stop our velocity dropping infinitely when grounded
				if (m_VerticalVelocity < 0.0f)
				{
					m_VerticalVelocity = -2f;
				}

				// Jump
				if (m_Jump && m_JumpTimeoutDelta <= 0.0f)
				{
					// h = 0.5 * g * t * t
					// v = g * t
					// v = sqrt(2 * h * g)
					m_VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);		// 要达到最大跳跃高度需要的速度

					// update animator if using character
                    m_PlayerBehaviour.SetJump(true);
				}

				// jump timeout
				if (m_JumpTimeoutDelta >= 0.0f)
				{
					m_JumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// 浮空状态即重置跳跃超时时间
				m_JumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (m_FallTimeoutDelta >= 0.0f)
				{
					m_FallTimeoutDelta -= Time.deltaTime;
				}
				else
				{
					// 只有当idle，walk，sprint切换至InAir状态时才根据fallTimeout判定，若jumpStart触发了将自动切换至InAir
                    m_PlayerBehaviour.SetFreeFall(true);
				}

				// 浮空状态不可跳跃
                m_Jump = false;
			}

			// v = g * t
			if (m_VerticalVelocity < m_TerminalVelocity)
			{
				m_VerticalVelocity += Gravity * Time.deltaTime;
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
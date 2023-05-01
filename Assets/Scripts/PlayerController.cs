using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	const float EPSILON = 0.0001f;

	[Header("References")]
	[SerializeField] Rigidbody2D m_rigidbody;
	[SerializeField] Health m_health;
	[SerializeField] CapsuleCollider2D m_collider;
	[SerializeField] Animator m_animator;
	[SerializeField] Animator m_rightDashAnimatior;
	[SerializeField] Animator m_leftDashAnimatior;
	[SerializeField] Animator m_dashReadyAnimator;
	[SerializeField] SpriteRenderer m_playerSprite;

	[Header("Acceleration Settings")]
	[SerializeField] float m_controlAccelerationWhenFalling;
	[SerializeField] float m_controlAccelerationWhenGrounded;

	[Header("Deceleration Settings")]
	[SerializeField] float m_controlDecelerationWhenFalling;
	[SerializeField] float m_controlDecelerationWhenGrounded;

	[Header("Run Speed Settings")]
	[SerializeField] float m_maxMoveSpeedInAir;
	[SerializeField] float m_maxMoveSpeedOnGround;

	[Header("Jump Settings")]
	[SerializeField] float m_jumpVelocity = 10.0f;

	[Header("Camera Settings")]
	[SerializeField] AnimationCurve m_cameraShakeIntensityByTimeInAir;

	[Header("Dash Settings")]
	[SerializeField] bool m_smoothDash = true;
	[SerializeField] float m_dashTimeLength = 0.2f;
	[SerializeField] float m_dashDistance = 5.0f;
	[SerializeField] float m_dashCooldown = 1.0f;

	[Header("Physics Settings")]
	[SerializeField] float m_maxFallSpeed;
	[SerializeField] float m_gravityScale = 3.0f;
	[SerializeField] float m_boxcastWidth = 0.05f;

	[Header("Ground Settings")]
	[SerializeField] float m_groundCheckDistance;
	[SerializeField] Vector2 m_topBottomCheckSize;
	[SerializeField] Vector2 m_leftRightCheckSize;
	[SerializeField] private LayerMask m_groundLayers;
	[SerializeField] private LayerMask m_enemiesLayers;

	//Data
	float m_currHorizontalSpeed;

	RaycastHit2D[] m_currentDownCollisions = new RaycastHit2D[3];
	[SerializeField, ReadOnly] int m_currentDownCollisionCount;

	RaycastHit2D[] m_currentRightCollisions = new RaycastHit2D[3];
	[SerializeField, ReadOnly] int m_currentRightCollisionCount;

	RaycastHit2D[] m_currentLeftCollisions = new RaycastHit2D[3];
	[SerializeField, ReadOnly] int m_currentLeftCollisionCount;

	RaycastHit2D[] m_currentTopCollisions = new RaycastHit2D[3];
	[SerializeField, ReadOnly] int m_currentTopCollisionCount;

	RaycastHit2D[] m_dashCollisionsAlloc = new RaycastHit2D[3];
	[SerializeField, ReadOnly] int m_dashCollisionsAllocCount;

	[SerializeField, Fillbar("m_dashCooldown")] float m_timeUntilNextDash = 0.5f;

	[SerializeField] float m_timeOfLastGroundedFrame = float.MaxValue;

	Vector2 m_velocity = Vector2.zero;

	bool m_isDashing = false;

	Coroutine m_currentJumpRoutine = null;
	Coroutine m_currentDashRoutine = null;

	bool m_dashTimeReady = false;
	float m_facingDirection = 1.0f;
	float m_horizontalInput = 0.0f;
	bool m_dashInput = false;
	float m_jumpInput = 0.0f;
	bool m_isRunning = false;
	bool m_isAirborne = false;
	bool m_inputEnabled = true;
	private Coroutine m_damageCR;
	Vector2 m_outsideForcesToApplyNextUpdate = Vector2.zero;
	bool m_dashAnimReady = true;
	private bool m_readyAnimationActive = true;

	public bool GetIsMoving
	{
		get
		{
			return Mathf.Abs(m_horizontalInput) > EPSILON;
		}
	}

	public bool GetIsJumping
	{
		get
		{
			return Mathf.Abs(m_jumpInput) > EPSILON;
		}
	}

	public bool GetIsGrounded
	{
		get
		{
			for (int i = 0; i < m_currentDownCollisionCount; i++)
			{
				if (m_currentDownCollisions[i] && (m_groundLayers & 1 << m_currentDownCollisions[i].collider.gameObject.layer) != 0)
					return true;
			}
			return false;
		}
	}

	public bool GetIsStandingOnEnemy
	{
		get
		{
			for (int i = 0; i < m_currentDownCollisionCount; i++)
			{
				if (m_currentDownCollisions[i] && (m_enemiesLayers & 1 << m_currentDownCollisions[i].collider.gameObject.layer) != 0)
					return true;
			}
			return false;
		}
	}

	private void Awake()
	{
		m_currHorizontalSpeed = 0.0f;
	}

	private void OnEnable()
	{
		Health.OnDead += OnDead;
		Health.OnTakeDamage += OnTakeDamage;
	}

	private void OnDisable()
	{
		Health.OnDead -= OnDead;
		Health.OnTakeDamage -= OnTakeDamage;
	}

	IEnumerator DoJumpRoutine()
	{
		EnableGravity(false);
		float startJumpTime = Time.time;
		yield return new WaitUntil(() => Time.time - startJumpTime >= 0.5f || m_currentTopCollisionCount > 0 || !GetIsJumping);
		EnableGravity(true);

		if (m_isAirborne)
		{
			m_animator.SetTrigger("tFall");
		}

		yield return null;
	}

	IEnumerator DoDashRoutine(float direction)
	{
		m_isDashing = true;

		m_timeOfLastGroundedFrame = Time.time;

		m_velocity = Vector2.zero;

		Vector3 targetPos = (Vector2)m_collider.bounds.center + new Vector2(direction * m_dashDistance, 0f);
		float targetDistance = m_dashDistance;

		float colliderHalfWidth = m_collider.bounds.size.x * 0.5f;

		m_dashCollisionsAllocCount = GetCollisionsInDirection(new Vector2(direction, 0.0f), m_dashDistance, m_dashCollisionsAlloc);

		if (m_dashCollisionsAllocCount > 0)
		{
			//get nearest collision
			for (int i = 0; i < m_dashCollisionsAllocCount; ++i)
			{
				Vector2 pos = m_dashCollisionsAlloc[0].point - new Vector2(System.Math.Sign(direction) * colliderHalfWidth, 0.0f);
				float distance = Mathf.Abs(pos.x - transform.position.x);
				if (distance < targetDistance)
				{
					targetPos = pos;
					targetDistance = distance;
				}
			}
		}

		if (targetDistance > EPSILON)
		{
			m_animator.SetBool("bDashing", true);

			if (targetDistance < m_dashDistance)
				Debug.Log("Distance: " + targetDistance);
			//Shorten dash time if the player is going to hit something and not do a complete dash
			float percentOfFullDash = targetDistance / m_dashDistance;
			float dashTime = m_dashTimeLength * percentOfFullDash;

			Vector3 startPos = transform.position;
			float startTime = Time.time;
			float timeSinceStart;
			do
			{
				timeSinceStart = Time.time - startTime;

				float percent = timeSinceStart / dashTime;

				//Smooth out acceleration and deceleration while still 
				if (m_smoothDash)
					percent = Mathf.SmoothStep(0.0f, 1.0f, percent);
				transform.position = Vector3.Lerp(startPos, targetPos, percent);

				yield return null;
			}
			while (timeSinceStart < dashTime);
			transform.position = targetPos;
		}
		m_animator.SetBool("bDashing", false);

		m_isDashing = false;
		m_dashAnimReady = false;
		m_readyAnimationActive = false;
	}

	public Health Health { get => m_health; }

	public int GetCollisionsInDirection(Vector2 direction, float distance, RaycastHit2D[] hits)
	{
		Vector2 castOrigin =
			(Vector2)transform.position +
			((m_collider.bounds.size * 0.5f) * direction) -
			direction * new Vector2(0.1f, 0.1f);

		Vector2 unsignedDir = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
		Vector2 boxCastSize =
			m_collider.bounds.size * new Vector2(unsignedDir.y, unsignedDir.x) * 0.8f +
			new Vector2(m_boxcastWidth, m_boxcastWidth) * unsignedDir;

		int hitCount = Physics2D.BoxCastNonAlloc(castOrigin, boxCastSize, 0f, direction, hits, distance);

		//Filter any hit who's normal is not opposite direction
		for (int i = hitCount - 1; i >= 0; i--)
		{
			if (hits[i].collider.isTrigger ||
				Vector2.Dot(hits[i].normal, direction) > -1.0f + EPSILON ||
				hits[i].collider.transform.root == transform.root)
			{
				hitCount--;
				hits[i] = hits[hitCount];
			}
		}

		return hitCount;
	}

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Vector2[] directions = {
		new Vector2(1.0f, 0.0f),
		new Vector2(-1.0f, 0.0f),
		new Vector2(0.0f, 1.0f),
		new Vector2(0.0f, -1.0f)};

		foreach (Vector2 direction in directions)
		{
			Vector2 castOrigin =
				(Vector2)transform.position +
				((m_collider.bounds.size * 0.5f) * direction) -
				direction * new Vector2(0.1f, 0.1f);

			Vector2 unsignedDir = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
			Vector2 boxCastSize =
				m_collider.bounds.size * new Vector2(unsignedDir.y, unsignedDir.x) * 0.8f +
				new Vector2(m_boxcastWidth, m_boxcastWidth) * unsignedDir;

			Color cachedCol = Gizmos.color;
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(castOrigin, boxCastSize);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(castOrigin + direction * m_groundCheckDistance, boxCastSize);
			Gizmos.color = cachedCol;
		}
	}
#endif

	void Update()
	{
		UpdateCooldowns();
		UpdateInput();
	}

	void UpdateCooldowns()
	{
		m_timeUntilNextDash -= Time.deltaTime;
		if (m_timeUntilNextDash <= 0 && !m_readyAnimationActive)
		{
			m_dashReadyAnimator.SetTrigger("tDashReady");
			m_readyAnimationActive = true;
		}
	}

	void UpdateInput()
	{
		if (m_inputEnabled)
		{
			m_horizontalInput = Input.GetAxis("Horizontal");

			m_dashTimeReady = m_timeUntilNextDash <= 0.0f;
			m_dashInput = m_dashTimeReady && m_dashAnimReady ? Input.GetButton("Dash") : false;
			if (Mathf.Abs(m_horizontalInput) > EPSILON)
			{
				m_facingDirection = m_horizontalInput < 0.0f ? -1.0f : 1.0f;
				m_playerSprite.flipX = m_facingDirection > 0 ? false : true;
			}

			if (GetIsGrounded || !GetIsJumping)
			{
				if (m_horizontalInput != 0)
				{
					m_isRunning = true;
					m_animator.SetTrigger("tRunStart");
				}
				else if (m_horizontalInput == 0)
				{
					m_isRunning = false;
					m_animator.SetTrigger("tIdle");
				}

				m_animator.SetBool("bRunning", m_isRunning);
			}

			m_jumpInput = Input.GetAxis("Jump");
			m_animator.SetBool("bJumping", !GetIsGrounded);
		}
	}

	void FixedUpdate()
	{
		m_currentDownCollisionCount = GetCollisionsInDirection(Vector2.down, m_groundCheckDistance, m_currentDownCollisions);
		m_currentTopCollisionCount = GetCollisionsInDirection(Vector2.up, m_groundCheckDistance, m_currentTopCollisions);
		m_currentLeftCollisionCount = GetCollisionsInDirection(Vector2.left, m_groundCheckDistance, m_currentLeftCollisions);
		m_currentRightCollisionCount = GetCollisionsInDirection(Vector2.right, m_groundCheckDistance, m_currentRightCollisions);

		bool grounded = GetIsGrounded;
		bool standingOnEnemy = GetIsStandingOnEnemy;
		bool isMoving = GetIsMoving;
		bool tryDash = m_dashInput;

		float accelToUse;
		float decelToUse;
		float maxSpeedToUse;

		if (grounded)
		{
			m_timeOfLastGroundedFrame = Time.time;
		}
		else
		{
			float timeInAir = Time.time - m_timeOfLastGroundedFrame;

			GameManager.Instance.CameraController.AddTrauma(.125f, m_cameraShakeIntensityByTimeInAir.Evaluate(timeInAir));
		}

		if (!m_isDashing)
		{
			if (tryDash)
			{
				if (m_currentDashRoutine != null)
					StopCoroutine(m_currentDashRoutine);

				m_currentDashRoutine = StartCoroutine(DoDashRoutine(m_facingDirection));

				AudioManager.Instance.PlaySFX(GameManager.Instance.DashSFX);
				GameManager.Instance.CameraController.AddTrauma(.125f, .15f);
				if (m_facingDirection < 0)
					m_rightDashAnimatior.SetTrigger("tDash");
				else
					m_leftDashAnimatior.SetTrigger("tDash");

				maxSpeedToUse = float.MaxValue;

				m_timeUntilNextDash = m_dashCooldown;
			}
			else
			{
				if (grounded)
				{
					accelToUse = m_controlAccelerationWhenGrounded;
					decelToUse = m_controlDecelerationWhenGrounded;
					maxSpeedToUse = m_maxMoveSpeedOnGround;

					m_isAirborne = false;
					m_animator.SetBool("bFalling", m_isAirborne);
				}
				else
				{
					accelToUse = m_controlAccelerationWhenFalling;
					decelToUse = m_controlDecelerationWhenFalling;
					maxSpeedToUse = m_maxMoveSpeedInAir;
				}

				if (isMoving)
				{
					m_velocity.x = Mathf.MoveTowards(m_velocity.x, m_maxMoveSpeedInAir * m_horizontalInput, accelToUse);
				}
				else // not moving. Decelerate speed to zero
				{
					m_velocity.x = Mathf.MoveTowards(m_velocity.x, 0.0f, decelToUse);
				}

				float jumpForce = 0.0f;

				if (grounded)
				{
					m_velocity.y = 0.0f;

					if (GetIsJumping)
					{
						m_velocity.y += m_jumpVelocity;

						if (!m_isAirborne)
						{
							m_animator.SetTrigger("tJump");
							m_isAirborne = true;
							m_animator.SetBool("bFalling", m_isAirborne);
						}

						AudioManager.Instance.PlaySFX(GameManager.Instance.JumpSFX);

						if (m_currentJumpRoutine != null)
							StopCoroutine(m_currentJumpRoutine);
						StartCoroutine(DoJumpRoutine());
					}
				}
				else if (standingOnEnemy)
				{
					//force player to jump
					jumpForce = -m_rigidbody.velocity.y + m_jumpVelocity;
					GameManager.Instance.CameraController.AddTrauma(0.2f, 0.2f);
					}
				else // else if falling
				{
					m_velocity.y = m_rigidbody.velocity.y;
				}
			}
		}
		else
		{
			maxSpeedToUse = float.MaxValue;
		}
		m_animator.SetBool("bFalling", !grounded);

		m_velocity += m_outsideForcesToApplyNextUpdate;
		m_outsideForcesToApplyNextUpdate = Vector2.zero;

		// Clamp the velocity to the maximum speed
		m_velocity.x = Mathf.Clamp(m_velocity.x, -maxSpeedToUse, maxSpeedToUse);
		m_velocity.y = Mathf.Clamp(m_velocity.y, -m_maxFallSpeed, float.MaxValue);//dont clamp up velocity to allow for faster jumps

		// Stop collision
		if (m_currentRightCollisionCount > 0 && m_velocity.x > 0.0f ||
			m_currentLeftCollisionCount > 0 && m_velocity.x < 0.0f)
			m_velocity.x = 0.0f;
		if (m_currentTopCollisionCount > 0 && m_velocity.y > 0.0f ||
			m_currentDownCollisionCount > 0 && m_velocity.y < 0.0f)
			m_velocity.y = 0.0f;

		// If velocity is basically zero, just set it to zero
		if (Mathf.Abs(m_velocity.x) < EPSILON)
			m_velocity.x = 0.0f;
		if (Mathf.Abs(m_velocity.y) < EPSILON)
			m_velocity.y = 0.0f;

		m_rigidbody.velocity = m_velocity;
	}

	void OnPlatformLand(Platform platform)
	{
		//if we're standing on touched platform and didn't just brush against it
		for (int i = 0; i < m_currentDownCollisionCount; i++)
		{
			if (m_currentDownCollisions[i].collider.gameObject == platform.gameObject)
			{
				platform.OnPlayerHit();
				m_isAirborne = false;
				m_animator.SetBool("bFalling", m_isAirborne);
				AudioManager.Instance.PlaySFX(GameManager.Instance.LandSFX);

				if(!platform.Breakable)
					GameManager.Instance.CameraController.AddTrauma(0.1f, 0.1f);
			}
		}
	}

	void OnDead()
	{
		var level = GameManager.Instance.CurrentLevel;
		if (level != null && !level.IsDescending)
		{
			StartCoroutine(LoseCR());
		}
		else
		{
			EnableInput(false);
			StartCoroutine(ResetCR());
		}
	}

	private IEnumerator ResetCR()
	{
		var phase = GameManager.Instance.CurrentPhase as PlayPhase;
		if (phase != null)
		{
			phase.ReloadLevel();
		}

		m_health.SetHealthTo(m_health.MaxHealth, false);

		transform.position = GameManager.Instance.CurrentLevel.PlayerStartLocation;
		yield return new WaitForSeconds(1f);
		EnableInput(true);
	}

	private IEnumerator LoseCR()
	{
		yield return new WaitForSeconds(.25f);
		var c = m_playerSprite.color;
		c.a = 0;
		m_playerSprite.color = c;
		yield return new WaitForSeconds(1f);
		LoadingUI.ShowLoadingScreen();
		GameManager.Instance.SetNextPhase(new PostGamePhase(false));
	}

	private void OnTakeDamage()
	{
		if (m_damageCR != null)
		{
			StopCoroutine(m_damageCR);
			m_damageCR = null;
			m_playerSprite.color = Color.white;
		}
		m_damageCR = StartCoroutine(TakeDamageCR());
	}

	public void OnDashReady()
	{
		m_dashAnimReady = true;
		StartCoroutine(DashReadyCR());
	}

	private void OnKnockback(Vector2 knockback)
	{
		m_outsideForcesToApplyNextUpdate += knockback;
	}

	private IEnumerator TakeDamageCR()
	{
		var c = m_playerSprite.color;
		m_playerSprite.color = Color.red;
		const float bloodAnimTime = 0.15f;
		yield return new WaitForSeconds(bloodAnimTime);
		m_playerSprite.color = c;
	}

	private IEnumerator DashReadyCR()
	{
		var c = Color.white;
		m_playerSprite.color = Color.cyan;
		const float dashReadyAnimTime = 0.1f;
		yield return new WaitForSeconds(dashReadyAnimTime);
		m_playerSprite.color = c;
	}

	public void EnableGravity(bool a_enable)
	{
		if (a_enable)
		{
			m_rigidbody.gravityScale = m_gravityScale;
		}
		else
		{
			m_rigidbody.gravityScale = 0.0f;
		}
	}

	public void EnableInput(bool a_enable, bool a_pausing = false)
	{
		if (a_enable)
		{
			m_inputEnabled = true;
			m_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
		}
		else
		{
			m_inputEnabled = false;

			if (!a_pausing)
			{
				m_rigidbody.velocity = Vector3.zero;
				m_rigidbody.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
				m_animator.SetTrigger("tIdle");
				m_animator.SetBool("bJumping", false);
				m_animator.SetBool("bRunning", false);
				m_animator.SetBool("bFalling", false);
			}
		}
	}
}

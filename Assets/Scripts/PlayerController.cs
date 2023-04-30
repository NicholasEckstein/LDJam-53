using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Yarn.Unity.Editor;

public class PlayerController : MonoBehaviour
{
	const float EPSILON = 0.0001f;

	[Header("References")]
	[SerializeField] Rigidbody2D m_rigidbody;
	[SerializeField] Health m_health;
	[SerializeField] BoxCollider2D m_collider;
	[SerializeField] Animator m_animator;
	[SerializeField] SpriteRenderer m_playerSprite;

	[Header("Control Settings")]
	[SerializeField] float m_controlAccelerationWhenFalling;
	[SerializeField] float m_controlAccelerationWhenGrounded;
	[Space]
	[SerializeField] float m_controlDecelerationWhenFalling;
	[SerializeField] float m_controlDecelerationWhenGrounded;
	[Space]
	[SerializeField] float m_maxMoveSpeedInAir;
	[SerializeField] float m_maxMoveSpeedOnGround;
	[Space]
	[SerializeField] float m_jumpVelocity = 10.0f;
	[Space]
	[SerializeField] float m_dashVelocity = 10.0f;

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

	[SerializeField] RaycastHit2D[] m_currentDownCollisions = new RaycastHit2D[3];
	[SerializeField] int m_currentDownCollisionCount;

	[SerializeField] RaycastHit2D[] m_currentRightCollisions = new RaycastHit2D[3];
	[SerializeField] int m_currentRightCollisionCount;

	[SerializeField] RaycastHit2D[] m_currentLeftCollisions = new RaycastHit2D[3];
	[SerializeField] int m_currentLeftCollisionCount;

	[SerializeField] RaycastHit2D[] m_currentTopCollisions = new RaycastHit2D[3];
	[SerializeField] int m_currentTopCollisionCount;

	Vector2 m_velocity = Vector2.zero;

	bool m_isDashing = false;

	Coroutine m_currentJumpRoutine = null;
	Coroutine m_currentDashRoutine = null;

	float horizontalInput = 0.0f;
	float dashInput = 0.0f;
	bool jump = false;
	bool m_isRunning = false;
	bool m_isAirborne = false;

	private void Awake()
	{
		m_currHorizontalSpeed = 0.0f;
	}

	private void OnEnable()
	{
		Health.OnDead += OnDead;
	}

	private void OnDisable()
	{
		Health.OnDead -= OnDead;
	}

	IEnumerator DoJumpRoutine()
	{
		EnableGravity(false);
		yield return new WaitForSeconds(0.5f);
		EnableGravity(true);

		if (m_isAirborne)
		{
			m_animator.SetTrigger("tFall");
		}

		yield return null;
	}

	IEnumerator DoDashRoutine()
	{
		m_isDashing = true;
		yield return new WaitForSeconds(0.5f);
		m_isDashing = false;
	}

	public bool GetIsMoving
	{
		get
		{
			return Mathf.Abs(horizontalInput) > EPSILON;
		}
	}

	public bool GetIsTryingToDash
	{
		get
		{
			return Mathf.Abs(dashInput) > EPSILON;
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

	void Update()
	{
		horizontalInput = Input.GetAxis("Horizontal");
		dashInput = Input.GetAxis("Dash");

		if (GetIsGrounded || !jump)
		{
			if (horizontalInput != 0)
			{
				m_isRunning = true;
				m_animator.SetTrigger("tRunStart");
			}
			else if (horizontalInput == 0)
			{
				m_isRunning = false;
				m_animator.SetTrigger("tIdle");
			}

			m_animator.SetBool("bRunning", m_isRunning);
		}

		if (m_playerSprite != null)
			m_playerSprite.flipX = horizontalInput < 0;

		jump = Input.GetButton("Jump");

		if(jump)
        {
			AudioManager.Instance.PlaySFX(GameManager.Instance.JumpSFX);
        }


		m_animator.SetBool("bJumping", !GetIsGrounded);
	}

	public int GetCollisionsInDirection(Vector2 direction, RaycastHit2D[] hits)
	{
		Vector2 castOrigin =
			(Vector2)transform.position +
			((m_collider.bounds.size * 0.5f) * direction) -
			direction * new Vector2(0.1f, 0.1f);

		Vector2 unsignedDir = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
		Vector2 boxCastSize =
			m_collider.bounds.size * new Vector2(unsignedDir.y, unsignedDir.x) * 0.8f +
			new Vector2(m_boxcastWidth, m_boxcastWidth) * unsignedDir;

		int hitCount = Physics2D.BoxCastNonAlloc(castOrigin, boxCastSize, 0f, direction, hits, m_groundCheckDistance);

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

	void FixedUpdate()
	{
		m_currentDownCollisionCount = GetCollisionsInDirection(Vector2.down, m_currentDownCollisions);
		m_currentTopCollisionCount = GetCollisionsInDirection(Vector2.up, m_currentTopCollisions);
		m_currentLeftCollisionCount = GetCollisionsInDirection(Vector2.left, m_currentLeftCollisions);
		m_currentRightCollisionCount = GetCollisionsInDirection(Vector2.right, m_currentRightCollisions);

		bool grounded = GetIsGrounded;
		bool standingOnEnemy = GetIsStandingOnEnemy;
		bool isMoving = GetIsMoving;
		bool tryDash = GetIsTryingToDash;

		float accelToUse;
		float decelToUse;
		float maxSpeedToUse;

		if (!m_isDashing)
		{
			if (tryDash)
			{
				if (m_currentDashRoutine != null)
					StopCoroutine(m_currentDashRoutine);

				m_currentDashRoutine = StartCoroutine(DoDashRoutine());

				m_velocity.x = m_dashVelocity * dashInput;
				AudioManager.Instance.PlaySFX(GameManager.Instance.DashSFX);
				maxSpeedToUse = float.MaxValue;
			}
			else
			{
				if (grounded)
				{
					accelToUse = m_controlAccelerationWhenGrounded;
					decelToUse = m_controlDecelerationWhenGrounded;
					maxSpeedToUse = m_maxMoveSpeedOnGround;
				}
				else
				{
					accelToUse = m_controlAccelerationWhenFalling;
					decelToUse = m_controlDecelerationWhenFalling;
					maxSpeedToUse = m_maxMoveSpeedInAir;
				}

				if (isMoving)
				{
					m_velocity.x = Mathf.MoveTowards(m_velocity.x, m_maxMoveSpeedInAir * horizontalInput, accelToUse);
				}
				else // not moving. Decelerate speed to zero
				{
					m_velocity.x = Mathf.MoveTowards(m_velocity.x, 0.0f, decelToUse);
				}

				float jumpForce = 0.0f;

				if (grounded)
				{
					m_velocity.y = 0.0f;

					if (jump)
					{
						m_velocity.y += m_jumpVelocity;

						if (!m_isAirborne)
						{
							m_animator.SetTrigger("tJump");
							m_isAirborne = true;
							m_animator.SetBool("bFalling", m_isAirborne);
						}

						if (m_currentJumpRoutine != null)
							StopCoroutine(m_currentJumpRoutine);
						StartCoroutine(DoJumpRoutine());
					}
				}
				else if (standingOnEnemy)
				{
					//force player to jump
					jumpForce = -m_rigidbody.velocity.y + m_jumpVelocity;
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
			}
		}
	}

	void OnDead()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
}

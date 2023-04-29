using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Compiler;

public class PlayerController : MonoBehaviour
{
	const float EPSILON = 0.0001f;

	enum PlayerState
	{
		FALLING, PLATFORMING
	}

	[Header("References")]
	[SerializeField] Rigidbody2D m_rigidbody;
	[SerializeField] Health m_health;

	[Header("Control Settings")]
	[SerializeField] float m_controlAccelerationWhenFalling;
	[SerializeField] float m_controlAccelerationWhenGrounded;
	[SerializeField] float m_maxSpeed;
	[SerializeField] float m_jumpForce;

	[Header("Physics Settings")]
	[SerializeField] float m_maxFallSpeed;

	[Header("Ground Settings")]
	[SerializeField] float m_groundCheckDistance;
	[SerializeField] Transform m_feetPos;
	[SerializeField] Vector2 m_feetSize;
	[SerializeField] private LayerMask m_groundLayers;
	[SerializeField] private LayerMask m_enemiesLayers;

	[Header("World Interaction Settings")]
	[SerializeField] float m_platformDestroyDelay;

	//Data
	float m_currHorizontalSpeed;

	[SerializeField] RaycastHit2D[] m_currentStandingOn = new RaycastHit2D[3];
	[SerializeField] int m_currentStandingOnCount;

	PlayerState m_playerState = PlayerState.FALLING;

	float horizontalInput = 0.0f;
	bool jump = false;

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Color cachedCol = Gizmos.color;
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(m_feetPos.transform.position, m_feetSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(m_feetPos.transform.position + Vector3.down * m_groundCheckDistance, m_feetSize);
		Gizmos.color = cachedCol;
	}
#endif

	private void Awake()
	{
		m_currHorizontalSpeed = 0.0f;
	}

	public bool GetIsMoving
	{
		get
		{
			return Mathf.Abs(horizontalInput) > EPSILON;
		}
	}

	public bool GetIsGrounded
	{
		get
		{
			for (int i = 0; i < m_currentStandingOnCount; i++)
			{
				if (m_currentStandingOn[i] && (m_groundLayers & 1 << m_currentStandingOn[i].collider.gameObject.layer) != 0)
					return true;
			}
			return false;
		}
	}

	public bool GetIsStandingOnEnemy
	{
		get
		{
			for (int i = 0; i < m_currentStandingOnCount; i++)
			{
				if (m_currentStandingOn[i] && (m_enemiesLayers & 1 << m_currentStandingOn[i].collider.gameObject.layer) != 0)
					return true;
			}
			return false;
		}
	}

	void Update()
	{
		horizontalInput = Input.GetAxis("Horizontal");
		jump = Input.GetButton("Jump");
	}

	void FixedUpdate()
	{
		m_currentStandingOnCount = Physics2D.BoxCastNonAlloc(
			m_feetPos.position, m_feetSize, 0f, Vector2.down, m_currentStandingOn, m_groundCheckDistance, m_groundLayers);

		bool grounded = GetIsGrounded;
		bool standingOnEnemy = GetIsStandingOnEnemy;
		bool isMoving = GetIsMoving;

		float controlForceToUse;

		float m_accelToUse = grounded ? m_controlAccelerationWhenGrounded : m_controlAccelerationWhenFalling;

		if (isMoving)
		{
			controlForceToUse = horizontalInput * m_accelToUse;
		}
		else // not moving. Decelerate speed to zero
		{
			controlForceToUse = -System.Math.Sign(m_rigidbody.velocity.x) * m_accelToUse;
		}

		float jumpForce = 0.0f;

		if (grounded)
		{
			jumpForce = -m_rigidbody.velocity.y;

			m_rigidbody.gravityScale = 0.0f;

			if (jump)
			{
				jumpForce += m_jumpForce;
			}
		}
		else if (standingOnEnemy)
		{
			//force player to jump
			jumpForce = -m_rigidbody.velocity.y + m_jumpForce;
		}
		else // else if falling
		{
			m_rigidbody.gravityScale = 1.0f;
		}

		m_rigidbody.AddForce(new Vector2(controlForceToUse, jumpForce));

		// Clamp the velocity to the maximum speed
		float clampedVelocityX = Mathf.Clamp(m_rigidbody.velocity.x, -m_maxSpeed, m_maxSpeed);
		float fallVelocityX = Mathf.Clamp(m_rigidbody.velocity.y, -m_maxFallSpeed, m_maxFallSpeed);

		// If velocity is basically zero, just set it to zero
		if (Mathf.Abs(clampedVelocityX) < EPSILON)
			clampedVelocityX = 0.0f;
		if (Mathf.Abs(fallVelocityX) < EPSILON)
			fallVelocityX = 0.0f;

		m_rigidbody.velocity = new Vector2(clampedVelocityX, fallVelocityX);
	}

	void OnPlatformLand(Platform platform)
	{
		//if we are falling (not platforming)
		if (m_playerState == PlayerState.FALLING)
		{
			//if we're standing on touched platform and didn't just brush against it
			for (int i = 0; i < m_currentStandingOnCount; i++)
			{
				//destroy the platform and take damage
				if (m_currentStandingOn[i] && (m_groundLayers & 1 << m_currentStandingOn[i].collider.gameObject.layer) != 0)
				{
					platform.DestroyPlatform(m_platformDestroyDelay);

					m_health.ChangeHealthBy(-1.0f, true);
				}
			}
		}
	}

	void OnDead(Health health)
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void EnableGravity(bool a_enable)
    {
		if(a_enable)
        {
			m_rigidbody.gravityScale = 1.0f;
		}
		else
        {
			m_rigidbody.gravityScale = 0.0f;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Compiler;

public class PlayerController : MonoBehaviour
{
	const float EPSILON = 0.0001f;

	[Header("References")]
	[SerializeField] Rigidbody2D m_rigidbody;

	[Header("Control Settings")]
	[SerializeField] float m_controlAccelerationWhenFalling;
	[SerializeField] float m_controlAccelerationWhenGrounded;
	[SerializeField] float m_maxSpeed;
	[SerializeField] float m_jumpForce;

	[Header("Physics Settings")]
	[SerializeField] float m_maxFallSpeed;

	[Header("Ground Settings")]
	[SerializeField] float m_groundedSensitivity;
	[SerializeField] Transform m_feetPos;
	[SerializeField] private LayerMask m_groundLayers;
	[SerializeField] private LayerMask m_enemiesLayers;

	[Header("Player Settings")]
	[SerializeField] float m_maxHealth;

	//Data
	float m_currentHealth;
	float m_currHorizontalSpeed;

	float horizontalInput = 0.0f;
	bool jump = false;

	private void Awake()
	{
		m_currentHealth = m_maxHealth;
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
			RaycastHit2D hit = Physics2D.Raycast(m_feetPos.position, Vector2.down, m_groundedSensitivity, m_groundLayers);
			return hit.collider != null;
		}
	}

	public bool GetIsStandingOnEnemy
	{
		get
		{
			RaycastHit2D hit = Physics2D.Raycast(m_feetPos.position, Vector2.down, m_groundedSensitivity, m_enemiesLayers);
			return hit.collider != null;
		}
	}

	void Update()
	{
		horizontalInput = Input.GetAxis("Horizontal");
		jump = Input.GetButton("Jump");
	}

	void FixedUpdate()
	{
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

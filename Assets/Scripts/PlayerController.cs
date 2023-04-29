using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
class RegionEnteredEvent : UnityEvent<CameraConstraintData> { }

public class PlayerController : MonoBehaviour
{
	const float EPSILON = 0.0001f;

	[Header("References")]
	[SerializeField] Rigidbody2D m_rigidbody;
	[SerializeField] Health m_health;
	[SerializeField] BoxCollider2D m_collider;

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
	[SerializeField] float m_jumpForce;

	[Header("Physics Settings")]
	[SerializeField] float m_maxFallSpeed;

	[Header("Ground Settings")]
	[SerializeField] float m_groundCheckDistance;
	[SerializeField] Vector2 m_topBottomCheckSize;
	[SerializeField] Vector2 m_leftRightCheckSize;
	[SerializeField] private LayerMask m_groundLayers;
	[SerializeField] private LayerMask m_enemiesLayers;

	[Header("Events")]
	[SerializeField] RegionEnteredEvent m_onRegionEntered;

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

	float horizontalInput = 0.0f;
	bool jump = false;

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
		jump = Input.GetButton("Jump");
	}

	public int GetCollisionsInDirection(Vector2 direction, RaycastHit2D[] hits)
	{
		Vector2 castOrigin =
			(Vector2)transform.position +
			((m_collider.bounds.size * 0.5f) * direction) -
			direction * new Vector2(0.1f, 0.1f);

		Vector2 unsignedDir = new Vector2(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
		Vector2 boxCastSize =
			m_collider.bounds.size * new Vector2(unsignedDir.y, unsignedDir.x) +
			new Vector2(0.1f, 0.1f) * unsignedDir;

		int hitCount = Physics2D.BoxCastNonAlloc(castOrigin, boxCastSize, 0f, direction, hits, m_groundCheckDistance);

		//Filter any hit who's normal is not opposite direction
		for (int i = hitCount - 1; i >= 0; i--)
		{
			if (hits[i].collider.isTrigger ||
				Vector2.Dot(hits[i].normal, direction) > -1.0f + EPSILON ||
				hits[i].collider.gameObject == gameObject)
			{
				hitCount--;
				hits[i] = hits[hitCount];
			}
		}

		return hitCount;
	}

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
				m_collider.bounds.size * new Vector2(unsignedDir.y, unsignedDir.x) +
				new Vector2(0.1f, 0.1f) * unsignedDir;

			Color cachedCol = Gizmos.color;
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(castOrigin, boxCastSize);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(castOrigin + direction * m_groundCheckDistance, boxCastSize);
			Gizmos.color = cachedCol;
		}
	}

	void FixedUpdate()
	{
		m_currentDownCollisionCount = GetCollisionsInDirection(Vector2.down, m_currentDownCollisions);
		m_currentTopCollisionCount = GetCollisionsInDirection(Vector2.up, m_currentTopCollisions);
		m_currentLeftCollisionCount = GetCollisionsInDirection(Vector2.left, m_currentLeftCollisions);
		m_currentRightCollisionCount = GetCollisionsInDirection(Vector2.right, m_currentRightCollisions);

		bool grounded = GetIsGrounded;
		bool standingOnEnemy = GetIsStandingOnEnemy;
		bool isMoving = GetIsMoving;

		float accelToUse;
		float decelToUse;
		float maxSpeedToUse;
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
				m_velocity.y += m_jumpForce;
			}
		}
		else if (standingOnEnemy)
		{
			//force player to jump
			jumpForce = -m_rigidbody.velocity.y + m_jumpForce;
		}
		else // else if falling
		{
			m_velocity.y = m_rigidbody.velocity.y;
		}

		// Clamp the velocity to the maximum speed
		m_velocity.x = Mathf.Clamp(m_velocity.x, -maxSpeedToUse, maxSpeedToUse);
		m_velocity.y = Mathf.Clamp(m_velocity.y, -m_maxFallSpeed, m_maxFallSpeed);

		// Stop collision
		if (m_currentRightCollisionCount > 0 && m_velocity.x > 0.0f ||
			m_currentLeftCollisionCount > 0 && m_velocity.x < 0.0f)
			m_velocity.x = 0.0f;

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
				platform.OnPlayerHit();
		}
	}

	void OnDead()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	void OnEnteredRegion(CameraConstraintData cameraConstraintData)
	{
		m_onRegionEntered.Invoke(cameraConstraintData);
	}

	public void EnableGravity(bool a_enable)
	{
		if (a_enable)
		{
			m_rigidbody.gravityScale = 1.0f;
		}
		else
		{
			m_rigidbody.gravityScale = 0.0f;
		}
	}
}

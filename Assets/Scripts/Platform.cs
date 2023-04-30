using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class Platform : MonoBehaviour
{
	[SerializeField] PolygonCollider2D m_collider;
	[SerializeField] SpriteRenderer m_renderer;
	[SerializeField] float m_breakDelay = 0.2f;
	[SerializeField] float m_damage = 1.0f;

	[SerializeField] bool m_breakable;

	Vector2[] m_colliderPoints = new Vector2[4];

	private void Start()
	{
		UpdateCollider();
	}

	public void SetPlatformSprite(Sprite sprite, bool breakable, float damage)
	{
		m_damage = damage;

		m_breakable = breakable;
		m_renderer.color = breakable ? Color.red : Color.white;

		// Update the platform sprite to match the collider shape
		m_renderer.sprite = sprite;

		// Update collider to match sprite size
		UpdateCollider();
	}

	void UpdateCollider()
	{
		Sprite sprite = m_renderer.sprite;

		Vector2 halfSize = m_renderer.size * 0.5f;

		m_colliderPoints[0] = new Vector2(halfSize.x, halfSize.y);
		m_colliderPoints[1] = new Vector2(halfSize.x, -halfSize.y);
		m_colliderPoints[2] = new Vector2(-halfSize.x, -halfSize.y);
		m_colliderPoints[3] = new Vector2(-halfSize.x, halfSize.y);

		if (sprite.GetPhysicsShapeCount() > 0)
		{
			m_collider.SetPath(0, m_colliderPoints);
		}
	}

	public void OnPlayerHit()
	{
		if (m_breakable)
			StartCoroutine(DoDestroy());
	}

	IEnumerator DoDestroy()
	{
		yield return new WaitForSeconds(m_breakDelay);
		Destroy(gameObject);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		collision.transform.root.BroadcastMessage("OnPlatformLand", this, SendMessageOptions.DontRequireReceiver);
		collision.transform.root.BroadcastMessage("ChangeHealthBy", -m_damage, SendMessageOptions.DontRequireReceiver);
	}
}
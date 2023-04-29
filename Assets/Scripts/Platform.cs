using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Platform : MonoBehaviour
{
	[SerializeField] PolygonCollider2D m_collider;
	[SerializeField] SpriteRenderer m_renderer;

	Vector2[] m_colliderPoints = new Vector2[4];

	private void Start()
	{
		UpdateCollider();
	}

	public void SetPlatformSprite(Sprite sprite)
	{
		// Update the platform sprite to match the collider shape
		m_renderer.sprite = sprite;

		// Update collider to match sprite size
		UpdateCollider();
	}

	public int asd;
#if UNITY_EDITOR
	private void OnValidate()
	{
		UpdateCollider();
	}
#endif

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

	public void DestroyPlatform(float delay)
	{
		StartCoroutine(DoDestroy(delay));
	}

	IEnumerator DoDestroy(float delay)
	{
		yield return new WaitForSeconds(delay);
		Destroy(gameObject);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		collision.gameObject.BroadcastMessage("OnPlatformLand", this, SendMessageOptions.DontRequireReceiver);
	}
}

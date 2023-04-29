using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Platform : MonoBehaviour
{
	[SerializeField] private Sprite[] m_platformSprites;

	private PolygonCollider2D m_collider;

	[SerializeField]

	private void Start()
	{
		m_collider = GetComponent<PolygonCollider2D>();
		SetRandomShape();
	}

	private void SetRandomShape()
	{
		if (m_platformSprites.Length == 0)
			return;

		// Choose a random sprite from the array
		int randomIndex = Random.Range(0, m_platformSprites.Length);
		Sprite randomSprite = m_platformSprites[randomIndex];

		List<Vector2> points = new List<Vector2>();
		randomSprite.GetPhysicsShape(0, points);
		if (randomSprite.GetPhysicsShapeCount() > 0)
		{
			m_collider.SetPath(0, points);
		}

		// Update the platform sprite to match the collider shape
		GetComponent<SpriteRenderer>().sprite = randomSprite;
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

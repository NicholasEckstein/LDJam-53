using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlaceholderWorldGenerator : MonoBehaviour
{
	[Header("References")]
	[SerializeField] Platform m_platformPrefab;
	[SerializeField] Sprite[] m_platformSprites;

	[Header("Settings")]
	[SerializeField] bool m_generatePlatforms = true;
	[Space]
	[SerializeField] float m_minVerticalDistanceBetweenPlatforms;
	[SerializeField] float m_maxVerticalDistanceBetweenPlatforms;
	[Space]
	[SerializeField] bool m_breakablePlatforms;
	[SerializeField] BoxCollider2D m_collider;
	[SerializeField] float m_platformDamage;
	[Space]
	[SerializeField] bool m_clampCameraX;
	[SerializeField] bool m_clampCameraY;

	private void Start()
	{
		float halfWidth = m_collider.size.x * 0.5f;

		if (m_generatePlatforms)
		{
			for (float yPos = 0.0f; yPos >= -m_collider.size.y; yPos -= Random.Range(m_minVerticalDistanceBetweenPlatforms, m_maxVerticalDistanceBetweenPlatforms))
			{
				Platform platform = Instantiate(m_platformPrefab);

				if (m_platformSprites.Length == 0)
					return;

				// Choose a random sprite from the array
				int randomIndex = Random.Range(0, m_platformSprites.Length);
				Sprite randomSprite = m_platformSprites[randomIndex];

				platform.SetPlatformSprite(randomSprite, m_breakablePlatforms, m_platformDamage);

				platform.transform.position =
					transform.position +
					new Vector3(m_collider.offset.x, m_collider.offset.y) +
					new Vector3(0.0f, m_collider.size.y * 0.5f) +
					new Vector3(Random.Range(-1.0f, 1.0f) * halfWidth, yPos);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		collision.gameObject.BroadcastMessage(
			"OnEnteredRegion",
			new CameraConstraintData
			{
				rect = new Rect(
					(Vector2)transform.position - m_collider.size * 0.5f,
					m_collider.size),
				xClamp = m_clampCameraX,
				yClamp = m_clampCameraY
			},
			SendMessageOptions.DontRequireReceiver);
	}
}
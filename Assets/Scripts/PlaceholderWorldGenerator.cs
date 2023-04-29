using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceholderWorldGenerator : MonoBehaviour
{
	[Header("References")]
	[SerializeField] GameObject m_platformPrefab;

	[Header("Settings")]
	[SerializeField] float m_minVerticalDistanceBetweenPlatforms;
	[SerializeField] float m_maxVerticalDistanceBetweenPlatforms;
	[Space]
	[SerializeField] float m_levelWidth;
	[SerializeField] float m_levelHeight;

	private void Start()
	{
		float yPos = 0.0f;

		for (; yPos >= -m_levelHeight;)
		{
			GameObject platform = Instantiate(m_platformPrefab);
			platform.transform.position = new Vector3(
				Random.Range(-0.5f, 0.5f) * m_levelWidth,
				yPos,
				0.0f);

			yPos -= Random.Range(m_minVerticalDistanceBetweenPlatforms, m_maxVerticalDistanceBetweenPlatforms);
		}
	}
}

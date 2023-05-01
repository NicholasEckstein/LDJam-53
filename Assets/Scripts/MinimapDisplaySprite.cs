using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapDisplaySprite : MonoBehaviour
{
	[SerializeField] SpriteRenderer m_myRenderer;
	[SerializeField] SpriteRenderer m_targetRendererToCopy;

	private void Update()
	{
		m_myRenderer.sprite = m_targetRendererToCopy.sprite;
	}
}

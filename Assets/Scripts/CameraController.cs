using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] Transform m_target;
	[SerializeField] bool m_clampCameraToTarget;
	[SerializeField] AnimationCurve m_followSpeedByDistance;
	[SerializeField] Vector3 m_offset;
	[SerializeField] bool m_clampX;

	private void FixedUpdate()
	{
		if (m_target == null)
			return;

		Vector3 newPos;
		if (m_clampCameraToTarget)
		{
			newPos = m_target.position + m_offset;
		}
		else
		{
			float distance = Vector3.Distance(transform.position, m_target.position);
			newPos = Vector3.Lerp(
			   transform.position,
			   m_target.position + m_offset,
			   m_followSpeedByDistance.Evaluate(distance) * Time.deltaTime);
		}
		if (m_clampX)
		{
			newPos.x = 0.0f;
		}

		transform.position = newPos;
	}

	public void SetTarget(Transform a_target)
	{
		m_target = a_target;
	}
}
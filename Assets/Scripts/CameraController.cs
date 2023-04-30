using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CameraConstraintData
{
	public Rect rect;
	public bool xClamp, yClamp;
}

public class CameraController : MonoBehaviour
{
	[SerializeField] Camera m_camera;
	[SerializeField] Transform m_target;
	[SerializeField] bool m_clampCameraToTarget;
	[SerializeField] AnimationCurve m_followSpeedByDistance;
	[SerializeField] Vector3 m_offset;
	[SerializeField] float m_viewWidth;

	[Space]
	[SerializeField] bool m_clampX;
	[SerializeField] float m_xToClampTo;

	[Space]
	[SerializeField] bool m_clampY;
	[SerializeField] float m_yToClampTo;

	private void Start()
	{
		m_camera.orthographicSize = (m_viewWidth / m_camera.aspect) * 0.5f;
	}

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
			newPos.x = m_xToClampTo;
		if (m_clampY)
			newPos.y = m_yToClampTo;

		transform.position = newPos;
	}

	public void SetTarget(Transform a_target)
	{
		m_target = a_target;
	}
}
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
	[SerializeField] bool m_clampX;
	[SerializeField] bool m_clampY;
	[SerializeField] Rect m_cameraConstraints;

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

		//Vector2 cameraHalfSize = new Vector2(
		//	Mathf.Clamp(m_camera.orthographicSize * m_camera.aspect, float.MaxValue, m_cameraConstraints.size.x),
		//	Mathf.Clamp(m_camera.orthographicSize, 0.0f, m_cameraConstraints.size.y));

		//newPos.x = Mathf.Clamp(newPos.x,
		//	m_cameraConstraints.xMin + cameraHalfSize.x,
		//	m_cameraConstraints.xMax - cameraHalfSize.x);

		//newPos.y = Mathf.Clamp(newPos.y,
		//	m_cameraConstraints.yMin + cameraHalfSize.y,
		//	m_cameraConstraints.yMax - cameraHalfSize.y);

		//if (m_clampX)
		//{
		//	newPos.x = m_cameraConstraints.center.x;
		//}
		//if (m_clampY)
		//{
		//	newPos.y = m_cameraConstraints.center.y;
		//}

		transform.position = newPos;
	}

	public void SetTarget(Transform a_target)
	{
		m_target = a_target;
	}

	public void OnTargetEnteredRegion(CameraConstraintData cameraConstraintData)
	{
		m_cameraConstraints = cameraConstraintData.rect;
		m_clampX = cameraConstraintData.xClamp;
		m_clampY = cameraConstraintData.yClamp;
	}
}
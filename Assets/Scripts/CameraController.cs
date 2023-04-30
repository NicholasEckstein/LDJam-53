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

	[Header("Camera Shake Values")]
	[SerializeField] Transform shakyCamera = null;
	[SerializeField] float shakeFrequency = 15;
	[SerializeField] float maxAngle = 3;
	[SerializeField, Range(0.001f, 1)] float maxOffset = 1.5f;
	[SerializeField] bool debugMode = false;

	float camShakeValue = 0;
	float shake, seed;

	bool debugShake;

	private void Awake()
	{
		seed = UnityEngine.Random.value;
	}

	private void Start()
	{
		m_camera.orthographicSize = (m_viewWidth / m_camera.aspect) * 0.5f;
	}

	private void Update()
	{
		if (debugMode)
		{
			CheckInput();
		}
	}

	private void FixedUpdate()
	{
		if (m_target == null)
			return;

		if (debugMode)
		{
			if (debugShake)
				AddTrauma(.25f, 1);
		}

		CameraShake(out Vector2 shakeOffset, out Vector3 shakeRotate);

		transform.eulerAngles = shakeRotate;

		Vector3 newPos;
		Vector3 goalPos = m_target.position + m_offset;

		if (m_clampX)
			goalPos.x = m_xToClampTo;
		if (m_clampY)
			goalPos.y = m_yToClampTo;

		if (m_clampCameraToTarget)
		{
			newPos = goalPos;
		}
		else
		{
			float distance = Vector3.Distance(transform.position, m_target.position);
			newPos = Vector3.Lerp(
			   transform.position,
			   goalPos,
			   m_followSpeedByDistance.Evaluate(distance) * Time.deltaTime);
		}

		newPos.z = -10.0f;
		transform.position = newPos + (Vector3)shakeOffset;
	}

	public void SetTarget(Transform a_target)
	{
		m_target = a_target;
	}

	//intensity must be between 0 and 1
	public void AddTrauma(float duration, float intensityScale, float frequency = int.MinValue, float maxRotAngle = int.MinValue, float maxPosOffset = int.MinValue)
	{
		camShakeValue = duration;

		shake = intensityScale; //could normalize this

		if (frequency != int.MinValue)
			shakeFrequency = frequency;
		if (maxRotAngle != int.MinValue)
			maxAngle = maxRotAngle;
		if (maxPosOffset != int.MinValue)
			maxOffset = maxPosOffset;
	}

	void CameraShake(out Vector2 shakeOffset, out Vector3 rotate)
	{
		if (camShakeValue > 0)
		{
			//Noise calculations
			float angle = maxAngle * shake * (Mathf.PerlinNoise(seed, Time.time * shakeFrequency) * 2 - 1);
			float xOffset = maxOffset * shake * (Mathf.PerlinNoise(seed + 1, Time.time * shakeFrequency) * 2 - 1);
			float yOffset = maxOffset * shake * (Mathf.PerlinNoise(seed + 2, Time.time * shakeFrequency) * 2 - 1);

			//Translational shake
			shakeOffset = new Vector3(xOffset, yOffset, 0);

			//Rotational shake
			rotate = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, angle);

			camShakeValue -= Time.fixedDeltaTime;
		}
		else
		{
			shakeOffset = Vector3.zero;
			rotate = Vector3.zero;
		}
	}

	void CheckInput()
	{
		debugShake = Input.GetKeyDown(KeyCode.S);
	}
}
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Reaper : MonoBehaviour
{
	[SerializeField] Animator m_animator;
	[SerializeField] Collider2D m_collider;
	[SerializeField] float m_speed;
	[SerializeField] float m_attackCooldown;
	[SerializeField] float m_damage;
	[SerializeField] float m_knockback;
	[SerializeField] float m_knockbackExtraUpward;
	[SerializeField] float m_spawnScreenShakeDuration = 2.0f;
	[SerializeField] float m_spawnScreenShakeIntensity = 0.75f;

	Vector3 scythOffsetPos = new Vector3(1.0f, 0.0f);

	float m_timeUntilNextAttack = 0.0f;

	List<Collider2D> m_colsHitThisSwing = new List<Collider2D>();

	private void OnEnable()
	{
		GameManager.Instance.CameraController.AddTrauma(m_spawnScreenShakeDuration, m_spawnScreenShakeIntensity);
		AudioManager.Instance.PlaySFX(GameManager.Instance.ReaperHowlSFX);
	}

	private void Update()
	{
		Vector3 playerPos = GameManager.Instance.PlayerController.transform.position;

		float flip = playerPos.x - transform.position.x < 0.0f ? -1.0f : 1.0f;

		Vector2 goalPos = playerPos - scythOffsetPos * flip;
		transform.position = Vector3.MoveTowards(transform.position, goalPos, m_speed * Time.deltaTime);

		if (m_timeUntilNextAttack <= 0.0f && Vector2.Distance(transform.position, goalPos) < 1.0f)
		{
			m_timeUntilNextAttack = m_attackCooldown;

			m_colsHitThisSwing.Clear();
			m_animator.SetTrigger("Attack");
		}
		else
		{
			m_timeUntilNextAttack -= Time.deltaTime;
		}

		transform.localScale = new Vector3(
			Mathf.Abs(transform.localScale.x) * flip,
			transform.localScale.y,
			transform.localScale.z);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (!m_colsHitThisSwing.Contains(collision))
		{
			m_colsHitThisSwing.Add(collision);
			collision.transform.root.BroadcastMessage("ChangeHealthBy", -m_damage, SendMessageOptions.DontRequireReceiver);

			Vector2 colPos = transform.position + (Vector3)m_collider.offset;
			Vector2 baseKnockback = ((Vector2)collision.transform.position - colPos).normalized * m_knockback;
			baseKnockback.y += m_knockbackExtraUpward;
			collision.transform.root.BroadcastMessage("OnKnockback", baseKnockback, SendMessageOptions.DontRequireReceiver);
		}
	}
}
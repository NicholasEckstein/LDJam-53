using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
	public static Action OnTakeDamage;
	public static Action OnDead;
	public static Action OnHealed;

	[SerializeField] float m_maxHealth;
	[SerializeField] float m_currentHealth;

	[SerializeField] float m_secondsOfInvincibleAfterHurt = 1.0f;

	public float MaxHealth { get => m_maxHealth; }
	public float CurrentHealth { get => m_currentHealth; }
	public float SecondsOfInvincibleAfterHurt { get => m_secondsOfInvincibleAfterHurt; }

	bool m_canTakeDamage = true;
	Coroutine m_currentInvinceRoutine = null;

	private void Awake()
	{
		m_currentHealth = m_maxHealth;
	}

	public float ChangeHealthBy(float amount) => ChangeHealthBy(amount, true);
	public float ChangeHealthBy(float amount, bool notify)
	{
		return SetHealthTo(m_currentHealth + amount, notify);
	}

	public float SetHealthTo(float newHealth, bool notify)
	{
		float oldHealth = m_currentHealth;
		m_currentHealth = newHealth;

		if (notify)
		{
			if (newHealth < oldHealth)//Took Damage
			{
				OnTakeDamage?.Invoke();
				//BroadcastMessage("OnTakeDamage", this, SendMessageOptions.DontRequireReceiver);

				if (m_currentInvinceRoutine != null)
					StopCoroutine(m_currentInvinceRoutine);
				m_currentInvinceRoutine = StartCoroutine(DoInvincible());

				if (newHealth <= 0.0f)
				{
					AudioManager.Instance.PlaySFX(GameManager.Instance.PlayerDeadSFX);
					OnDead?.Invoke();
					//BroadcastMessage("OnDead", this, SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					AudioManager.Instance.PlaySFX(GameManager.Instance.PlayerHitSFX);
				}
			}
			else if (newHealth > oldHealth)//Healed
			{
				OnHealed?.Invoke();
				//BroadcastMessage("OnHealed", this, SendMessageOptions.DontRequireReceiver);
			}
		}

		return newHealth;
	}

	IEnumerator DoInvincible()
	{
		m_canTakeDamage = false;
		yield return new WaitForSeconds(m_secondsOfInvincibleAfterHurt);
		m_canTakeDamage = true;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
	[SerializeField] float m_maxHealth;
	float m_currentHealth;

	[SerializeField] float m_secondsOfInvincibleAfterHurt = 1.0f;

	public float MaxHealth { get => m_maxHealth; }
	public float CurrentHealth { get => m_currentHealth; }

	bool m_canTakeDamage = true;
	Coroutine m_currentInvinceRoutine = null;

	private void Awake()
	{
		m_currentHealth = m_maxHealth;
	}

	public float ChangeHealthBy(float amount, bool notify)
	{
		return SetHealthTo(m_currentHealth + amount, notify);
	}

	public float SetHealthTo(float amount, bool notify)
	{
		float newHealth = amount;

		if (notify)
		{
			if (newHealth < m_currentHealth)//Took Damage
			{
				BroadcastMessage("OnTakeDamage", this, SendMessageOptions.DontRequireReceiver);

				if (m_currentInvinceRoutine != null)
					StopCoroutine(m_currentInvinceRoutine);
				m_currentInvinceRoutine = StartCoroutine(DoInvincible());

				if (newHealth <= 0.0f)
					BroadcastMessage("OnDead", this, SendMessageOptions.DontRequireReceiver);
			}
			else if (newHealth > m_currentHealth)//Healed
			{
				BroadcastMessage("OnHealed", this, SendMessageOptions.DontRequireReceiver);
			}
		}

		m_currentHealth = newHealth;
		return newHealth;
	}

	IEnumerator DoInvincible()
	{
		m_canTakeDamage = false;
		yield return new WaitForSeconds(m_secondsOfInvincibleAfterHurt);
		m_canTakeDamage = true;
	}
}

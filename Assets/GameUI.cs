using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : UIPrefab
{
    [SerializeField]
    private TMPro.TMP_Text m_timerText;

    [SerializeField]
    private List<Image> m_hearts;

    [SerializeField]
    private Sprite m_damagedHeartSprite;
    private bool m_timerActive;
    private float m_timer;

    private void OnEnable()
    {
        Health.OnTakeDamage += OnTakeDamage;
    }

    private void OnDisable()
    {
        Health.OnTakeDamage -= OnTakeDamage;
    }

    private void Update()
    {
        if (m_timerActive)
        {
            if (m_timer > 0)
            {
                m_timer -= Time.deltaTime;
                var span = TimeSpan.FromSeconds(m_timer);
                m_timerText.text = span.TotalSeconds.ToString();
            }
            else
            {
                m_timerActive = false;

                //TODO: LOSE STATE HERE
            }
        }
    }

    public void Init()
    {
        EnableTimer(false);
    }

    public void OnTakeDamage()
    {
        for (int i = m_hearts.Count - 1; i >= 0; i--)
        {
            if (m_hearts[i] == null)
                continue;

            if (m_hearts[i].sprite != m_damagedHeartSprite)
            {
                m_hearts[i].sprite = m_damagedHeartSprite;
            }
        }
    }

    public void EnableTimer(bool a_enable)
    {
        m_timerText.gameObject.SetActive(a_enable);
        m_timer = GameManager.Instance.CurrentLevel.TimeToAscend;
        m_timerActive = true;
    }
}

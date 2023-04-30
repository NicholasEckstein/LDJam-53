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
        var currHealth = GameManager.Instance.PlayerController.Health.CurrentHealth - 1;
        for (int i = 0; i < m_hearts.Count; i++)
        {
            if (m_hearts[i] == null)
                continue;

            if(i >= currHealth)
            {
                m_hearts[i].sprite = null;
                var c = m_hearts[i].color;
                c.a = 0;
                m_hearts[i].color = c;
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

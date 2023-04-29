using System;
using System.Collections;
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
        
    }

    public void Init()
    {
        EnableTimer(false);
    }

    public void OnTakeDamage()
    {
        for (int i = m_hearts.Count - 1; i >= 0; i++)
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
    }
}

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
    private Sprite m_heartSprite;
    private bool m_timerActive;
    private Coroutine m_loseCR;
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
        if (m_loseCR != null)
            return;

        if (m_timerActive)
        {
            if (m_timer > 0)
            {
                m_timer -= Time.deltaTime;
                if (m_timer < 0)
                    m_timer = 0;

                var span = TimeSpan.FromSeconds(m_timer);

                var str = span.TotalSeconds.ToString();
                var strArr = str.Split(".");
                m_timerText.text = strArr[0];
            }
            else
            {
                m_timerActive = false;
                
                if(m_loseCR == null)
                    m_loseCR = StartCoroutine(LoseCR());
            }
        }
    }

    public void Init()
    {
        EnableTimer(false);
        StopTimer();
    }

    public void OnTakeDamage()
    {
        var currHealth = GameManager.Instance.PlayerController.Health.CurrentHealth;
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
    }

    public void StartTimer()
    {
        m_timerActive = true;
        if (m_timerActive)
        {
            m_timer = GameManager.Instance.CurrentLevel.TimeToAscend;
        }
    }

    public void StopTimer()
    {
        m_timerActive = false;
    }


    public void ResetHeartUI()
    {
        for (int i = 0; i < m_hearts.Count; i++)
        {
            m_hearts[i].sprite = m_heartSprite;
            var c = m_hearts[i].color;
            c.a = 1;
            m_hearts[i].color = c;
        }
    }

    private IEnumerator LoseCR()
    {
        yield return new WaitForSeconds(1f);
        LoadingUI.ShowLoadingScreen();
        GameManager.Instance.SetNextPhase(new PostGamePhase(false));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : UIPrefab
{
    [SerializeField]
    private float m_pulseTime;

    [SerializeField]
    private TMPro.TMP_Text m_playText;

    private bool m_pulseUp = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if(GameManager.Instance.NextPhase is not LevelSelectPhase)
            {
                LoadingUI.ShowLoadingScreen();
                GameManager.Instance.SetNextPhase(new LevelSelectPhase());
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        if (!m_pulseUp)
        {
            m_playText.alpha -= Time.deltaTime * m_pulseTime;

            if(m_playText.alpha <= 0)
            {
                m_pulseUp = true;
            }
        }
        else
        {
            m_playText.alpha += Time.deltaTime * m_pulseTime;
            if (m_playText.alpha >= 1)
            {
                m_pulseUp = false;
            }
        }
    }
}

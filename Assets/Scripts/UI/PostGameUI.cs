using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostGameUI : UIPrefab
{
    [SerializeField]
    private TMPro.TMP_Text m_headerText;

    [SerializeField]
    private string m_winHeader;

    [SerializeField]
    private string m_loseHeader;

    [SerializeField]
    private Transform m_winBG;

    [SerializeField]
    private Transform m_loseBG;

    [SerializeField]
    private Transform m_exitText;

    bool m_win;
    private bool m_canExit;

    public void Init(bool a_win)
    {
        m_exitText.gameObject.SetActive(false);
        m_canExit = false;
        m_win = a_win;
        m_headerText.text = m_win ? m_winHeader : m_loseHeader;

        if(m_win)
        {
            m_loseBG.gameObject.SetActive(false);
            m_winBG.gameObject.SetActive(true);
        }
        else
        {
            m_loseBG.gameObject.SetActive(true);
            m_winBG.gameObject.SetActive(false);
        }

        StartCoroutine(ExitCR());
    }

    private void Update()
    {
        if (!m_canExit)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadingUI.ShowLoadingScreen();
            GameManager.Instance.SetNextPhase(new MainMenuPhase(GameManager.Instance.CurrentPhase));
        }
    }

    private IEnumerator ExitCR()
    {
        yield return new WaitForSeconds(2f);
        m_canExit = true;
        m_exitText.gameObject.SetActive(true);
    }
}

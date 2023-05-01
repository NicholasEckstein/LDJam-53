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
    private Transform m_continueText;

    [SerializeField]
    private Transform m_exitText;

    bool m_win;
    bool m_playFinalDialgue = false;
    private bool m_canExit;

    public void Init(bool a_win)
    {
        m_exitText.gameObject.SetActive(false);
        m_continueText.gameObject.SetActive(false);
        m_canExit = false;
        m_win = a_win;

        var l1 = PlayerPrefs.GetInt(GameManager.Level1Str, -1);
        var l2 = PlayerPrefs.GetInt(GameManager.Level2Str, -1);
        if(l1 == GameManager.LevelComplete && l2 == GameManager.LevelComplete)
        {
            m_playFinalDialgue = true;
        }

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

        if (m_playFinalDialgue && m_win)
        {
            GameManager.Instance.DialogueRunner.StartDialogue(GameManager.Instance.FinaleDialogueNode);
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

        if (!m_playFinalDialgue && Input.GetKeyDown(KeyCode.Return))
        {
            //Enter loads level 2
            LoadingUI.ShowLoadingScreen();
            GameManager.Instance.SetNextPhase(new PlayPhase(1));
        }
    }

    private IEnumerator ExitCR()
    {
        yield return null;
        var gameMan = GameManager.Instance;
        yield return new WaitUntil(() => !gameMan.DialogueRunner.IsDialogueRunning);

        yield return new WaitForSeconds(1f);

        m_canExit = true;
        m_exitText.gameObject.SetActive(true);

        if(!m_playFinalDialgue)
            m_continueText.gameObject.SetActive(true);
    }
}

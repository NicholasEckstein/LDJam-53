using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPhase : GamePhase
{
    private bool m_initialized;
    private GameUI m_gameUI;
    private int m_levelIndex;
    private PauseMenuUI m_pauseMenuUI;

    public PlayPhase(int a_levelIndex)
    {
        m_levelIndex = a_levelIndex;
    }

    public override bool Initialize()
    {
        if(GameManager.Instance.PlayerController == null)
            GameManager.Instance.CreatePlayer();

        if (GameManager.Instance.CurrentLevel == null)
            GameManager.Instance.LoadLevel(m_levelIndex);

        if (m_gameUI == null)
        {
            m_gameUI = GameManager.OpenUI<GameUI>(GameManager.Instance.GameUI);
            if (m_gameUI != null)
            {
                m_gameUI.Init();
            } 
        }

        if (!LoadingUI.IsClosing)
        {
            LoadingUI.CloseLoadingScreen(GameManager.Instance.LoadingScreenDelayTime, () =>
            {
                AudioManager.Instance.PlayMusic(GameManager.Instance.DescentMusic);
                m_initialized = true;
            });
        }

        return m_initialized;
    }

    public override void Start()
    {
       GameManager.Instance.PlayerController.EnableGravity(true);
    }

    public override bool Uninitialize()
    {
        if (GameManager.Instance.PlayerController != null)
            GameManager.Instance.DestroyPlayer();

        if (GameManager.Instance.CurrentLevel != null)
            GameManager.Instance.DestroyLevel();

        GameManager.CloseUI(m_gameUI);
        m_gameUI = null;
        GameManager.CloseUI(m_pauseMenuUI);
        m_pauseMenuUI = null;
        return true;
    }

    public override void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.TogglePaused())
            {
                if (m_pauseMenuUI == null)
                    m_pauseMenuUI = GameManager.OpenUI<PauseMenuUI>(GameManager.Instance.PauseMenuUI);
            }
        }
    }

    public void EnableTimer()
    {
        m_gameUI.EnableTimer(true);
    }

    public void ReloadLevel()
    {
        GameManager.Instance.DestroyLevel();
        GameManager.Instance.LoadLevel(m_levelIndex);
        m_gameUI.ResetHeartUI();
    }
}

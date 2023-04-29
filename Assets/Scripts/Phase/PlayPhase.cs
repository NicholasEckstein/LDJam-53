using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPhase : GamePhase
{
    private bool m_initialized;
    private GameUI m_gameUI;

    public override bool Initialize()
    {
        if(GameManager.Instance.PlayerController == null)
            GameManager.Instance.CreatePlayer();

        m_gameUI = GameManager.OpenUI<GameUI>(GameManager.Instance.GameUI);
        if(m_gameUI != null)
        {
            m_gameUI.Init();
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
        //TODO: some kind of delay or *player input* before player starts falling
        GameManager.Instance.PlayerController.EnableGravity(true);
    }

    public override bool Uninitialize()
    {
        if (GameManager.Instance.PlayerController != null)
            GameManager.Instance.DestroyPlayer();

        GameManager.CloseUI(m_gameUI);

        return true;
    }

    public override void Update()
    {
    }
}

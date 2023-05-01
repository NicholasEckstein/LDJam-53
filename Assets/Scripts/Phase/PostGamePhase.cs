using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostGamePhase : GamePhase
{
    private bool m_win;
    private bool m_initialized;
    private PostGameUI m_postGameUI;

    public PostGamePhase(bool a_win)
    {
        m_win = a_win;
    }

    public override bool Initialize()
    {
        if (m_postGameUI == null)
        {
            m_postGameUI = GameManager.OpenUI<PostGameUI>(GameManager.Instance.PostGameUI);
            if (m_postGameUI != null)
            {
                m_postGameUI.Init(m_win);
            }
        }

        if (!LoadingUI.IsClosing)
        {
            LoadingUI.CloseLoadingScreen(GameManager.Instance.LoadingScreenDelayTime, () =>
            {
                m_initialized = true;
            });
        }

        return m_initialized;
    }

    public override void Start()
    {
        var gmgr = GameManager.Instance;
        var clip = m_win ? gmgr.WinMusic : gmgr.LoseMusic;
        AudioManager.Instance.PlayMusic(clip);
    }

    public override bool Uninitialize()
    {
        if(m_postGameUI != null)
        {
            GameManager.CloseUI(m_postGameUI);
            m_postGameUI = null;
        }
        return true;
    }

    public override void Update()
    {
    }
}

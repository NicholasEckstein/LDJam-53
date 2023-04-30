using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectPhase : GamePhase
{
    private LevelSelectUI m_levelSelectUI;
    private bool m_initialized;

    public override bool Initialize()
    {
        if (!LoadingUI.IsClosing)
        {
            LoadingUI.CloseLoadingScreen(GameManager.Instance.LoadingScreenDelayTime, () =>
            {
                m_levelSelectUI = GameManager.OpenUI<LevelSelectUI>(GameManager.Instance.LevelSelect);
                m_initialized = true;
            });
        }

        return m_initialized;
    }

    public override void Start()
    {
    }

    public override bool Uninitialize()
    {
        GameManager.CloseUI(m_levelSelectUI);
        m_levelSelectUI = null;
        return true;
    }

    public override void Update()
    {
    }
}
using System.Collections;
using UnityEngine;

public class MainMenuPhase : GamePhase
{
    private bool m_initialized = false;
    private MainMenuUI m_mainMenuUI = null;
    private GamePhase m_previousPhase;

    public MainMenuPhase(GamePhase a_prevPhase)
    {
        m_previousPhase = a_prevPhase;
    }

    //Will keep running if false
    public override bool Initialize()
    {
        if(m_previousPhase != null && m_previousPhase is LevelSelectPhase || m_previousPhase is PlayPhase)
        {
            if (!LoadingUI.IsClosing)
            {
                LoadingUI.CloseLoadingScreen(GameManager.Instance.LoadingScreenDelayTime, () =>
                {
                    InitMainMenu();
                });
            }
        }
        else
        {
            InitMainMenu();
        }


        return m_initialized;
    }

    private void InitMainMenu()
    {
        if (m_mainMenuUI == null)
            m_mainMenuUI = GameManager.OpenUI<MainMenuUI>(GameManager.Instance.MainMenuUI);

        AudioManager.Instance.PlayMusic(GameManager.Instance.MainMenuMusic);
        m_initialized = true;
    }

    public override void Start()
    {
    }

    public override bool Uninitialize()
    {
        GameManager.CloseUI(m_mainMenuUI);
        m_mainMenuUI = null;
        return true;
    }

    public override void Update()
    {
    }
}
